using Contracts.Models;
using Microsoft.AspNetCore.Mvc;
using Publisher.Services;
using System.Net.Http;
using System.Text;

namespace Publisher.Controllers
{
    [ApiController]
    [Route("api/publisher/RESTDataProvider")]
    public class RESTProducer : Controller
    {
        public readonly IDataProducerService dataProducerService;
        public readonly ISqLiteRepo sqLiteRepo;
        private readonly ILogger<RESTProducer> _logger;
        public readonly IList<Joystick> iJoystickList;
        private readonly HttpClient _httpClient;


        public RESTProducer(IDataProducerService dataProducerService, ISqLiteRepo sqLiteRepo, ILogger<RESTProducer> logger, HttpClient httpClient)
        {
            this.dataProducerService = dataProducerService;
            this.sqLiteRepo = sqLiteRepo;
            _logger = logger;
            iJoystickList = dataProducerService.GetJoystickData();
            _httpClient = httpClient;
        }

        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var message = sqLiteRepo.GetJoystickById(id);
                return Ok(message);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                // Retrieve all Joystick messages using the sqLiteRepo
                var messages = sqLiteRepo.GetAllJoysticks();

                // Return the list of Joystick messages as a successful response
                return Ok(messages);
            }
            catch
            {
                throw; // Re-throw the exception to be handled further up the call stack
            }
        }
        [HttpGet("RestObjectGenerator")]
        public async Task<IActionResult> SimulateObjectGeneration()
        {
            foreach (var joystick in iJoystickList)
            {
                var stringObject = String.Join(",", joystick.time, joystick.axis_1, joystick.axis_2,
                joystick.button_1, joystick.button_2, joystick.ToString());
                var requestData = new { Data = stringObject };
                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"http://host.docker.internal:8081/api/RestClient/object", content);
                // Check if the HTTP response is successful
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error", response.ToString());
                }
            }
            return Ok(DateTime.Now);
        }

    }
}
