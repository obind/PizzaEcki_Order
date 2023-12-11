using Microsoft.AspNetCore.SignalR;
using PizzaServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddDbContext<PizzaDbContext>();
builder.Services.AddScoped<PizzaDataService>();

var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
        builder
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithOrigins(allowedOrigins) // Verwende die konfigurierten URLs
        .AllowCredentials());
});

var app = builder.Build();

// Konfigurieren der Middleware-Reihenfolge.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("CorsPolicy");

app.UseRouting();

app.UseAuthorization();

// API-Endpunkte
app.MapGet("/drivers", async (PizzaDataService service) =>
{
    try
    {
        var drivers = await service.GetAllDriversAsync();
        return Results.Ok(drivers);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/orderAssignments", async (PizzaDataService service, OrderAssignmentDto dto) =>
{
    try
    {
        await service.SaveOrderAssignmentAsync(dto.OrderId, dto.DriverId, dto.Price);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/unassignedOrders", async (PizzaDataService service) =>
{
    try
    {
        var orders = await service.GetUnassignedOrdersAsync();
        return Results.Ok(orders);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});


app.MapGet("/GetCustomer", async (PizzaDataService service, string phoneNumber) =>
{
    try
    {
        var customer = await service.GetCustomerByPhoneNumber(phoneNumber);
        return customer != null ? Results.Ok(customer) : Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});


app.MapGet("/healthcheck", () => "Server OK");

app.MapHub<PizzaHub>("/pizzaHub"); // Map the SignalR Hub

app.MapRazorPages();

app.Run();
