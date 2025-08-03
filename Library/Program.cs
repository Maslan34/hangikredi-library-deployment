using Confluent.Kafka;
using Library.Data;
using Library.Messaging;
using Library.Services;
using Microsoft.EntityFrameworkCore;
using Nest;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllersWithViews();

// ========================= PostgreSQL =========================
// PostgreSQL Connection String yapýlandýrmasý
string connectionString;

// Railway'de DATABASE_URL environment variable'ý varsa onu kullan
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    // DATABASE_URL formatý: postgresql://user:password@host:port/database
    var uri = new Uri(databaseUrl);
    var password = uri.UserInfo.Split(':')[1];
    var username = uri.UserInfo.Split(':')[0];

    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    Console.WriteLine($"Using Railway PostgreSQL connection");
}
else
{
    // Local development için appsettings.json'dan al
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    Console.WriteLine("Using local PostgreSQL connection");
}

// Configure DbContext for PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
// ========================= PostgreSQL =========================

// ========================= Kafka (Sadece local'de) =========================
var kafkaBootstrapServers = builder.Configuration.GetSection("Kafka")["BootstrapServers"];
if (!string.IsNullOrEmpty(kafkaBootstrapServers))
{
    Console.WriteLine("Configuring Kafka services...");

    // Kafka Producer registration
    builder.Services.AddSingleton<IProducer<Null, string>>(sp =>
    {
        var config = new ProducerConfig
        {
            BootstrapServers = kafkaBootstrapServers
        };
        return new ProducerBuilder<Null, string>(config).Build();
    });

    // Adding Kafka producer to services
    builder.Services.AddSingleton<KafkaLibraryProducer>();

    // Register Kafka initializer as a HostedService
    builder.Services.AddHostedService<KafkaInitializerService>();

    // Add Kafka consumer as a background service
    builder.Services.AddHostedService<KafkaLibraryConsumer>();
}
else
{
    Console.WriteLine("Kafka configuration not found, skipping Kafka services...");
}
// ========================= Kafka =========================

// ========================= Elastic (Sadece local'de) =========================
var elasticUri = builder.Configuration.GetSection("Elastic")["Uri"];
if (!string.IsNullOrEmpty(elasticUri))
{
    Console.WriteLine("Configuring Elasticsearch services...");

    // Elasticsearch client
    builder.Services.AddSingleton<ElasticClient>(sp =>
    {
        var elasticUsername = builder.Configuration.GetSection("Elastic")["Username"] ?? "elastic";
        var elasticPassword = builder.Configuration.GetSection("Elastic")["Password"] ?? "PwkDC4KKmTKSA0_z9zI7";

        var settings = new ConnectionSettings(new Uri(elasticUri))
            .DefaultIndex("books")
            .BasicAuthentication(elasticUsername, elasticPassword)
            .DisableDirectStreaming()
            .EnableDebugMode();
        return new ElasticClient(settings);
    });

    // Service Registration
    builder.Services.AddScoped<BookIndexService>();
    builder.Services.AddHostedService<ElasticsearchInitializer>();
}
else
{
    Console.WriteLine("Elasticsearch configuration not found, skipping Elasticsearch services...");

    // Elasticsearch olmadýðýnda BookIndexService'i boþ implementation ile ekle
    builder.Services.AddScoped<BookIndexService>(sp =>
    {
        // Elasticsearch olmadýðýnda null ElasticClient ile çalýþacak þekilde ayarla
        return new BookIndexService(null);
    });
}
// ========================= Elastic =========================

var app = builder.Build();

// --- Automatic migration apply ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        Console.WriteLine("Running database migrations...");
        db.Database.Migrate();
        Console.WriteLine("Database migrations completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration error: {ex.Message}");
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}
// --- Automatic migration apply ---

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "books",
    pattern: "{controller=Books}/{action=Index}/{id?}");

Console.WriteLine($"Starting application on port: {Environment.GetEnvironmentVariable("PORT") ?? "80"}");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");

app.Run();