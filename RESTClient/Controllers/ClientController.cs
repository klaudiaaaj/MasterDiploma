using Microsoft.AspNetCore.Mvc;

namespace RESTClient.cs.Controllers
{
    [ApiController]
    [Route("api/RestClient")]
    public class ClientController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IHttpClientFactory httpClientFactory, ILogger<ClientController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetDataAll()
        {
            var response = await _httpClient.GetAsync("http://host.docker.internal:8080/RestGetAll");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return Ok(data);
            }
            else
            {
                _logger.LogError("Error", response.ToString());

                return StatusCode((int)response.StatusCode);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDataById(string id)
        {
            var response = await _httpClient.GetAsync($"http://host.docker.internal:8080/RetGetById/{id}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                _logger.LogInformation(data.ToString());
                    return Ok(data);
            }
            else
            {
                _logger.LogError("Error", response.ToString());
                return StatusCode((int)response.StatusCode);
            }
        }
    }
}
