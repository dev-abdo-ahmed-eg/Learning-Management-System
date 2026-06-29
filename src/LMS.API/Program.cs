using LMS.Application.Events;
using LMS.Application.Factory;
using LMS.Application.Services;
using LMS.Application.Subscribers;
using LMS.Domain.Interfaces;
using LMS.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IAssignmentRepository, InMemoryAssignmentRepository>();
builder.Services.AddSingleton<ISubmissionRepository, InMemorySubmissionRepository>();
builder.Services.AddSingleton<IGradingStrategyFactory, GradingStrategyFactory>();
builder.Services.AddSingleton<IEventBus, InProcessEventBus>();
builder.Services.AddSingleton<GradingService>();
builder.Services.AddSingleton<NotificationSubscriber>();
builder.Services.AddSingleton<ProgressSubscriber>();
builder.Services.AddSingleton<AuditSubscriber>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.MapGet("/health", () => Results.Ok());

app.Run();
