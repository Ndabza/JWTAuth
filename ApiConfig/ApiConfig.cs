using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace JWTAuth.ApiConfig;

public static class ApiConfig
{
    public static void ApiConfigServices(this IServiceCollection services)
    {
        services.AddDbContext<AuthDbContext>();
        services.AddControllers();
        services.AddOpenApi();
    }

    public static void UseOpenApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        if (app.Environment.IsProduction())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
    }
}
