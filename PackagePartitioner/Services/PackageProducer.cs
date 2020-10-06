using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Confluent.Kafka;
using Polly;

namespace PackagePartitioner.Services
{
	public class PackageProducer : IDisposable
	{
		private readonly string _topic;
		private readonly IProducer<long, long> _producer;

		public PackageProducer(string topic)
		{
			_topic = topic;

			var config = new ProducerConfig
			{
				BootstrapServers = "kafka:9092",
				ClientId = topic, // идентификатор клиента == ID узла БД
				EnableIdempotence = true, // идемпотентность - в данной постановке задлачи этого достаточно и транзакциии в продьюсере не нужны
				Acks = Acks.All // подтверждение от всех реплик
			};

			_producer = new ProducerBuilder<long, long>(config).Build();
		}

		public void Dispose()
		{
			_producer?.Dispose();
		}

		public async Task<DeliveryResult<long, long>> Push(long boxId, long containerId)
		{
			return await Policy
				.Handle<ProduceException<long, long>>()
				.OrResult<DeliveryResult<long, long>>(r => r.Status != PersistenceStatus.Persisted)
				.RetryAsync(5)
				.ExecuteAsync(() =>
					_producer.ProduceAsync(_topic, new Message<long, long> {Key = boxId, Value = containerId}));
		}
	}
}
