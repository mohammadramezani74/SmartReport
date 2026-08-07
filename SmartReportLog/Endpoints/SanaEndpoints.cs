using Microsoft.AspNetCore.Mvc;
using SmartReportLog.Model.Sana;
using SmartReportLog.Services.Sana;

namespace SmartReportLog.Endpoints
{
    public static class SanaEndpoints
    {
        public static void MapSanaEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/sana")
                           .AllowAnonymous()          // احراز هویت با API key انجام می‌شود، نه کوکی
                           .DisableAntiforgery();     // فراخوانی سرویس‌به‌سرویس است، نه فرم مرورگر

            // ------------------------------------------------------------
            // ۱) دریافت فایل زیپ توتال از سانا
            // POST /api/sana/total   (multipart/form-data, field: file)
            // ------------------------------------------------------------
            group.MapPost("/total", async (
                    IFormFile? file,
                    ISanaArchiveService service,
                    CancellationToken ct) =>
            {
                if (file is null || file.Length == 0)
                    return Results.BadRequest(new TotalUploadResponse(false, "فایلی ارسال نشده است."));

                var name = Path.GetFileName(file.FileName);

                if (!name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    return Results.BadRequest(new TotalUploadResponse(false, "فقط فایل zip پذیرفته می‌شود."));

                await using var stream = file.OpenReadStream();
                var result = await service.IngestAsync(stream, name, ct);

                return result.Success ? Results.Ok(result) : Results.BadRequest(result);
            })
              .AddEndpointFilter(new ApiKeyScope("upload"))
    .WithName("UploadSanaTotal")
    .WithSummary("دریافت فایل زیپ گزارش توتال")
    .WithDescription("فایل زیپ خروجی سانا را دریافت، اطلاعات تیکت و بازه را استخراج و ذخیره می‌کند.")
    .WithTags("گزارش توتال")
    .Accepts<IFormFile>("multipart/form-data")
    .Produces<TotalUploadResponse>(200)
    .Produces<TotalUploadResponse>(400)
    .ProducesProblem(401)
    .DisableAntiforgery();

            // ------------------------------------------------------------
            // ۲) لیست آرشیوها با فیلتر تاریخ / سریال / تیکت
            // GET /api/sana/total?from=&to=&serial=&ticket=&page=&pageSize=
            // ------------------------------------------------------------
            group.MapGet("/total", async (
                    [FromQuery] string? ticket,
                    [FromQuery] string? serial,
                    [FromQuery] string? from,
                    [FromQuery] string? to,
                    [FromQuery] int page,
                    [FromQuery] int pageSize,
                    ISanaArchiveService service,
                    CancellationToken ct) =>
            {
                DateOnly? fromDate = DateOnly.TryParse(from, out var f) ? f : null;
                DateOnly? toDate = DateOnly.TryParse(to, out var t) ? t : null;

                if (fromDate.HasValue && toDate.HasValue && toDate < fromDate)
                    return Results.BadRequest(new { success = false, message = "بازه تاریخ نامعتبر است." });

                var result = await service.GetArchivesAsync(
                    ticket, serial, fromDate, toDate,
                    page <= 0 ? 1 : page,
                    pageSize <= 0 ? 50 : pageSize, ct);

                return Results.Ok(result);
            })
                .AddEndpointFilter(new ApiKeyScope("read"))
                .WithName("ListSanaTotals")
                .WithSummary("لیست گزارش‌های توتال")
    .WithDescription("با فیلتر شماره تیکت، سریال دستگاه و بازه تاریخ.")
    .WithTags("گزارش توتال")
    .Produces<TotalArchiveListResponse>(200);

            // ------------------------------------------------------------
            // دانلود فایل آرشیو
            // GET /api/sana/total/{id}/download
            // ------------------------------------------------------------
            group.MapGet("/total/{id:guid}/download", async (
                    Guid id,
                    ISanaArchiveService service,
                    CancellationToken ct) =>
            {
                var (stream, fileName) = await service.OpenArchiveAsync(id, ct);

                if (stream is null)
                    return Results.NotFound(new { success = false, message = "فایل مورد نظر یافت نشد." });

                return Results.File(stream, "application/zip", fileName ?? "archive.zip");
            })
                .AddEndpointFilter(new ApiKeyScope("read"))
                .WithName("DownloadSanaTotal").WithSummary("دانلود فایل زیپ آرشیو")
    .WithTags("گزارش توتال")
    .Produces(200, contentType: "application/zip")
    .ProducesProblem(404);

            // ------------------------------------------------------------
            // ۳) وضعیت گزارش‌های یک تیکت
            // GET /api/sana/ticket/{ticketNumber}/status
            // ------------------------------------------------------------
            group.MapGet("/ticket/{ticketNumber}/status", async (
                    string ticketNumber,
                    ISanaArchiveService service,
                    CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(ticketNumber) || !long.TryParse(ticketNumber, out _))
                    return Results.BadRequest(new { success = false, message = "شماره تیکت معتبر نیست." });

                var result = await service.GetTicketStatusAsync(ticketNumber, ct);
                return Results.Ok(result);
            })
                .AddEndpointFilter(new ApiKeyScope("read"))
                .WithName("GetTicketStatus").WithSummary("بررسی وضعیت گزارش‌های یک تیکت")
    .WithDescription("مشخص می‌کند گزارش هفتگی، توتال، هر دو یا هیچ‌کدام ثبت شده است.")
    .WithTags("وضعیت تیکت")
    .Produces<TicketStatusResponse>(200); ;
        }
    }

    /// <summary>فیلتر کوچک برای تعیین scope موردنیاز هر endpoint.</summary>
    public sealed class ApiKeyScope : IEndpointFilter
    {
        private readonly string _scope;

        public ApiKeyScope(string scope) => _scope = scope;

        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var options = context.HttpContext.RequestServices
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<SanaStorageOptions>>();

            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<ApiKeyEndpointFilter>>();

            var filter = new ApiKeyEndpointFilter(options, logger, _scope);
            return await filter.InvokeAsync(context, next);
        }
    }
}