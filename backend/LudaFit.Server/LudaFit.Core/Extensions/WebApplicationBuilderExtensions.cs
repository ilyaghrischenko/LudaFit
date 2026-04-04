using System.Globalization;
using System.Security.Authentication;
using System.Text;
using FluentValidation;
using LudaFit.Core.BackgroundServices;
using LudaFit.Core.Options;
using LudaFit.Infrastructure.SQLite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LudaFit.Core.Extensions;

internal static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        
        if (builder.Environment.IsDevelopment())
        {
            builder.ValidateDIOnBuild();
        }
        
        builder
            .AddResponseCompression()
            .AddDbContext()
            .AddCors()
            .AddFluentValidation();
        
        //todo: scalar в качестве документации апи добавить
        
        string tokenIssuer = builder.Configuration.GetOrThrow("TOKEN_ISSUER");
        string tokenAudience = builder.Configuration.GetOrThrow("TOKEN_AUDIENCE");
        string tokenKey = builder.Configuration.GetOrThrow("TOKEN_KEY");
        string tokenLifetime = builder.Configuration.GetOrThrow("TOKEN_LIFETIME");
        
        builder.AddJwtBearer(tokenIssuer, tokenAudience, tokenKey, tokenLifetime);

        builder.Services.AddTypesToDi();

        builder.Services.AddHostedService<DeleteExpiredDiscountsBackgroundService>();

        return builder;
    }
    
    private static WebApplicationBuilder AddJwtBearer(this WebApplicationBuilder builder, string issuer, string audience, string key, string lifetime)
    {
        builder.Services.Configure<JwtOptions>(options =>
        {
            options.Issuer = issuer;
            options.Audience = audience;
            options.Key = key;
            options.Lifetime = int.Parse(lifetime, CultureInfo.InvariantCulture);
        });
        
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                //todo: добавить настройки валидации jwt token
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    
                    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                };
            });

        //todo
        //services.AddAuthorizationBuilder()
        //    .AddPoliciesByRoles();
        
        return builder;
    }
    
    private static WebApplicationBuilder AddFluentValidation(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
        
        return builder;
    }
    
    private static void ValidateDIOnBuild(this WebApplicationBuilder builder)
    {
        builder.Host.UseDefaultServiceProvider((context, options) =>
        {
            options.ValidateOnBuild = true;
            options.ValidateScopes = true;
        });
    }
    
    private static WebApplicationBuilder AddResponseCompression(this WebApplicationBuilder builder)
    {
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        return builder;
    }
    
    private static WebApplicationBuilder AddCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            if (builder.Environment.IsDevelopment())
            {
                options.AddPolicy("AllowReactDevClient", corsBuilder =>
                {
                    //todo: поменять порт
                    corsBuilder.WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            }
        });
        
        return builder;
    }
    
    private static WebApplicationBuilder AddDbContext(this WebApplicationBuilder builder)
    {
        string? connectionString = builder.Configuration["DB_CONNECTION_STRING"];

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidCredentialException("DB_CONNECTION_STRING is not set");
        }
        
        builder.Services.AddDbContext<LudaFitDbContext>(options =>
            options.UseSqlite(connectionString));

        return builder;
    }
}
