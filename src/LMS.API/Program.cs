using System.Reflection;
using System.Text.Json.Serialization;
using LMS.Application.Events;
using LMS.Application.Factory;
using LMS.Application.Services;
using LMS.Application.Subscribers;
using LMS.Domain.Interfaces;
using LMS.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IAssignmentRepository, InMemoryAssignmentRepository>();
builder.Services.AddSingleton<ISubmissionRepository, InMemorySubmissionRepository>();
builder.Services.AddSingleton<IGradingStrategyFactory, GradingStrategyFactory>();
builder.Services.AddSingleton<IEventBus, InProcessEventBus>();
builder.Services.AddSingleton<GradingService>();
builder.Services.AddSingleton<SubmissionService>();
builder.Services.AddSingleton<NotificationSubscriber>();
builder.Services.AddSingleton<ProgressSubscriber>();
builder.Services.AddSingleton<AuditSubscriber>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LMS Design Patterns API",
        Version = "v1",
        Description = "Assignment → submission → grading workflow demonstrating Strategy, Factory, and Observer patterns."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

var bus = app.Services.GetRequiredService<IEventBus>();
bus.Subscribe(app.Services.GetRequiredService<NotificationSubscriber>());
bus.Subscribe(app.Services.GetRequiredService<ProgressSubscriber>());
bus.Subscribe(app.Services.GetRequiredService<AuditSubscriber>());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok())
    .WithName("HealthCheck")
    .WithTags("Health");

app.MapControllers();

app.Run();
