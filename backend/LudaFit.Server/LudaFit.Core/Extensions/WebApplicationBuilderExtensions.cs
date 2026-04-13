using System.Globalization;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using FluentValidation;
using LudaFit.Core.BackgroundServices;
using LudaFit.Core.Settings;
using LudaFit.Domain.Entities;
using LudaFit.Infrastructure.Gmail;
using LudaFit.Infrastructure.Gmail.Settings;
using LudaFit.Infrastructure.SQLite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LudaFit.Core.Extensions;

internal static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(TimeProvider.System);
        
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
        
        string tokenIssuer = builder.Configuration.GetOrThrow("TOKEN_ISSUER");
        string tokenAudience = builder.Configuration.GetOrThrow("TOKEN_AUDIENCE");
        string tokenKey = builder.Configuration.GetOrThrow("TOKEN_KEY");
        string tokenLifetime = builder.Configuration.GetOrThrow("TOKEN_LIFETIME");

        builder.AddJwtBearer(tokenIssuer, tokenAudience, tokenKey, tokenLifetime);
        
        string organisationName = builder.Configuration.GetOrThrow("ORGANISATION_NAME");
        string organisationEmail = builder.Configuration.GetOrThrow("ORGANISATION_EMAIL");
        string specialistName = builder.Configuration.GetOrThrow("SPECIALIST_NAME");
        string specialistEmail = builder.Configuration.GetOrThrow("SPECIALIST_EMAIL");
        string smtpServer = builder.Configuration.GetOrThrow("SMTP_SERVER");
#pragma warning disable CA1305
        int port = int.Parse(builder.Configuration.GetOrThrow("SMTP_PORT"));
#pragma warning restore CA1305
        string password = builder.Configuration.GetOrThrow("SMTP_PASSWORD");

        builder.AddMailKit(organisationName, organisationEmail, specialistName, specialistEmail, smtpServer, port, password);
        
        builder.Services.AddTypesToDi([Assembly.GetExecutingAssembly(), typeof(EmailSender).Assembly]);

        builder.Services.AddHostedService<DeleteExpiredDiscountsBackgroundService>();
        builder.Services.AddHostedService<SendEmailsBackgroundService>();

        return builder;
    }

    private static WebApplicationBuilder AddMailKit(
        this WebApplicationBuilder builder,
        string organisationName,
        string organisationEmail,
        string specialistName,
        string specialistEmail,
        string smtpServer,
        int port,
        string password)
    {
        builder.Services.Configure<EmailSettings>(options =>
        {
            options.OrganisationName = organisationName;
            options.OrganisationEmail = organisationEmail;
            options.SpecialistName = specialistName;
            options.SpecialistEmail = specialistEmail;
            options.SmtpServer = smtpServer;
            options.Port = port;
            options.Password = password;
        });
        
        return builder;
    }
    
    private static WebApplicationBuilder AddJwtBearer(this WebApplicationBuilder builder, string issuer, string audience, string key, string lifetime)
    {
        builder.Services.AddSingleton<IPasswordHasher<Admin>, PasswordHasher<Admin>>();
        
        builder.Services.Configure<JwtSettings>(options =>
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
        builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);
        
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
        string connectionString = builder.Configuration.GetOrThrow("DB_CONNECTION_STRING");
        
        builder.Services.AddDbContextPool<LudaFitDbContext>(options =>
            options.UseSqlite(connectionString));

        return builder;
    }
}
