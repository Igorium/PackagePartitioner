using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using PackagePartitioner.Services;

namespace PackagePartitioner.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PackageController : ControllerBase
	{
		private readonly ProducerFacade _producerFacade;

		public PackageController(ProducerFacade producerFacade)
		{
			_producerFacade = producerFacade;
		}

		// get для отладки
		[HttpPost("{boxId}/{containerId}")]
		[HttpGet("{boxId}/{containerId}")] 
		public async Task PutBoxToContainer(long boxId, long containerId)
		{
			await _producerFacade.PutBoxToContainer(boxId, containerId);
		}

		[HttpGet("")]
		public ActionResult<IEnumerable<string>> Index()
		{
			return new[]
			{
				"Put box into container: http://localhost:8080/api/package/123456/654321",
				"Find container of the box: http://localhost:8081/api/package/WhereIsBox/123456",
				"List boxes in container: http://localhost:8081/api/package/ContainerBalance/654321",
			};
		}
	}
}
