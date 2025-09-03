using CustomerSupport.Data;
using CustomerSupport.Hubs;
using CustomerSupport.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
        
    // PostgreSQL sink eklemek istersen (opsiyonel)
    var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(connectionString))
    {
        configuration.WriteTo.PostgreSQL(
            connectionString: connectionString,
            tableName: "Logs",
            needAutoCreateTable: true);
    }
});

// Entity Framework Core configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

// Services
builder.Services.AddScoped<IEmbeddingService, OpenAIEmbeddingService>();
builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
builder.Services.AddScoped<IRagService, RagService>();
builder.Services.AddScoped<IAIService, OpenAIService>();

// Controllers
builder.Services.AddControllers();

// SignalR
builder.Services.AddSignalR();

// Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// HttpClient for admin panel
builder.Services.AddHttpClient();

// API Documentation
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Database migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı migration işlemi sırasında hata oluştu");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowBlazor");

// SignalR Hub
app.MapHub<ChatHub>("/chathub");

// API Controllers
app.MapControllers();

// Blazor Server
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
