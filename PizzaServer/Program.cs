using Microsoft.AspNetCore.SignalR;
using PizzaServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR(); // Add SignalR services

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
        builder
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithOrigins("https://localhost:7166", "http://localhost:5062") // HTTPS und HTTP Ports des Servers
        .AllowCredentials());
});




// Rest deines Codes...

var app = builder.Build();
// Dann in der app-Konfiguration:
app.UseCors("CorsPolicy");
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
//API
app.MapGet("/sendmessage", async (IHubContext<PizzaHub> hubContext, string user, string message) =>
{
    await hubContext.Clients.All.SendAsync("ReceiveMessage", user, message);
    return Results.Ok($"Nachricht '{message}' von '{user}' gesendet");
});



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapHub<PizzaHub>("/pizzaHub"); // Map the PizzaHub

app.MapRazorPages();

app.Run();
