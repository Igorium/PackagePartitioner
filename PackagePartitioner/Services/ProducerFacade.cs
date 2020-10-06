using System;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace PackagePartitioner.Services
{
	public class ProducerFacade : IDisposable
	{
		private PackageProducer[] _producers;

		public ProducerFacade()
		{
			// один продюсер на каждый узел
			_producers = Sharding.Map.Nodes.Select(n => new PackageProducer(n)).ToArray();
		}

		public async Task PutBoxToContainer(long boxId, long containerId)
		{
			var shardNo = Sharding.Map.GetShardNo(boxId);
			var result = await _producers[shardNo].Push(boxId, containerId);

			if (result.Status != PersistenceStatus.Persisted)
				throw new ApplicationException("Message can't be saved");
		}

		public void Dispose()
		{
			if (_producers != null)
			{
				foreach (var producer in _producers)
				{
					producer.Dispose();
				}
			}
		}
	}
}
