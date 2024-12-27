using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Thêm YARP vào DI container
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromSeconds(30);   // Thời gian cửa sổ 30 giây (tức nếu đã vượt quá giới hạn phải chờ 30s)
        limiterOptions.PermitLimit = 5;                     // Tối đa 5 request trong mỗi cửa sổ
        limiterOptions.QueueLimit = 0;                      // Không cho phép xếp hàng chờ
    });

    // Cấu hình thông báo lỗi khi bị giới hạn
    options.OnRejected = async (context, cancellationToken) =>
    {
        // Thiết lập mã trạng thái HTTP
        context.HttpContext.Response.StatusCode = 429;

        // Nội dung thông báo JSON
        var response = new
        {
            status = 429,
            title = "Too Many Requests",
            type = "https://tools.ietf.org/html/rfc6585#section-4",
            detail = "Rate limit exceeded. Try again later."
        };

        // Thiết lập Content-Type là application/json
        context.HttpContext.Response.ContentType = "application/json";

        // Ghi nội dung phản hồi
        await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(response), cancellationToken);
    };
});

var app = builder.Build();

// Sử dụng YARP
app.MapReverseProxy();
app.UseRateLimiter();

app.Run();
