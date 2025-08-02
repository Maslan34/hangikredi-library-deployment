using Confluent.Kafka;
using Library.Models;
using System.Text.Json;

public class KafkaLibraryProducer
{
    private readonly IProducer<Null, string> _producer;

    public KafkaLibraryProducer(IProducer<Null, string> producer)
    {
        _producer = producer;
    }

    public async Task PublishBookCreatedAsync(Book book)
    {
        var messageObj = new
        {
            Event = "BookCreated",
            Data = book
        };
        var json = JsonSerializer.Serialize(messageObj);
        Console.WriteLine("Serialized (create): " + json);
        await _producer.ProduceAsync("book-topic", new Message<Null, string> { Value = json });
    }

    public async Task PublishBookDeletedAsync(int bookId)
    {
        var messageObj = new
        {
            Event = "BookDeleted",
            Data = new { Id = bookId }
        };
        var json = JsonSerializer.Serialize(messageObj);
        Console.WriteLine("Serialized (delete): " + json);
        await _producer.ProduceAsync("book-topic", new Message<Null, string> { Value = json });
    }
}
