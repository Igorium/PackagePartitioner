using System;
using System.Linq;

namespace Sharding
{
	public static class Map
	{
		// физические узлы БД
		public static string[] Nodes => new[]
		{
			"partition-topic-0",
			"partition-topic-1",
			"partition-topic-2"
		};

		private const int ScaleFactor = 10;

		// максимальное кол-во секций до которого можем масштабироваться
		private static readonly int PartitionsCount = Nodes.Length * ScaleFactor;
		// маппинг секций на узлы
		private static readonly int[] PartitionsShardsMap;

		static Map()
		{
			PartitionsShardsMap = new int[PartitionsCount];

			for (var i = 0; i < PartitionsCount; i++)
			{
				PartitionsShardsMap[i] = i/ScaleFactor;
			}
		}

		// если boxId не число - предварительно хешируем его FNV64
		public static int GetShardNo(long boxId)
		{
			var partitionNo = boxId % PartitionsCount;
			// получаем номер узла где находится данная секция
			var shardNo = PartitionsShardsMap[partitionNo];

			return shardNo;
		}
	}
}
