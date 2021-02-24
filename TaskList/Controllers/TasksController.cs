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
using TaskList.CustomSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskList.Controllers
{
    //[Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class TasksController : ControllerBase
    {
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
        /// The task limit setting
        /// </summary>
        private readonly TaskLimit _maxTaskEntries;

        /// <summary>
        /// Initializes a new instance of the <see cref="TasksController"/> class.
        /// </summary>
        public TasksController(ILogger<TasksController> logger, 
                                       DatabaseContext context, 
                                       IConfiguration configuration, 
                                       IOptions<TaskLimit> taskLimit)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _maxTaskEntries = taskLimit.Value;
        }

        /// <summary>
        /// Creates a new task based on client input.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns>Success message (201)with resource location header.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(TaskResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(List<ErrorResponse>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.Conflict)]
        [Route("api/v1/tasks")]
        public async Task<IActionResult> CreateTask([FromBody] TaskInput payload)
        {
            var newTask = new Models.Task();

            try
            {
                if (ModelState.IsValid)
                {
                    // Validate that the database task limit has not been reached.
                    long totalTasks = (from c in _context.Tasks select c).Count();

                    if (_maxTaskEntries.MaxTaskEntries <= totalTasks)
                    {
                        ErrorResponse errorResponse = new ErrorResponse();

                        errorResponse.errorNumber = 4;
                        errorResponse.parameterName = null;
                        errorResponse.parameterValue = null;
                        errorResponse.errorDescription = "The maximum number of entities have been created. No further entities can be created at this time.";

                        return StatusCode((int)HttpStatusCode.Forbidden, errorResponse);
                    }

                    // Validate that the taskName is unique
                    if (!TaskInput.CheckUniqueTaskName(payload, _context))
                    {
                        ErrorResponse errorResponse = new ErrorResponse();

                        errorResponse.errorNumber = 1;
                        errorResponse.parameterName = "taskName";
                        errorResponse.parameterValue = payload.taskName;
                        errorResponse.errorDescription = "The entity already exists.";

                        return StatusCode((int)HttpStatusCode.Conflict, errorResponse);
                    }

                    newTask.taskName = payload.taskName;
                    newTask.isCompleted = payload.isCompleted;
                    newTask.dueDate = payload.dueDate;

                    _context.Tasks.Add(newTask);

                    _context.SaveChanges();
                }
                else
                {
                    // If model is invalid, create a list of all invalid parameters
                    List<ErrorResponse> errorResponses = new List<ErrorResponse>();

                    using StreamReader sr = new StreamReader(Request.Body);
                    Request.Body.Seek(0, SeekOrigin.Begin);
                    string inputJsonString = await sr.ReadToEndAsync();

                    using (JsonDocument jsonDocument = JsonDocument.Parse(inputJsonString))
                    {
                        // Determine which properties have errors and the property name (key value)
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
            return CreatedAtRoute("GetTaskByIdRoute", new { id = newTask.id }, new TaskResponse(newTask));
        }

        /// <summary>
        /// Updates an existing Task object based on user input.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="payload"></param>
        /// <returns>No content.</returns>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(List<ErrorResponse>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.Conflict)]
        [Route("tasks/{id}")]
        [HttpPatch]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskInput payload)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validate that the taskName is unique (excluding the existing entry for this Task object).
                    if (!TaskInput.CheckUniqueTaskName(payload, _context, id))
                    {
                        ErrorResponse errorResponse = new ErrorResponse();

                        errorResponse.errorNumber = 1;
                        errorResponse.parameterName = "taskName";
                        errorResponse.parameterValue = payload.taskName;
                        errorResponse.errorDescription = "The entity already exists.";

                        return StatusCode((int)HttpStatusCode.Conflict, errorResponse);
                    }

                    Models.Task targetTask = (from c in _context.Tasks where c.id == id select c).SingleOrDefault();

                    // Catch task not found error (404).
                    if (targetTask == null)
                    {
                        _logger.LogInformation(LoggingEvents.GetItem, $"TasksController Task(id=[{id}]) was not found.", id);

                        ErrorResponse errorResponse = new ErrorResponse();

                        errorResponse.errorNumber = 5;
                        errorResponse.parameterName = "id";
                        errorResponse.parameterValue = id.ToString();
                        errorResponse.errorDescription = "The entity could not be found.";

                        return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
                    }

                    targetTask.taskName = payload.taskName;
                    targetTask.isCompleted = payload.isCompleted;
                    targetTask.dueDate = payload.dueDate;

                    _context.SaveChanges();
                }
                else
                {
                    // If model is invalid, create a list of all invalid parameters
                    List<ErrorResponse> errorResponses = new List<ErrorResponse>();

                    // Access to the raw input
                    using StreamReader sr = new StreamReader(Request.Body);
                    Request.Body.Seek(0, SeekOrigin.Begin);
                    string inputJsonString = await sr.ReadToEndAsync();

                    using (JsonDocument jsonDocument = JsonDocument.Parse(inputJsonString))
                    {
                        // Determine which objects have errors knowing the object name (key value)
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
                _logger.LogError(LoggingEvents.InternalError, ex, $"CustomerControlle Customer(id=[{id}]) caused an internal error.", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return NoContent();
        }

        /// <summary>
        /// Deletes a specified Task object from the DB.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>No Content.</returns>
        [HttpDelete]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.NotFound)]
        [Route("tasks/{id}", Name = "GetTaskByIdRoute")]
        public IActionResult DeleteTask(int id)
        {
            try
            {
                Models.Task targetTask = (from c in _context.Tasks where c.id == id select c).SingleOrDefault();

                // Catch task not found error (404).
                if (targetTask == null)
                {
                    _logger.LogInformation(LoggingEvents.GetItem, $"TasksController Task(id=[{id}]) was not found.", id);

                    ErrorResponse errorResponse = new ErrorResponse();

                    errorResponse.errorNumber = 5;
                    errorResponse.parameterName = "id";
                    errorResponse.parameterValue = id.ToString();
                    errorResponse.errorDescription = "The entity could not be found.";

                    return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
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


        /// <summary>
        /// Gets a specified Task object from the DB.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The specified task.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(Models.Task), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [Route("tasks/{id}")]
        public IActionResult GetTaskById(int id)
        {
            try
            {
                Models.Task task = (from c in _context.Tasks where c.id == id select c).SingleOrDefault();

                // Catch task not found error (404).
                if (task == null)
                {
                    _logger.LogInformation(LoggingEvents.GetItem, $"TasksController Task(id=[{id}]) was not found.", id);

                    ErrorResponse errorResponse = new ErrorResponse();

                    errorResponse.errorNumber = 5;
                    errorResponse.parameterName = "id";
                    errorResponse.parameterValue = id.ToString();
                    errorResponse.errorDescription = "The entity could not be found.";

                    return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
                }
                return new ObjectResult(new TaskResponse(task));
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, $"TasksController Task(id=[{id}]) caused an internal error.", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Gets all task objects in the DB.
        /// </summary>
        /// <returns>A list of all tasks.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(Models.Task), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Models.Task), StatusCodes.Status400BadRequest)]

        [Route("tasks")]
        public IActionResult GetAllTasks()
        {
            List<int> ids = (from c in _context.Tasks select c.id).ToList();

            return new ObjectResult(ids);
        }
    }
}
