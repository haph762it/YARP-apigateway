# API gateway với YARP

cài đặt YARP

```
Install-Package Yarp.ReverseProxy
```

## Cấu hình YARP
ReverseProxy sẽ có 2 phần chính là: Routes và Clusters
gateway_service sẽ là ClusterId ứng với ClusterId của Routes

```csharp
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ReverseProxy": {
    "Routes": {
      "gateway_route": {
        "ClusterId": "gateway_service",
        "RateLimiterPolicy": "fixed",
        "Match": {
          "Path": "/gateway/{**catch-all}" // Định tuyến các yêu cầu đến backend gateway_service
        },
        "Transforms": [ { "PathPattern": "{**catch-all}" } ]
      },
      "core_route": {
        "ClusterId": "core_service",
        "RateLimiterPolicy": "fixed",
        "Match": {
          "Path": "/core-api/{**catch-all}" // Định tuyến các yêu cầu đến backend core_service
        },
        "Transforms": [ { "PathPattern": "{**catch-all}" } ]
      }
    },
    "Clusters": {
      "gateway_service": { // ClusterId
        "Destinations": {
          "gateway": {
            "Address": "http://192.168.1.18:8081" // gateway sẽ chuyển các yêu cầu đến địa chỉ này
          }
        }
      },
      "core_service": { // ClusterId
        "Destinations": {
          "core-api": {
            "Address": "http://192.168.1.18:8082" // core-api sẽ chuyển các yêu cầu đến địa chỉ này
          }
        }
      }
    }
  }
}
```

## Cấu hình DI YARP
```csharp
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
```

## Cấu hình AddRateLimiter
```csharp
options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromSeconds(30);   // Thời gian cửa sổ 30 giây (tức nếu đã vượt quá giới hạn phải chờ 30s)
        limiterOptions.PermitLimit = 5;                     // Tối đa 5 request trong mỗi cửa sổ
        limiterOptions.QueueLimit = 0;                      // Không cho phép xếp hàng chờ
    });
```