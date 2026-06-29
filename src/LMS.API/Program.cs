using LMS.Application.Factory;
using LMS.Application.Services;
using LMS.Domain.Interfaces;
using LMS.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IAssignmentRepository, InMemoryAssignmentRepository>();
builder.Services.AddSingleton<ISubmissionRepository, InMemorySubmissionRepository>();
builder.Services.AddSingleton<IGradingStrategyFactory, GradingStrategyFactory>();
builder.Services.AddSingleton<GradingService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok());

app.Run();
