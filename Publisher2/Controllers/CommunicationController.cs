using Contracts.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Publisher.Services;
using System.Globalization;

namespace Publisher.Controllers
{
    public class CommunicationController : Controller
    {
        public readonly IRabbitMqSender rabbitMqSender;
        public readonly IKaffkaSender kaffkaSender;
       
        string _sheetPath = @"C:\Users\klaud\source\repos\Publisher\Publisher\joystick_data.csv";

        public CommunicationController(IRabbitMqSender rabbitMqSender, IKaffkaSender kaffkaSender)
        {
            this.rabbitMqSender = rabbitMqSender;
            this.kaffkaSender = kaffkaSender;
        }

        [HttpGet("rabbitMq")]
        public Task SendDataByRabbitMq()
        {
            var data = GetJoysticData();
            rabbitMqSender.Send(data);

            return Task.CompletedTask;
        }


        [HttpGet("kaffka")]
        public Task SendByKaffka()
        {
            var data = GetJoysticData();
            kaffkaSender.Send(data);

            return Task.CompletedTask;
        }

        private IList<Joystic> GetJoysticData()
        {
            IList<Joystic> joysticData = new List<Joystic>();

            //////Read the data
            using (var reader = new StreamReader(_sheetPath))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                };
                using (var csv = new CsvReader(reader, config))
                {
                    var joystickData = csv.GetRecords<Joystic>();
                    joysticData = joystickData.ToList();
                }
            }
            return joysticData;
        }
    }
}
