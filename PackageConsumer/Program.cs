using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace PackageConsumer
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

			var consumers = Sharding.Map.Nodes.Select(n => new PackageConsumer(n, new NpgsqlConnection(connectionString))).ToArray();

			foreach (var packageConsumer in consumers)
			{
				Task.Factory.StartNew(() => packageConsumer.Consume());
			}


			Task.Delay(TimeSpan.FromHours(1)).Wait(); // для отладки

			foreach (var packageConsumer in consumers)
			{
				packageConsumer.Stop();
			}

		}
	}
}
