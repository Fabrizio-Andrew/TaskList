﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskList.DataTransferObjects;
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
        /// The database of tasks.
        /// </summary>
        private static Dictionary<int, DataTransferObjects.Task> _tasks = new Dictionary<int, DataTransferObjects.Task>();

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomersController"/> class.
        /// </summary>
        public TasksController(ILogger<TasksController> logger)
        {
            _logger = logger;
        }


        private static DateTime firstdate = DateTime.Parse("2021-02-03");
        static TasksController()
        {
            // Initialize Data
            _tasks.Add(1, new DataTransferObjects.Task() { id = 1, taskName = "Buy groceries", isCompleted = false, dueDate = DateTime.Parse("2021-02-03") });
            _tasks.Add(2, new DataTransferObjects.Task() { id = 2, taskName = "Workout", isCompleted = true, dueDate = DateTime.Parse("2021-01-01") });
            _tasks.Add(3, new DataTransferObjects.Task() { id = 3, taskName = "Paint fence", isCompleted = false, dueDate = DateTime.Parse("2021-03-15") });
            _tasks.Add(4, new DataTransferObjects.Task() { id = 4, taskName = "Mow Lawn", isCompleted = false, dueDate = DateTime.Parse("2021-06-11") });
        }

        // GET: api/<TasksController>
        [HttpGet]
        [ProducesResponseType(typeof(DataTransferObjects.Task), StatusCodes.Status200OK)]
        [Route("tasks")]
        public IActionResult GetAllTasks()
        {
            return new ObjectResult(_tasks.Keys.ToArray());
        }

        // POST api/<TasksController>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        [Route("tasks")]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreate payload)
        {
            var newTask = new DataTransferObjects.Task();

            try
            {
                if (ModelState.IsValid)
                {
                    newTask.taskName = payload.taskName;
                    newTask.isCompleted = payload.isCompleted;
                    newTask.dueDate = payload.dueDate;
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
            //catch (KeyNotFoundException knfEx)
            //{
            //    _logger.LogInformation(LoggingEvents.GetItem, knfEx, "TasksController Customer(id=[{id}]) was not found.", id);
            //    return NotFound();
            //}
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, "TasksController Customer(id=[{id}]) caused an internal error.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

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
                    _tasks[id] = new DataTransferObjects.Task()
                    {
                        id = id,
                        taskName = payload.taskName,
                        isCompleted = payload.isCompleted,
                        dueDate = payload.dueDate,
                    };
                }
                else
                {
                    List<ErrorResponse> errorResponses = new List<ErrorResponse>();


                    // DEMO: Enable multi-stream read
                    // The EnableMultipleStreamReadMiddleware is needed for reading from the
                    // Request Body a second time, the first time the Request.Body is read
                    // is in the middleware for deserializing the Customer Input

                    // This allows us access to the raw input
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
            catch (KeyNotFoundException knfEx)
            {
                _logger.LogInformation(LoggingEvents.GetItem, knfEx, "CustomerController Customer(id=[{id}]) was not found.", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, "CustomerControlle Customer(id=[{id}]) caused an internal error.", id);
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
            return NoContent();
        }


        // GET api/<TasksController>/5
        [HttpGet]
        [ProducesResponseType(typeof(DataTransferObjects.Task), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        [Route("tasks/{id}")]
        public IActionResult GetTaskById(int id)
        {
            try
            {
                if (id < 1)
                {
                    throw new Exception("Invalid ID.");
                }
                return new ObjectResult(_tasks[id]);
            }
            catch (KeyNotFoundException knfEx)
            {
                _logger.LogInformation(LoggingEvents.GetItem, knfEx, "CustomerController Customer(id=[{id}]) was not found.", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, "CustomerControlle Customer(id=[{id}]) caused an internal error.", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
