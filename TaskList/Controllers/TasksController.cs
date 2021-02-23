using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskList.DataTransferObjects;
using TaskList.Models;
using TaskList.Data;
using TaskList.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TaskList.Controllers
{
    //[Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        /// <summary>
        /// The get customer by identifier route
        /// </summary>
        private const string GetTaskByIdRoute = "GetTaskByIdRoute";

        /// <summary>
        /// The database context
        /// </summary>
        private readonly DatabaseContext _context;

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The configuration instance
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TasksController"/> class.
        /// </summary>
        public TasksController(ILogger<TasksController> logger, DatabaseContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;

            // TO-DO: Implement Task limit
        }

        // GET: api/<TasksController>
        [HttpGet]
        [ProducesResponseType(typeof(Models.Task), StatusCodes.Status200OK)]
        [Route("tasks")]
        public IActionResult GetAllTasks()
        {
            List<int> ids = (from c in _context.Tasks select c.id).ToList();

            return new ObjectResult(ids);
        }

        // POST api/<TasksController>
        [HttpPost]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        [Route("tasks")]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreate payload)
        {
            var newTask = new Models.Task();

            try
            {
                if (ModelState.IsValid)
                {
                    // TO-DO: Add customer limit validation
                    //if (!CanAddMoreCustomers())
                    //{
                    //    return StatusCode((int)HttpStatusCode.Forbidden, $"Customer limit reached MaxCustomers: [{_customerLimits.MaxCustomers}]");
                    //}

                    newTask.taskName = payload.taskName;
                    newTask.isCompleted = payload.isCompleted;
                    newTask.dueDate = payload.dueDate;

                    _context.Tasks.Add(newTask);

                    _context.SaveChanges();
                }
                else
                {
                    List<ErrorResponse> errorResponses = new List<ErrorResponse>();

                    using StreamReader sr = new StreamReader(Request.Body);
                    Request.Body.Seek(0, SeekOrigin.Begin);
                    string inputJsonString = await sr.ReadToEndAsync();

                    using (JsonDocument jsonDocument = JsonDocument.Parse(inputJsonString))
                    {
                        // This is an approach for determining which properties have errors and knowing the
                        // property name as its the key value
                        foreach (string key in ModelState.Keys)
                        {
                            if (ModelState[key].ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                            {
                                foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in ModelState[key].Errors)
                                {
                                    string cleansedKey = key.CleanseModelStateKey();
                                    string camelCaseKey = cleansedKey.ToCamelCase();

                                    System.Diagnostics.Trace.WriteLine($"MODEL ERROR: key:{cleansedKey} attemtedValue:{jsonDocument.RootElement.GetProperty(camelCaseKey)}, errorMessage:{error.ErrorMessage}");

                                    ErrorResponse errorResponse = new ErrorResponse();
                                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage(error.ErrorMessage);
                                    errorResponse.parameterName = camelCaseKey;
                                    errorResponse.parameterValue = jsonDocument.RootElement.GetProperty(camelCaseKey).ToString();
                                    errorResponses.Add(errorResponse);
                                }
                            }
                        }
                    }
                    return BadRequest(errorResponses);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, "TasksController Customer(id=[{id}]) caused an internal error.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            // DO I NEED ANYTHING ELSE IN THESE HEADERS?
            return new ObjectResult(newTask);
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        [Route("tasks/{id}")]
        [HttpPatch]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskUpdate payload)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    Models.Task targetTask = (from c in _context.Tasks where c.id == id select c).SingleOrDefault();

                    targetTask.taskName = payload.taskName;
                    targetTask.isCompleted = payload.isCompleted;
                    targetTask.dueDate = payload.dueDate;

                    _context.SaveChanges();
                }
                else
                {
                    List<ErrorResponse> errorResponses = new List<ErrorResponse>();

                    // Access to the raw input
                    using StreamReader sr = new StreamReader(Request.Body);
                    Request.Body.Seek(0, SeekOrigin.Begin);
                    string inputJsonString = await sr.ReadToEndAsync();

                    using (JsonDocument jsonDocument = JsonDocument.Parse(inputJsonString))
                    {
                        // Determine which objects have errors knowing the object name as its the key value
                        foreach (string key in ModelState.Keys)
                        {
                            if (ModelState[key].ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                            {
                                foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in ModelState[key].Errors)
                                {
                                    string cleansedKey = key.CleanseModelStateKey();
                                    string camelCaseKey = cleansedKey.ToCamelCase();

                                    System.Diagnostics.Trace.WriteLine($"MODEL ERROR: key:{cleansedKey} attemtedValue:{jsonDocument.RootElement.GetProperty(camelCaseKey)}, errorMessage:{error.ErrorMessage}");

                                    ErrorResponse errorResponse = new ErrorResponse();
                                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage(error.ErrorMessage);
                                    errorResponse.parameterName = camelCaseKey;
                                    errorResponse.parameterValue = jsonDocument.RootElement.GetProperty(camelCaseKey).ToString();
                                    errorResponses.Add(errorResponse);
                                }
                            }
                        }
                    }

                    return BadRequest(errorResponses);
                }
            }
            catch (KeyNotFoundException knfEx)
            {
                _logger.LogInformation(LoggingEvents.GetItem, knfEx, $"CustomerController Customer(id=[{id}]) was not found.", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, $"CustomerControlle Customer(id=[{id}]) caused an internal error.", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        [HttpDelete]
        [ProducesResponseType(typeof(int), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(int), StatusCodes.Status404NotFound)]
        [Route("tasks/{id}")]
        public IActionResult DeleteTask(int id)
        {
            try
            {
                Models.Task targetTask = (from c in _context.Tasks where c.id == id select c).SingleOrDefault();
                if (targetTask == null)
                {
                    _logger.LogInformation(LoggingEvents.GetItem, $"TasksController Task(id=[{id}]) was not found.", id);
                    return NotFound();
                }
                _context.Tasks.Remove(targetTask);

                _context.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, $"TasksController Task(id=[{id}]) caused an internal error.", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }


        // GET api/<TasksController>/5
        [HttpGet]
        [ProducesResponseType(typeof(Models.Task), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        [Route("tasks/{id}")]
        public IActionResult GetTaskById(int id)
        {
            try
            {
                // TO-DO: Understand
                Models.Task task = (from c in _context.Tasks where c.id == id select c).SingleOrDefault();

                if (task == null)
                {
                    _logger.LogInformation(LoggingEvents.GetItem, $"TasksController Task(id=[{id}]) was not found.", id);
                    return NotFound();
                }

                return new ObjectResult(new TaskResponse(task));
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, $"TasksController Task(id=[{id}]) caused an internal error.", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
