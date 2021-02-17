using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TaskList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {

        /// <summary>
        /// The database of tasks.
        /// </summary>
        private static Dictionary<int, Task> _tasks = new Dictionary<int, Task>();

        // POST api/<TasksController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // GET: api/<TasksController>
        [HttpGet]
        [ProducesResponseType(typeof(Task), StatusCodes.Status200OK)]
        [Route("api/v1/tasks")]
        public IActionResult Get()
        {
            return new ObjectResult(_tasks.Keys.ToArray());
        }

        // GET api/<TasksController>/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Task), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        [Route("api/v1/tasks/{id}")]
        public IActionResult Get(int id)
        {
            return "value";
        }
    }
}
