using APIService; // Update namespace to match your project
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://192.168.20.155:3000") // Allow requests from your frontend
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()); // Important if credentials or SignalR WebSockets are used
});

// Add controller services
builder.Services.AddControllers(); // Enables controller routes
builder.WebHost.UseUrls("http://0.0.0.0:5000"); // Configure URL binding

// Add SignalR
builder.Services.AddSignalR();

// Add hosted service and dependencies
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<ISerialPortService, SerialPortService>(); // Update to match your dependency

var app = builder.Build();

// Use CORS middleware before routing
app.UseCors("AllowFrontend");

app.UseRouting();

app.MapControllers(); // Map controller routes
app.MapHub<ArduinoHub>("/arduinoHub"); // Map SignalR hub

app.Run();
