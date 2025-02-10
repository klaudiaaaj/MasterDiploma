using Contracts.Models;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitmqSubscriberDirectExchange.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Channels;

namespace RabbitmqSubscriber.Controllers
{
    [ApiController]
    [Route("api/rabbitMqConsumer/direct")]
    public class RabbitMqClientController : Controller
    {
        private readonly IConfiguration _configuration;
           private readonly IRabbitMqConnectionFactory rabbitMqConnectionFactory;

        public RabbitMqClientController(IConfiguration configuration, IRabbitMqConnectionFactory rabbitMqConnectionFactory)
        {
            _configuration = configuration;
            this.rabbitMqConnectionFactory = rabbitMqConnectionFactory;
        }


        [HttpGet("single")]
        public IActionResult GetSingleObjectById()
        {
            try
            {
                rabbitMqConnectionFactory.Channel.QueueDeclare(queue: rabbitMqConnectionFactory.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(rabbitMqConnectionFactory.Channel);
                BasicGetResult result = rabbitMqConnectionFactory.Channel.BasicGet(rabbitMqConnectionFactory.QueueName, autoAck: true);

                if (result != null)
                {
                    var data = Encoding.UTF8.GetString(result.Body.ToArray());
                    return Ok(data);
                }
                else
                {
                    return NotFound("No data available in the queue.");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the process
                // Return an HTTP 500 Internal Server Error response with the error message
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("all")]
        public IActionResult GetAllData()
        {
            try
            {
                // Declare a queue with specific properties
                rabbitMqConnectionFactory.Channel.QueueDeclare(queue: rabbitMqConnectionFactory.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var dataList = new List<string>(); // Initialize a list to store retrieved data

                // Continuously retrieve messages from the queue until it is empty
                while (true)
                {
                    // Attempt to retrieve a message from the queue
                    BasicGetResult result = rabbitMqConnectionFactory.Channel.BasicGet(rabbitMqConnectionFactory.QueueName, autoAck: true);

                    if (result != null)
                    {
                        // Convert the message body to a UTF-8 encoded string and add to the list
                        var data = Encoding.UTF8.GetString(result.Body.ToArray());
                        dataList.Add(data);
                    }
                    else
                    {
                        // Exit the loop if the queue is empty
                        break;
                    }
                }

                if (dataList.Count > 0)
                {
                    // Return an HTTP 200 OK response containing the list of retrieved data
                    return Ok(dataList);
                }
                else
                {
                    // If no data was retrieved, return an HTTP 404 NotFound response
                    return NotFound("No data available in the queue.");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the process
                // Return an HTTP 500 Internal Server Error response with the error message
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    }
}
