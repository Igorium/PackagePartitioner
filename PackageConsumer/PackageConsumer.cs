using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Npgsql;

namespace PackageConsumer
{
	public class PackageConsumer
	{
		private readonly string _topic;
		private readonly NpgsqlConnection _conn;
		private readonly ConsumerConfig _config;

		private volatile bool _isActive;
		private readonly object _syncRoot = new object();
		private CancellationTokenSource _tokenSource;

		public PackageConsumer(string topic, NpgsqlConnection conn)
		{
			_topic = topic;
			_conn = conn;

			_config = new ConsumerConfig
			{
				BootstrapServers = "kafka:9092",
				ClientId = _topic, // идентификатор клиента == ID узла БД
				GroupId = "package-partitioner-group",
				EnableAutoCommit = false, // для обеспечения exactly once комитим вручную
				EnableAutoOffsetStore = false,
				IsolationLevel = IsolationLevel.ReadCommitted,
			};
		}

		public async Task<long> ReadOffset()
		{
			_conn.Open();
			var sql = $"SELECT topic_offset FROM \"{_topic}-offset\"";

			using (var cmd = new NpgsqlCommand(sql, _conn))
			{
				var res = await cmd.ExecuteScalarAsync();
				return (long)res;
			}
		}

		public void Consume()
		{
			if (_isActive)
				return;

			lock (_syncRoot)
			{
				if (_isActive)
					return;

				_isActive = true;
			}

			IConsumer<long, long> consumer = null;
			_tokenSource = new CancellationTokenSource();

			try
			{
				consumer = new ConsumerBuilder<long, long>(_config)
					.SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}")) // todo
					.Build();

				// последний подтвержденный offset
				var offset = ReadOffset().Result;
				consumer.Assign(new TopicPartitionOffset(_topic, 0, offset));

				while (_isActive)
				{
					try
					{
						var consumeResult = consumer.Consume(_tokenSource.Token);

						// TODO для улучшения производительности читать/записывать пачками и подтверждать offset в конце пачки

						var insertBox = $"Insert into \"{_topic}\"(boxid,packageid) values( {consumeResult.Message.Key}, {consumeResult.Message.Value})";
						var updateOffset = $"UPDATE \"{_topic}-offset\" SET topic_offset = {consumeResult.Offset.Value}";

						// так как писатель один, то ReadCommitted вполне достаточно
						using (var transaction = _conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
						{
							try
							{
								using (var cmd = new NpgsqlCommand(insertBox, _conn, transaction))
									cmd.ExecuteNonQuery();

								using (var cmd = new NpgsqlCommand(updateOffset, _conn, transaction))
									cmd.ExecuteNonQuery();

								transaction.Commit();
							}
							catch (Exception)
							{
								transaction.Rollback();
								// todo повторить запись в БД
							}
						}

						// Дорогой синхронный коммит
						consumer.Commit(consumeResult);
					}
					catch (ConsumeException e)
					{
						Console.WriteLine($"Consume error: {e.Error.Reason}"); // todo
					}
				}
			}
			catch (OperationCanceledException)
			{
				Console.WriteLine("Closing consumer.");
			}
			finally
			{
				consumer?.Dispose();
				_tokenSource?.Dispose();
				_tokenSource = null;
				_isActive = false;
			}
		}

		public void Stop()
		{
			if (!_isActive)
				return;

			_isActive = false;
			_tokenSource?.Cancel();
		}
	}

}
