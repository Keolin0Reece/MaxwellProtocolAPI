using APIService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------
// Service Registration
// ------------------------------------

// Add CORS policy to allow frontend requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://0.0.0.0:3000") // Hidden URL for development testing
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()); // Required for SignalR WebSockets
});

builder.Services.AddControllers();

// Configure SignalR for real-time communication
builder.Services.AddSignalR();

// Register background worker and serial port service
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<ISerialPortService, SerialPortService>();

// Configure application to listen on all network interfaces (port 5000)
builder.WebHost.UseUrls("http://0.0.0.0:5000");

var app = builder.Build();

// ------------------------------------
// Middleware Pipeline
// ------------------------------------

// Enable CORS
app.UseCors("AllowFrontend");

// Enable routing
app.UseRouting();

// Map controller routes
app.MapControllers();

// Map SignalR hub for real-time communication
app.MapHub<ArduinoHub>("/arduinoHub");

app.Run();
