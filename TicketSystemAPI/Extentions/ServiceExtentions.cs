using System.Text;
using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using ExceptionHandler;
using LoggerService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository;
using Services;
using Quartz;
using Entities.DbContext;
// using CryptUtility;
using Quartz.Spi;
using Entities;
using AutoMapper;
using Entities.Common;
using Entities.Migrations;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
namespace TicketSystemAPI.Extensions
{
    /// <summary>
    /// Class <c>Service Extenstions</c> is static consists of service extensions.
    /// Contains static method to configure the services required to run the application.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// This method is used to configure the CORS (Cross-Origin Resource Sharing).
        /// CORS is a mechanism that gives rights to the user to access resources
        /// from the server on a different domain
        /// </summary>
        public static void ConfigureCors(this IServiceCollection services, IConfiguration config)
        {
            _ = services.AddCors(options =>
            {
                options.AddPolicy(
                    "CorsPolicy",
                    builder =>
                        builder
                            .SetIsOriginAllowed(origin =>
                        origin.Equals(config[Common.DEFAULT_FRONT_END_ORIGIN]!, StringComparison.OrdinalIgnoreCase) ||
                        origin.Equals(config[Common.DEFAULT_FRONT_END_ORIGIN_LOCAL]!, StringComparison.OrdinalIgnoreCase) ||
                        origin.Equals(config[Common.DEFAULT_FRONT_END_ORIGIN_LOCAL_SINGLE_APP]!, StringComparison.OrdinalIgnoreCase) ||
                        origin.Equals(config[Common.DEFAULT_FRONT_END_ORIGIN_SINGLE_APP]!, StringComparison.OrdinalIgnoreCase) ||
                        origin.EndsWith(config[Common.ARIBA_DOMAIN_ORIGIN]!, StringComparison.OrdinalIgnoreCase)
                    )
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .WithExposedHeaders("Content-Disposition")
                );
            });
        }
        /// <summary>
        /// This method is used to configure the rate limiting for the APIs. It limits the number of requests from a single IP address to prevent abuse and protect the server from overload.
        /// </summary> <param name="services"></param>
        public static void ConfigureRateLimiting(this IServiceCollection services)
        {
            // Forwarded Headers (for real IP behind proxy)
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor;
            });

            // Rate Limiter
            services.AddRateLimiter(options =>
            {
                options.AddPolicy("LoginPolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                       partitionKey: httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                            ?? httpContext.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429;
                    context.HttpContext.Response.ContentType = "application/json";

                    var response = new
                    {
                        status_code = 429,
                        message = "Maximum limit reached. Please try again later.",
                        description = "Too many requests from this IP. Please try again later."
                    };

                    await context.HttpContext.Response.WriteAsJsonAsync(response);
                };
            });
        }
        /// <summary>
        /// This method is used to configure the IIS integration helps to configure the
        /// properties for IIS server deployment.
        /// </summary>
        public static void ConfigureIISIntegration(this IServiceCollection services)
        {
            _ = services.Configure<IISOptions>(options => { });
        }

        /// <summary>
        /// This method is used to inject logger service inside the .NET Core’s IOC container with
        /// singleton scope.
        /// </summary>
        public static void ConfigureLoggerService(this IServiceCollection services)
        {
            _ = services.AddSingleton<ILoggerManager, LoggerManager>();
        }

        /// <summary>
        /// This method is used to inject the repository context into IOC with DBContext scope.
        /// </summary>
        /// <paramref name="config">The config paramter used to read attributes in appsettings.json</paramref>
        public static void ConfigureDBContext(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            string dbString = config.GetConnectionString("DefaultConnection")!;
            services.AddDbContext<RepositoryContext>(options =>
            {
                options.UseSqlServer(dbString, 
                        sqlOptions =>
                        {
                            sqlOptions.CommandTimeout(900);
                        });
            });
        }

        /// <summary>
        /// This method is used to inject the services where the business logics are implemented.
        /// </summary>
        public static void ConfigureServiceWrapper(
            this IServiceCollection services
        )
        {
            _ = services.AddScoped<IUserService, UserService>();
            _ = services.AddScoped<IPersonService, PersonService>();
            _ = services.AddScoped<IAuthService, AuthService>();
            _ = services.AddScoped<IAccessTokenService, AccessTokenService>();
            _ = services.AddScoped<IFeatureService, FeatureService>();
            _ = services.AddScoped<IRoleFeatureMappingService, RoleFeatureMappingService>();
            _ = services.AddScoped<IUserIdentityService, UserIdentityService>();
            _ = services.AddScoped<IAssetService, AssetService>();
            _ = services.AddScoped<IMetadataService, MetadataService>();
            _ = services.AddScoped<ITicketService, TicketService>();
            _ = services.AddScoped<IUserRoleMappingService, UserRoleMappingService>();
            _ = services.AddScoped<IUserContext, UserContext>();
            _ = services.AddScoped<IDomainService, DomainService>();
            _ = services.AddScoped<IEmailService, EmailService>();
            _ = services.AddScoped<IJobFactory, MyJobFactoryService>();
            _ = services.AddScoped<ISchedulerService, SchedulerService>();
            _ = services.AddScoped<EmailRemainderService>();
            _ = services.AddScoped<DomainTicketService>();
            _ = services.AddScoped<IPlanConfigService, PlanConfigService>();
            _ = services.AddScoped<IDiscountService, DiscountService>();
            _ = services.AddScoped<EmailFailedBackgroundService>();
            _ = services.AddScoped<DomainPlanBackgroundService>();
            _ = services.AddScoped<DomainContractExpiryBackgroundService>();
            _ = services.AddScoped<IExcelUploadService, ExcelUploadService>();
            _ = services.AddScoped<IAribaPRService, AribaPRService>();
            _ = services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            _ = services.AddScoped<IPurchaseOrderBotProcessService, PurchaseOrderBotProcessService>();
            _ = services.AddScoped<IAribaService, AribaService>();
            _ = services.AddScoped<IInvoicePendingApprovableService, InvoicePendingApprovableService>();
            _ = services.AddScoped<IVenueService, VenueService>();
            _ = services.AddScoped<IPRPendingApprovableService, PRPendingApprovableService>();
            _ = services.AddScoped<IPRPendingApprovableService, PRPendingApprovableService>();
            _ = services.AddScoped<AribaInvoiceJob>();
            _ = services.AddScoped<AribaPRJob>();
            _ = services.AddScoped<CreditDefaultBackgroundService>();
            _ = services.AddScoped<ContractJob>();
            _ = services.AddScoped<AribaContractJob>();
            _ = services.AddScoped<IContractPendingService, ContractPendingService>();
            _ = services.AddScoped<IBulkInsertHelper, BulkInsertHelper>();
            _ = services.AddScoped<IContractService, ContractService>();
            _ = services.AddScoped<ISchedulerHistoryService, SchedulerHistoryService>();
            _ = services.AddScoped<IAdditionalQueryService, AdditionalQueryService>();
            _ = services.AddScoped<IUploadAribaInvoiceService, UploadAribaInvoiceService>();
            _ = services.AddScoped<ILinkService, LinkService>();
            _ = services.AddScoped<ILoginRecordService, LoginRecordService>();
            _ = services.AddScoped<IMaterialPRService, MaterialPRService>();
            _ = services.AddScoped<IAribaMaterialPRService, AribaMaterialPRService>();
            _ = services.AddScoped<IDraftMaterialPRService, DraftMaterialPRService>();
            _ = services.AddScoped<IAribaPaymentService, AribaPaymentService>();
            _ = services.AddScoped<IExcelSafeStoreHelper, ExcelSafeStoreHelper>();
            _ = services.AddScoped<IAribaServicePRService, AribaServicePRService>();
            _ = services.AddScoped<IFinalPurchaseOrderService, FinalPurchaseOrderService>();
            _ = services.AddScoped<IServicesPRService, ServicesPRService>();
            _ = services.AddScoped<IPublicRelationService, PublicRelationService>();
            _ = services.AddScoped<IPublicRelationSoapService, PublicRelationSoapService>();
            _ = services.AddScoped<IConciergePRCreationService, ConciergePRCreationService>();
            _ = services.AddScoped<IDraftServicePrService, DraftServicePRService>();
            _ = services.AddScoped<IPublicRelationServicePRService, PublicRelationServicePRService>();
            _ = services.AddScoped<IPublicRelationServicePRSoapService, PublicRelationServicePRSoapService>();
            _ = services.AddScoped<IConciergeServicePRService, ConciergeServicePRService>();
            _ = services.AddScoped<ILegalDepartmentMaterialService, LegalDepatmentMaterialService>();
            _ = services.AddScoped<IPRService, PRService>();
            _ = services.AddScoped<IContractExcelService, ContractExcelService>();
            _ = services.AddScoped<ICountryListService, CountryListService>();
            _ = services.AddControllers();

        }

        /// <summary>
        /// This method is used to inject the entity repository as scoped instance.
        /// </summary>
        public static void ConfigureRepositoryWrapper(this IServiceCollection services)
        {
            _ = services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
        }

        public static void ConfigureScheduler(this IServiceCollection services)
        {
            var scheduler = services.BuildServiceProvider().GetRequiredService<ISchedulerService>();
            scheduler.Start();
            scheduler.ScheduleFailedEmailJob();
            scheduler.ScheduleDomainPlanRemainderJob();
            scheduler.ScheduleDomainContractExpiryJob();
            scheduler.ScheduleAribaInvoiceJob();
            // scheduler.ScheduleContractJob();
            scheduler.ScheduleAribContractJob();
            scheduler.ScheduleAribPRJob();
            scheduler.SchedulePlanCreditDefault();
        }

        /// <summary>
        /// This method is used to inject the mappings of Entities and Dto.
        /// </summary>
        public static void ConfigureAutoMapper(this IServiceCollection services)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            }, loggerFactory);

            services.AddSingleton<IMapper>(config.CreateMapper());
        }

        /// <summary>
        /// This method is used to inject the custom exception middleware.
        /// </summary>
        public static IApplicationBuilder UseHttpStatusCodeExceptionMiddleware(
            this IApplicationBuilder builder
        )
        {
            return builder.UseMiddleware<CustomExceptionMiddleware>();
        }

        /// <summary>
        /// This method is used to validate the Authorization token. APIs annotated with Authorize
        /// must have Authorization header to pass through, otherwise it returns unauthorized.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void ConfigureAuthentication(
            this IServiceCollection services
        )
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes("1kt6ryCXoY13_9LrCfyfF_wSgPVxlewGtrBlY0PkyxA")
                        ),
                        ValidateLifetime = false,
                        ClockSkew = TimeSpan.Zero //the default for this setting is 5 minutes
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            if (context.Request.Cookies.TryGetValue(Common.COOKIE_ACCESS_TOKEN_KEY, out var token))
                            {
                                context.Token = token;
                            }
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            if (
                                context.Exception.GetType() == typeof(SecurityTokenExpiredException)
                            )
                            {
                                context.Response.Headers.Append("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
        }
    }
}
