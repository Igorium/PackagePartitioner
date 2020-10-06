using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace PackageReader.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PackageController : ControllerBase
	{
		private readonly NpgsqlConnection _conn;

		public PackageController(NpgsqlConnection conn)
		{
			_conn = conn;
		}

		[HttpGet("WhereIsBox/{boxId}")]
		public async Task<ActionResult<long>> WhereIsBox(long boxId)
		{
			// TODO слой доступа
			_conn.Open();

			var node = Sharding.Map.Nodes[Sharding.Map.GetShardNo(boxId)];
			
			var sql = $"SELECT packageid FROM \"{node}\" where boxid = {boxId}";

			using (var cmd = new NpgsqlCommand(sql, _conn))
				return (long)(await cmd.ExecuteScalarAsync());
		}

		[HttpGet("ContainerBalance/{containerId}")]
		public async Task<ActionResult<IEnumerable<long>>> ContainerBalance(long containerId)
		{
			_conn.Open();

			var countDown = new CountdownEvent(Sharding.Map.Nodes.Length);
			var result = new ConcurrentQueue<long>();

			// выборку по вторичному индексу в данной реализации необходимо производить на всех узлах
			// можно реализовать поддержку 2х primary индексов, две таблицы: boxId/containerId и containerId/boxId
			// но это сильно усложнит путь записи и поддержку консистентности
			foreach (var node in Sharding.Map.Nodes)
			{
				try
				{
					var sql = $"SELECT boxid FROM \"{node}\" where packageid = {containerId}";

					using (var cmd = new NpgsqlCommand(sql, _conn))
						using (var r = await cmd.ExecuteReaderAsync())
							while (await r.ReadAsync())
								result.Enqueue(r.GetInt64(0));
				}
				finally
				{
					countDown.Signal();
				}
			}

			countDown.Wait();

			return result;
		}

	}
}
