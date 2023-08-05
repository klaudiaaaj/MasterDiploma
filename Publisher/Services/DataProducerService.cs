using Contracts.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Publisher.Services
{
    public class DataProducerService : IDataProducerService
    {
        string _sheetPath = @"Joystickk_data.csv";

        public IList<Joystick> GetJoystickData()
        {
            IList<Joystick> JoystickData = new List<Joystick>();
            IList<Joystick> JoystickData2 = new List<Joystick>();

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
                    var JoystickkData = csv.GetRecords<Joystick>();
                    foreach (var Joystickk in JoystickkData.Take(300000))
                    {
                        JoystickData2.Add(Joystickk);
                    }
                    JoystickData = JoystickData.ToList();
                }
            }
            return JoystickData2;
        }
    }
}
