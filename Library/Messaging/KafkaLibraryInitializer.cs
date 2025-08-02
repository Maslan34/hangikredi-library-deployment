namespace Library.Messaging
{
    using Confluent.Kafka;
    using Confluent.Kafka.Admin;
    using System;
    using System.Threading.Tasks;

    public class KafkaLibraryInitializer
    {
        private readonly string _bootstrapServers;

        public KafkaLibraryInitializer(string bootstrapServers)
        {
            _bootstrapServers = bootstrapServers;
        }

        public async Task CreateTopicAsync(string topicName, int numPartitions = 1, short replicationFactor = 1)
        {
            var config = new AdminClientConfig { BootstrapServers = _bootstrapServers };

            using var adminClient = new AdminClientBuilder(config).Build();

            try
            {
                await adminClient.CreateTopicsAsync(new TopicSpecification[]
                {
                new TopicSpecification
                {
                    Name = topicName,
                    NumPartitions = numPartitions,
                    ReplicationFactor = replicationFactor
                }
                });
                Console.WriteLine($"Topic '{topicName}' created successfully.");
            }
            catch (CreateTopicsException e)
            {
                 // Throws an error if the topic already exists
                if (e.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
                {
                    Console.WriteLine($"Topic '{topicName}' already exists.");
                }
                else
                {
                    Console.WriteLine($"Topic creation error: {e.Results[0].Error.Reason}");
                    throw;
                }
            }
        }
    }

}
