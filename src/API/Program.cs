using API.Middleware;
using Application;
using Application.Settings;
using Domain.Interfaces;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors();
builder.Services.AddControllers()
    .AddJsonOptions(options => 
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase))
        );

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "balance-tracker-backed",
        Description = "An ASP.NET Core Web API for managing user balance"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.\n\n" +
                      "Enter 'Bearer' [space] and then your token in the text input below.\n\n" +
                      "Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
              },
              Scheme = "oauth2",
              Name = "Bearer",
              In = ParameterLocation.Header,

            },
            new List<string>()
          }
        });
});

Infrastructure.DependencyInjection.AddServices(builder.Services, builder.Configuration);
Application.DependencyInjection.AddServices(builder.Services);

var app = builder.Build();

app.UseMiddleware<LoggingMiddleware>()
    .UseMiddleware<ExceptionHandlerMiddleware>()
    .UseMiddleware<JwtTokenMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Lifetime.ApplicationStarted.Register(async () =>
{
    var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetService<CategoriesLoader>().LoadAsync(
        scope.ServiceProvider.GetRequiredService<ILogger<CategoriesLoader>>(),
        app.Configuration,
        scope.ServiceProvider.GetRequiredService<ICategoryRepository>(),
        scope.ServiceProvider.GetRequiredService<IEntryRepository>()
        );
});

app.UseHttpsRedirection();

app.UseCors(options =>
{
    options.WithOrigins(FrontendSettings.Get(builder.Configuration).Address)
        .AllowAnyMethod()
        .AllowAnyHeader();
});

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
