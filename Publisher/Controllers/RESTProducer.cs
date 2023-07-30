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
                var test = sqLiteRepo.GetJoysticById(id);
                _logger.LogInformation("Data: ", test.ToString());
                return Ok(test);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                throw ex;
            }
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                var test = sqLiteRepo.GetAllJoystics();
                _logger.LogInformation("Data: ", test.Count);

                return Ok(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw ex;
            }
        }
    }
}
