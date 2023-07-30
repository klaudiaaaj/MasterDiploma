using AzureServiceBusSubscriber.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using System.Text;

namespace AzureServiceBusSubscriber
{

    [ApiController]
    [Route("api/asbConsumer/queue")]
    public class AzureServiceBusClientQueue : Controller
    {
        private readonly IConfiguration _configuration;

        public AzureServiceBusClientQueue(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet("single")]
        public async Task<IActionResult> GetSingleData()
        {
            try
            {
                var messageReceivedTaskCompletionSource = new TaskCompletionSource<string>();

                var queueClient = new QueueClient(_configuration["AzureConnectionString"], _configuration["Azure_QueueName"], ReceiveMode.PeekLock);

                queueClient.RegisterMessageHandler(
                    async (message, token) =>
                    {
                        try
                        {
                            var messageBody = Encoding.UTF8.GetString(message.Body);
                            Console.WriteLine($"Received: {messageBody}, time: {DateTime.Now}");
                            await queueClient.CompleteAsync(message.SystemProperties.LockToken);
                            messageReceivedTaskCompletionSource.SetResult(messageBody); // Ustawienie wyniku TaskCompletionSource po odbiorze wiadomości
                            await queueClient.CloseAsync();
                        }
                        catch (Exception ex)
                        {
                            messageReceivedTaskCompletionSource.SetException(ex); // Ustawienie wyjątku TaskCompletionSource w przypadku błędu
                        }
                    },
                    new MessageHandlerOptions(async args => Console.WriteLine(args.Exception))
                    { MaxConcurrentCalls = 1, AutoComplete = true });

                var receivedMessage = await messageReceivedTaskCompletionSource.Task; // Czekaj na dostanie wiadomości lub na wyjątek


                return Ok(receivedMessage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllData()
        {
            List<string> recivedMessages = new List<string>();
            try
            {
                var queueClient = new QueueClient(_configuration["AzureConnectionString"], _configuration["Azure_QueueName"], ReceiveMode.PeekLock);

                var messageHandlerOptions = new MessageHandlerOptions(async args => Console.WriteLine(args.Exception))
                { MaxConcurrentCalls = 1, AutoComplete = true };

                queueClient.RegisterMessageHandler(async (message, token) =>
                {
                    try
                    {
                        while (recivedMessages.Count() < 50)
                        {
                            var messageBody = Encoding.UTF8.GetString(message.Body);
                            Console.WriteLine($"Received: {messageBody}, time: {DateTime.Now}");
                            recivedMessages.Add(messageBody); // Dodanie wiadomości do listy
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                    }
                }, messageHandlerOptions);

                await Task.Delay(TimeSpan.FromSeconds(20)); // Odczekaj pewien czas na odebranie wiadomości

                await queueClient.CloseAsync();
            }
            catch (Exception ex)
            {
                // Obsługa błędu, jeśli wystąpił wyjątek podczas odbierania wiadomości
                Console.WriteLine($"Exception occurred during message processing: {ex.Message}");
            }

            return Ok(recivedMessages);
        }

    }
}
