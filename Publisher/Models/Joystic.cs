using CsvHelper.Configuration.Attributes;

namespace Contracts.Models
{
    public class Joystic
    {
        [Optional]
        public string time { get; set; }
        public string axis_1 { get; set; }
        public string axis_2 { get; set; }
        public string button_1 { get; set; }
        public string button_2 { get; set; }
        public int? id { get; set; }
    }
}
