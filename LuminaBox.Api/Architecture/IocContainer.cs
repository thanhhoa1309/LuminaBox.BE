using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Exceptions;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Mappings;
using LuminaBox.Application.Services;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;
using LuminaBox.Infrastructure;
using LuminaBox.Infrastructure.Commons;
using LuminaBox.Infrastructure.Persistence;
using LuminaBox.Infrastructure.Realtime;
using LuminaBox.Infrastructure.Service;
using StripeSettings = LuminaBox.Application.Commons.StripeSettings;

namespace LuminaBox.Api.Architecture
{
    public static class IocContainer
    {
        public static IServiceCollection SetupIocContainer(this IServiceCollection services, IConfiguration configuration)
        {
            services.SetupDbContext(configuration);
            services.SetupSwagger();
            services.SetupCors(configuration);

            services.AddHttpContextAccessor();

            services.AddScoped<ICurrentTime, CurrentTime>();
            services.AddScoped<IClaimsService, ClaimsService>();
            services.AddScoped<IEmailService, LogEmailService>();

            services.SetupStripe(configuration);
            services.SetupGeography(configuration);
            services.SetupRealtime();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IStripePaymentService, StripePaymentService>();

            // Business services — đăng ký khi mang Application Services sang
            services.SetupBusinessServicesLayer();

            services.AddAutoMapper(cfg => cfg.LicenseKey = configuration["AutoMapper:LicenseKey"], typeof(StallMappingProfile).Assembly);
            services.SetupValidation();
            services.SetupJwt(configuration);

            return services;
        }

        private static IServiceCollection SetupDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
            }

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);

            services.AddDbContext<LuminaBoxDbContext>(options =>
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(LuminaBoxDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                })
            );

            return services;
        }

        public static IServiceCollection SetupBusinessServicesLayer(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IOrganizerRegistrationService, OrganizerRegistrationService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IGeoSyncService, GeoSyncService>();
            services.AddScoped<ICakeCategoryService, CakeCategoryService>();
            services.AddScoped<IFillingService, FillingService>();
            services.AddScoped<IOrganizerAccessService, OrganizerAccessService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductItemService, ProductItemService>();
            services.AddScoped<IStallService, StallService>();
            services.AddScoped<IBoxItemService, BoxItemService>();
            services.AddScoped<IShippingFeeService, ShippingFeeService>();
            services.AddScoped<IGhnClient, GhnClient>();

            return services;
        }

        private static IServiceCollection SetupStripe(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<StripeSettings>(opts =>
            {
                opts.SecretKey = configuration["Stripe:SecretKey"]
                    ?? configuration["STRIPE_SECRET_KEY"]
                    ?? string.Empty;
                opts.PublishableKey = configuration["Stripe:PublishableKey"]
                    ?? configuration["STRIPE_PUBLISHABLE_KEY"]
                    ?? string.Empty;
                opts.WebhookSecret = configuration["Stripe:WebhookSecret"]
                    ?? configuration["STRIPE_WEBHOOK_SECRET"]
                    ?? string.Empty;
            });

            return services;
        }

        private static IServiceCollection SetupGeography(this IServiceCollection services, IConfiguration configuration)
        {
            var ghnSection = configuration.GetSection("ShippingProviders:ghn");

            services.Configure<GhnSettings>(ghnSection);

            services.AddHttpClient(GeoSyncService.GovernmentClientName, client =>
            {
                client.BaseAddress = new Uri(configuration["Geography:GovernmentBaseUrl"] ?? "https://provinces.open-api.vn/");
                client.Timeout = TimeSpan.FromMinutes(2);
            });

            services.AddHttpClient(GeoSyncService.GhnClientName, client =>
            {
                var settings = ghnSection.Get<GhnSettings>() ?? new GhnSettings();
                client.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/");
                if (!string.IsNullOrWhiteSpace(settings.Token))
                    client.DefaultRequestHeaders.Add("Token", settings.Token);
                client.Timeout = TimeSpan.FromMinutes(2);
            });

            return services;
        }

        private static IServiceCollection SetupRealtime(this IServiceCollection services)
        {
            services.AddSignalR()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddScoped<SignalRRealtimePublisher>();
            services.AddScoped<IRealtimePublisher>(sp => sp.GetRequiredService<SignalRRealtimePublisher>());

            return services;
        }

        private static IServiceCollection SetupValidation(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<AppException>();

            services.PostConfigure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var message = string.Join(" ", context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    return new BadRequestObjectResult(ApiResult.Failure("400", message));
                };
            });

            return services;
        }

        public const string FrontendCorsPolicy = "LuminaBoxFrontend";

        private static IServiceCollection SetupCors(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            if (allowedOrigins is null || allowedOrigins.Length == 0)
            {
                allowedOrigins = new[] { "http://localhost:5173", "http://127.0.0.1:5173" };
            }

            services.AddCors(options =>
            {
                options.AddPolicy(FrontendCorsPolicy, policy =>
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });

            return services;
        }

        private static IServiceCollection SetupSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.UseInlineDefinitionsForEnums();

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "LuminaBox API",
                    Version = "v1",
                    Description = "API for LuminaBox."
                });

                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter your JWT token in the text input below (without the 'Bearer' prefix).",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", jwtSecurityScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });

                foreach (var xmlFile in Directory.GetFiles(AppContext.BaseDirectory, "LuminaBox.*.xml"))
                {
                    c.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
                }

                c.UseAllOfForInheritance();
                c.EnableAnnotations();
            });

            return services;
        }

        private static IServiceCollection SetupJwt(this IServiceCollection services, IConfiguration configuration)
        {
            var secretKey = configuration["JWT:SecretKey"];
            var issuer = configuration["JWT:Issuer"];
            var audience = configuration["JWT:Audience"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT:SecretKey not found in configuration.");
            }

            if (string.IsNullOrEmpty(issuer))
            {
                throw new InvalidOperationException("JWT:Issuer not found in configuration.");
            }

            if (string.IsNullOrEmpty(audience))
            {
                throw new InvalidOperationException("JWT:Audience not found in configuration.");
            }

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            if (!string.IsNullOrEmpty(accessToken) && context.Request.Path.StartsWithSegments("/hubs"))
                                context.Token = accessToken;

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("UserPolicy", policy =>
                    policy.RequireRole(nameof(UserRole.User)));

                options.AddPolicy("ManagerPolicy", policy =>
                    policy.RequireRole(nameof(UserRole.Manager)));

                options.AddPolicy("OrganizationPolicy", policy =>
                    policy.RequireRole(nameof(UserRole.Organization)));
            });

            return services;
        }
    }
}
