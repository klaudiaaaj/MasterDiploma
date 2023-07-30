using Contracts.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Publisher.Services
{
    public class DataProducerService : IDataProducerService
    {
        string _sheetPath = @"joystick_data.csv";

        public IList<Joystic> GetJoysticData()
        {
            IList<Joystic> joysticData = new List<Joystic>();
            IList<Joystic> joysticData2 = new List<Joystic>();

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
                    foreach (var joystick in joystickData.Take(1000))
                    {
                        joysticData2.Add(joystick);
                    }
                    joysticData = joysticData.ToList();
                }
            }
            return joysticData2;
        }
    }
}
