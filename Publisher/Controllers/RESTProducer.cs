using Microsoft.AspNetCore.Mvc;
using Publisher.Services;

namespace Publisher.Controllers
{
    [ApiController]
    [Route("api/publisher/RESTDataProvider")]
    public class RESTProducer : Controller
    {
        public readonly IDataProducerService dataProducerService;
        public readonly ISqLiteRepo sqLiteRepo;
        private readonly ILogger<RESTProducer> _logger;

        public RESTProducer(IDataProducerService dataProducerService, ISqLiteRepo sqLiteRepo, ILogger<RESTProducer> logger)
        {
            this.dataProducerService = dataProducerService;
            this.sqLiteRepo = sqLiteRepo;
            _logger = logger;
        }

        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                // Retrieve a single Joystick message by its ID using the sqLiteRepo
                var message = sqLiteRepo.GetJoystickById(id);

                // Return the retrieved Joystick message as a successful response
                return Ok(message);
            }
            catch
            {
                throw; // Re-throw the exception to be handled further up the call stack
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

    }
}
