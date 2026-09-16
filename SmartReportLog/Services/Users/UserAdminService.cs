using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartReportLog.Entity.Identity;
using SmartReportLog.Model.Users;
using SmartReportLog.Persistance;

namespace SmartReportLog.Services.Users
{
    public sealed class UserAdminService : IUserAdminService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SmartLogContext _context;
        private readonly IHttpContextAccessor _http;

        public UserAdminService(
            UserManager<AppUser> userManager,
            SmartLogContext context,
            IHttpContextAccessor http)
        {
            _userManager = userManager;
            _context = context;
            _http = http;
        }

        private string? CurrentUserId =>
            _userManager.GetUserId(_http.HttpContext?.User!);

        public async Task<List<UserListItemDto>> GetUsersAsync(CancellationToken ct)
        {
            var users = await _userManager.Users
                .AsNoTracking()
                .OrderBy(u => u.UserName)
                .ToListAsync(ct);

            var list = new List<UserListItemDto>(users.Count);

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                list.Add(new UserListItemDto(
                                                      user.Id,
                                                      user.UserName ?? "-",
                                                      user.FullName,
                                                      roles.FirstOrDefault() ?? AppRoles.Viewer,
                                                      user.StateCode,
                                                      user.StateName,
                                                      user.LockoutEnd is not null && user.LockoutEnd > DateTimeOffset.Now,
                                                      user.CreatedAt));
            }

            return list;
        }
        public async Task<List<StateOptionDto>> GetStateOptionsAsync(CancellationToken ct)
        {
            var rows = await _context.Atms.AsNoTracking()
                .Where(a => a.StateCode != null && a.StateName != null)
                .GroupBy(a => new { Code = a.StateCode!.Value, Name = a.StateName! })
                .Select(g => new { g.Key.Code, g.Key.Name, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToListAsync(ct);

            return rows.Select(x => new StateOptionDto(x.Code, x.Name, x.Count)).ToList();
        }

        public async Task<UserActionResult> CreateAsync(
    string userName, string? fullName, string password,
    string role, int? stateCode, CancellationToken ct)
        {
            userName = userName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userName))
                return new(false, "نام کاربری الزامی است.");

            if (string.IsNullOrWhiteSpace(password))
                return new(false, "رمز عبور الزامی است.");

            if (!AppRoles.All.Contains(role))
                return new(false, "نقش انتخاب‌شده معتبر نیست.");

            if (await _userManager.FindByNameAsync(userName) is not null)
                return new(false, "این نام کاربری قبلاً ثبت شده است.");

            var stateName = await ResolveStateNameAsync(stateCode, ct);

            if (stateCode.HasValue && stateName is null)
                return new(false, "استان انتخاب‌شده معتبر نیست.");

            // مدیر سیستم نباید محدود به استان شود
            if (role == AppRoles.Admin && stateCode.HasValue)
                return new(false, "کاربر مدیر نمی‌تواند محدود به یک استان باشد.");

            var user = new AppUser
            {
                UserName = userName,
                FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim(),
                StateCode = stateCode,
                StateName = stateName,
                EmailConfirmed = true,
                LockoutEnabled = true
            };
            var created = await _userManager.CreateAsync(user, password);
            if (!created.Succeeded)
                return new(false, Describe(created));

            var assigned = await _userManager.AddToRoleAsync(user, role);
            if (!assigned.Succeeded)
            {
                // اگر نقش تخصیص نیافت، کاربر ناقص باقی نماند
                await _userManager.DeleteAsync(user);
                return new(false, Describe(assigned));
            }

            return new(true, $"کاربر {userName} ایجاد شد.");
        }

        public async Task<UserActionResult> UpdateAsync(
            string userId, string? fullName, string role,
            int? stateCode, CancellationToken ct)
        {
            if (!AppRoles.All.Contains(role))
                return new(false, "نقش انتخاب‌شده معتبر نیست.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return new(false, "کاربر یافت نشد.");

            if (role == AppRoles.Admin && stateCode.HasValue)
                return new(false, "کاربر مدیر نمی‌تواند محدود به یک استان باشد.");

            var stateName = await ResolveStateNameAsync(stateCode, ct);

            if (stateCode.HasValue && stateName is null)
                return new(false, "استان انتخاب‌شده معتبر نیست.");

            var currentRoles = await _userManager.GetRolesAsync(user);
            var currentRole = currentRoles.FirstOrDefault();

            if (currentRole == AppRoles.Admin && role != AppRoles.Admin && await IsLastAdmin(user.Id))
                return new(false, "این تنها حساب مدیر سیستم است و نقش آن قابل تغییر نیست.");

            bool scopeChanged = user.StateCode != stateCode;

            user.FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim();
            user.StateCode = stateCode;
            user.StateName = stateName;

            var updated = await _userManager.UpdateAsync(user);
            if (!updated.Succeeded)
                return new(false, Describe(updated));

            if (currentRole != role)
            {
                if (currentRoles.Count > 0)
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                var assigned = await _userManager.AddToRoleAsync(user, role);
                if (!assigned.Succeeded)
                    return new(false, Describe(assigned));
            }

            // با تغییر محدوده، نشست فعلی کاربر باید بازسازی شود
            if (scopeChanged || currentRole != role)
                await _userManager.UpdateSecurityStampAsync(user);

            return new(true, scopeChanged
                ? "تغییرات ذخیره شد. محدوده جدید تا حداکثر ۳۰ دقیقه یا با ورود مجدد کاربر اعمال می‌شود."
                : "تغییرات ذخیره شد.");
        }

        public async Task<UserActionResult> ResetPasswordAsync(
            string userId, string newPassword, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return new(false, "رمز عبور جدید الزامی است.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return new(false, "کاربر یافت نشد.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            return result.Succeeded
                ? new(true, "رمز عبور تغییر کرد.")
                : new(false, Describe(result));
        }

        public async Task<UserActionResult> SetLockAsync(string userId, bool locked, CancellationToken ct)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return new(false, "کاربر یافت نشد.");

            if (locked && user.Id == CurrentUserId)
                return new(false, "نمی‌توانید حساب خودتان را غیرفعال کنید.");

            if (locked && await IsLastAdmin(user.Id))
                return new(false, "این تنها حساب مدیر سیستم است و قابل غیرفعال کردن نیست.");

            await _userManager.SetLockoutEnabledAsync(user, true);

            var result = await _userManager.SetLockoutEndDateAsync(
                user, locked ? DateTimeOffset.MaxValue : null);

            return result.Succeeded
                ? new(true, locked ? "حساب غیرفعال شد." : "حساب فعال شد.")
                : new(false, Describe(result));
        }

        public async Task<UserActionResult> DeleteAsync(string userId, CancellationToken ct)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return new(false, "کاربر یافت نشد.");

            if (user.Id == CurrentUserId)
                return new(false, "نمی‌توانید حساب خودتان را حذف کنید.");

            if (await IsLastAdmin(user.Id))
                return new(false, "این تنها حساب مدیر سیستم است و قابل حذف نیست.");

            var result = await _userManager.DeleteAsync(user);

            return result.Succeeded
                ? new(true, "کاربر حذف شد.")
                : new(false, Describe(result));
        }

        /// <summary>بررسی می‌کند آیا این کاربر تنها مدیر باقی‌مانده است.</summary>
        private async Task<bool> IsLastAdmin(string userId)
        {
            var admins = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
            return admins.Count <= 1 && admins.Any(a => a.Id == userId);
        }

        private static string Describe(IdentityResult result)
            => string.Join(" ", result.Errors.Select(Translate));

        private static string Translate(IdentityError error) => error.Code switch
        {
            "PasswordTooShort" => "رمز عبور باید حداقل ۸ کاراکتر باشد.",
            "PasswordRequiresDigit" => "رمز عبور باید حداقل یک رقم داشته باشد.",
            "PasswordRequiresLower" => "رمز عبور باید حداقل یک حرف کوچک انگلیسی داشته باشد.",
            "PasswordRequiresUpper" => "رمز عبور باید حداقل یک حرف بزرگ انگلیسی داشته باشد.",
            "PasswordRequiresNonAlphanumeric" => "رمز عبور باید حداقل یک نویسه ویژه داشته باشد.",
            "DuplicateUserName" => "این نام کاربری قبلاً ثبت شده است.",
            "InvalidUserName" => "نام کاربری شامل نویسه‌های غیرمجاز است.",
            _ => error.Description
        };
        /// <summary>نام استان را از روی کد، از میان استان‌های موجود پیدا می‌کند.</summary>
        private async Task<string?> ResolveStateNameAsync(int? stateCode, CancellationToken ct)
        {
            if (!stateCode.HasValue) return null;

            return await _context.Atms.AsNoTracking()
                .Where(a => a.StateCode == stateCode.Value && a.StateName != null)
                .Select(a => a.StateName!)
                .FirstOrDefaultAsync(ct);
        }
    }
}