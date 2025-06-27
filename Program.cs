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

        builder.Services.AddControllers();

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

        // Register services
        builder.Services.AddScoped<CategoryService>();
        builder.Services.AddScoped<CourseService>();
        builder.Services.AddScoped<ModuleService>();
        builder.Services.AddScoped<LessonService>();
        builder.Services.AddScoped<LessonPartService>();

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