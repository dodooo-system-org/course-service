using course_service.Data;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using course_service.Modules.Course.Services;
using course_service.Shared.Middleware;
using course_service.Modules.Category.Services;
using course_service.Modules.Modules.Services;
using course_service.Modules.Lesson;
using course_service.Modules.LessonPart.Services;
using course_service.Shared.RMQ;
using course_service.Shared.RMQ.Interfaces;
using course_service.Shared.RMQ.Services;
using course_service.Modules.Course.Interfaces;
using course_service.Modules.Category.Interfaces;
using course_service.Modules.Modules.Interfaces;
using course_service.Modules.Lesson.Interfaces;
using course_service.Modules.LessonPart.Interfaces;
using course_service.Modules.Caching.Interfaces;
using course_service.Modules.Caching.Services;
using course_service.Shared.Interfaces;
using course_service.Shared.Services;

namespace APIWithControllers;

public class Program
{
    public static void Main(string[] args)
    {
        // Load environment variables from .env file
        Env.Load();

        var builder = WebApplication.CreateBuilder(args);

        // Connect to the database
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 29));
        var connectionString = $"server={Env.GetString("MYSQL_HOST")};port={Env.GetString("MYSQL_PORT")};database={Env.GetString("MYSQL_DATABASE")};user={Env.GetString("MYSQL_USER")};password={Env.GetString("MYSQL_PASSWORD")};";
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, serverVersion));


        // Register Swagger for API documentation
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Course service API",
            });
        });

        // Add health checks (without RabbitMQ since our service manages its own connection)
        builder.Services.AddHealthChecks();

        // Register RabbitMQ service
        builder.Services.AddSingleton<IRMQService, RMQService>();
        builder.Services.AddSingleton<IRMQAuthService, RMQAuthService>();

        // Register redis cache service
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            string redisHost = Env.GetString("REDIS_HOST");
            string redisPort = Env.GetString("REDIS_PORT");
            string redisPassword = Env.GetString("REDIS_PASSWORD");
            options.Configuration = $"{redisHost}:{redisPort},password={redisPassword}";
            options.InstanceName = "course_service_cache:";
        });

        // Register services
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<ICourseService, CourseService>();
        builder.Services.AddScoped<IModuleService, ModuleService>();
        builder.Services.AddScoped<ILessonService, LessonService>();
        builder.Services.AddScoped<ILessonPartService, LessonPartService>();

        // Register shared services
        builder.Services.AddScoped<ICacheManager, CacheManager>();

        // Register caching services
        builder.Services.AddScoped<ICourseCachingService, CourseCachingService>();

        // Register controllers
        builder.Services.AddControllers();

        var app = builder.Build();

        // Initialize RabbitMQ service to ensure queue creation
        var rmqService = app.Services.GetRequiredService<IRMQService>();

        // Configure Swagger for API documentation
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = "swagger";
            });
        }

        // Only use HTTPS redirection in production, not development
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        // Register global exception middleware
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<AuthenticationMiddleware>();

        // Map health check endpoint
        app.MapHealthChecks("/health");

        app.MapControllers();

        app.Run();
    }
}