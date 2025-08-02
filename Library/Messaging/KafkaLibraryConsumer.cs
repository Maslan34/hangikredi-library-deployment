using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Library.Services;

namespace Library.Messaging
{
    public class KafkaLibraryConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public KafkaLibraryConsumer(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield(); // <-- Bu satır host'un startup'u engellemesini engeller
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = "book-index-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe("book-topic");

            Console.WriteLine("Kafka Consumer started, waiting for messages...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(TimeSpan.FromMilliseconds(500));
                    if (result != null)
                    {
                        var message = result.Message.Value;
                        Console.WriteLine($"Kafka Consumer - Message received: {message}");

                        using var jsonDoc = System.Text.Json.JsonDocument.Parse(message);
                        var root = jsonDoc.RootElement;

                        if (root.TryGetProperty("Event", out var eventProperty))
                        {
                            var eventType = eventProperty.GetString();

                            using var scope = _scopeFactory.CreateScope();
                            var bookIndexService = scope.ServiceProvider.GetRequiredService<BookIndexService>();

                            if (eventType == "BookCreated")
                            {
                                var bookJson = root.GetProperty("Data").GetRawText();
                                var book = System.Text.Json.JsonSerializer.Deserialize<Library.Models.Book>(bookJson);

                                if (book != null)
                                {
                                    await bookIndexService.IndexBookAsync(book);
                                    Console.WriteLine("Kafka Consumer - Book indexing completed (create).");
                                }
                            }
                            else if (eventType == "BookDeleted")
                            {
                                var data = root.GetProperty("Data");
                                var bookId = data.GetProperty("Id").GetInt32();

                                await bookIndexService.DeleteBookAsync(bookId);
                                Console.WriteLine("Kafka Consumer - Book indexing completed (delete).");
                            }
                            else
                            {
                                Console.WriteLine($"Kafka Consumer - Unknown event type: {eventType}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Kafka Consumer - 'Event' field not found.");
                        }
                    }
                }
                catch (Confluent.Kafka.ConsumeException ex)
                {
                    Console.WriteLine($"Kafka Consumer Error: {ex.Error.Reason}");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Kafka Consumer unexpected error: {ex.Message}");
                }
            }

            consumer.Close();
            Console.WriteLine("Kafka Consumer service stopped.");
        }
    }
}
