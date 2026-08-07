using Microsoft.Extensions.Options;
using SmartReportLog.Services.Sana;

namespace SmartReportLog.Endpoints
{
    /// <summary>
    /// احراز هویت ساده سرویس‌به‌سرویس با هدر X-Api-Key.
    /// روی گروه /api/sana اعمال می‌شود، نه روی کل برنامه.
    /// </summary>
    public sealed class ApiKeyEndpointFilter : IEndpointFilter
    {
        private const string HeaderName = "X-Api-Key";

        private readonly SanaStorageOptions _options;
        private readonly ILogger<ApiKeyEndpointFilter> _logger;
        private readonly string _requiredScope;

        public ApiKeyEndpointFilter(
            IOptions<SanaStorageOptions> options,
            ILogger<ApiKeyEndpointFilter> logger,
            string requiredScope = "read")
        {
            _options = options.Value;
            _logger = logger;
            _requiredScope = requiredScope;
        }

        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var http = context.HttpContext;

            if (!http.Request.Headers.TryGetValue(HeaderName, out var provided)
                || string.IsNullOrWhiteSpace(provided))
            {
                return Results.Json(new { success = false, message = "کلید دسترسی ارسال نشده است." },
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var client = _options.ApiKeys.FirstOrDefault(k => FixedTimeEquals(k.Key, provided!));

            if (client is null)
            {
                _logger.LogWarning("تلاش دسترسی با کلید نامعتبر از {Ip}",
                    http.Connection.RemoteIpAddress);

                return Results.Json(new { success = false, message = "کلید دسترسی معتبر نیست." },
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            if (!client.Scopes.Contains(_requiredScope, StringComparer.OrdinalIgnoreCase))
            {
                return Results.Json(new { success = false, message = "این کلید مجوز انجام عملیات را ندارد." },
                    statusCode: StatusCodes.Status403Forbidden);
            }

            http.Items["ApiClientName"] = client.Name;
            return await next(context);
        }

        /// <summary>مقایسه با زمان ثابت تا کلید از طریق تفاوت زمان پاسخ قابل حدس نباشد.</summary>
        private static bool FixedTimeEquals(string a, string b)
        {
            var x = System.Text.Encoding.UTF8.GetBytes(a);
            var y = System.Text.Encoding.UTF8.GetBytes(b);
            return x.Length == y.Length
                && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(x, y);
        }
    }
}