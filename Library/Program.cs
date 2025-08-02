
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

// PostgreSQL connection string (from appsettings.json or directly)

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configure DbContext for PostgreSQL

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ========================= PostgreSQL =========================



// ========================= Kafka =========================

// Kafka Producer registration

builder.Services.AddSingleton<IProducer<Null, string>>(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = builder.Configuration.GetSection("Kafka")["BootstrapServers"]
    };
    return new ProducerBuilder<Null, string>(config).Build();
});

//Adding Kafka producer to services
builder.Services.AddSingleton<KafkaLibraryProducer>();

// Register Kafka initializer as a HostedService
builder.Services.AddHostedService<KafkaInitializerService>();

// Add Kafka consumer as a background service

builder.Services.AddHostedService<KafkaLibraryConsumer>();

// ========================= Kafka =========================



// ========================= Elastic =========================

// Elasticsearch client
builder.Services.AddSingleton<ElasticClient>(sp =>
{
    var settings = new ConnectionSettings(new Uri("http://elasticsearch:9200"))
        .DefaultIndex("books")
        .BasicAuthentication("elastic", "PwkDC4KKmTKSA0_z9zI7")
        .DisableDirectStreaming()
        .EnableDebugMode();
    return new ElasticClient(settings);
});

// Service Registration
builder.Services.AddScoped<BookIndexService>();
builder.Services.AddHostedService<ElasticsearchInitializer>();

// ========================= Elastic =========================



var app = builder.Build();

// --- Automatic migration apply ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
// --- Automatic migration apply ---


app.UseExceptionHandler("/Home/Error");
app.UseHsts();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "books",
    pattern: "{controller=Books}/{action=Index}/{id?}");

app.Run();
