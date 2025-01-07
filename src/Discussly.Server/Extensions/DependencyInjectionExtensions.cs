using Discussly.Server.Data.Domain;
using Discussly.Server.Data.Domain.Interfaces;
using Discussly.Server.Data.Entities.Users;
using Discussly.Server.Data.Repositories;
using Discussly.Server.Data.Repositories.Interfaces;
using Discussly.Server.DTO;
using Discussly.Server.Endpoints.Comments;
using Discussly.Server.Infrastructure.Consumers;
using Discussly.Server.Infrastructure.Services.Azure;
using Discussly.Server.Infrastructure.Services.Azure.Interfaces;
using Discussly.Server.Infrastructure.Services.Elastic;
using Discussly.Server.Infrastructure.Services.Elastic.Interfaces;
using Discussly.Server.Infrastructure.Services.WebSockets;
using Discussly.Server.Middlewares;
using Discussly.Server.Services;
using Discussly.Server.Services.Captcha;
using Discussly.Server.Services.Captcha.Interfaces;
using Discussly.Server.Services.Interfaces;
using Discussly.Server.Services.Tokens;
using Discussly.Server.Services.Tokens.Interfaces;
using Discussly.Server.SharedKernel.Settings.Azure;
using Discussly.Server.Workers;
using Elastic.Clients.Elasticsearch;
using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;
using System.Reflection;

namespace Discussly.Server.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddCaching(this IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
            });
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);

            return services;
        }

        public static IServiceCollection AddRedis(this IServiceCollection services, ConfigurationManager configurationManager)
        {
            var redisConnectionString = configurationManager["Redis:ConnectionString"];
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "Discussly.Server:";
            });

            return services;
        }

        public static IServiceCollection AddSqlConnection<TReadContext>(this IServiceCollection services, ConfigurationManager configurationManager)
           where TReadContext : DbContext
        {
            var usersConnectionString = configurationManager.GetConnectionString("DiscusslyDbContext");
            services.AddDbContext<DiscussionDataContext>(options =>
            {
                options.UseSqlServer(usersConnectionString);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                options.LogTo(_ => { }, LogLevel.None);
            });

            return services;
        }

        public static IServiceCollection AddIdentityServer4(this IServiceCollection services, ConfigurationManager configurationManager)
        {
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            }).AddEntityFrameworkStores<DiscussionDataContext>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                options.DefaultChallengeScheme =
                options.DefaultForbidScheme =
                options.DefaultScheme =
                options.DefaultSignOutScheme =
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configurationManager["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configurationManager["JWT:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(configurationManager["JWT:SigningKey"]!)
                    )
                };
            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            services.AddAuthorization();

            return services;
        }

        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, ConfigurationManager configurationManager)
        {
            var rabbitConnectionString = configurationManager["Rabbit:ConnectionString"];

            services.AddMassTransit(x =>
            {
                x.AddConsumer<CommentConsumer>();
                x.AddConsumer<UserInfoConsumer>();
                x.AddConsumer<ChunkedCommentFileConsumer>();
                x.AddConsumer<ChunkedAvatarFileConsumer>();
                x.AddConsumer<DeleteUserAvatarConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitConnectionString, "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ReceiveEndpoint("comment_queue", e =>
                    {
                        e.ConfigureConsumer<CommentConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("user_info_queue", e =>
                    {
                        e.ConfigureConsumer<UserInfoConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("chunked_comment_file_queue", e =>
                    {
                        e.ConfigureConsumer<ChunkedCommentFileConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("chunked_avatar_file_queue", e =>
                    {
                        e.ConfigureConsumer<ChunkedAvatarFileConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("delete_user_avatar_queue", e =>
                    {
                        e.ConfigureConsumer<DeleteUserAvatarConsumer>(context);
                    });
                });
            });

            return services;
        }

        public static IServiceCollection AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
        {
            var elasticUri = configuration["Elasticsearch:Uri"];
            var defaultIndex = configuration["Elasticsearch:DefaultIndex"]!;

            var settings = new ElasticsearchClientSettings(new Uri(elasticUri!)).DefaultIndex(defaultIndex);
            var client = new ElasticsearchClient(settings);

            services.AddSingleton(client);
            services.AddScoped<IElasticSearchService, ElasticSearchService>();
            services.AddHostedService<ElasticSearchBackgroundWorker>();

            return services;
        }

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("MyCorsPolicy", builder => builder
                    .WithOrigins("http://4.232.137.247", "http://localhost:3000", "http://localhost")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IDiscussionDataUnitOfWork, DiscussionDataUnitOfWork>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ICaptchaImageGenerator, DefaultCaptchaImageGenerator>();
            services.AddScoped<IChunkedFileSenderService, ChunkedFileSenderService>();

            services.Configure<AzureBlobStorageSettings>(configuration.GetSection("AzureBlobStorage"));
            services.AddSingleton<IBlobStorageService, BlobStorageService>();

            services.AddSingleton<WebSocketHandler>();
            services.AddSingleton<Infrastructure.Services.WebSockets.WebSocketManager>();

            services.AddSingleton<ErrorHandlingMiddleware>();

            services.AddScoped<Query>();

            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .AddType<CommentSortByDtoType>()
                .AddType<CommentSortDirectionDtoType>()
                .AddAuthorization()
                .AddFiltering()
                .AddSorting()
                .AddProjections()
                .ModifyOptions(o =>
                {
                    o.EnableStream = true;
                    o.StrictValidation = false;
                    o.EnableDefer = true;
                });

            return services;
        }

        public static IServiceCollection AddSessions(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = ".Discussly.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(5);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            });

            return services;
        }
    }
}