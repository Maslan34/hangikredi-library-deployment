namespace Library.Services
{
    
    using Confluent.Kafka.Admin;
    using Confluent.Kafka;
    using Microsoft.Extensions.Hosting;


    public class KafkaInitializerService : IHostedService
    {
        private readonly IConfiguration _configuration;

        public KafkaInitializerService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var bootstrapServers = _configuration.GetSection("Kafka")["BootstrapServers"];
            var topicName = "book-topic";

            var config = new AdminClientConfig
            {
                BootstrapServers = bootstrapServers
            };

            using var adminClient = new AdminClientBuilder(config).Build();

            try
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                if (metadata.Topics.Any(t => t.Topic == topicName))
                {
                    Console.WriteLine($"----> Kafka topic '{topicName}' already exists.");
                    return;
                }

                await adminClient.CreateTopicsAsync(new TopicSpecification[]
                {
                new TopicSpecification
                {
                    Name = topicName,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
                });

                Console.WriteLine($"---> Kafka topic '{topicName}' created successfully.");

            }
            catch (CreateTopicsException e)
            {
                Console.WriteLine($"---> Kafka topic creation failed: {e.Results[0].Error.Reason}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

}
