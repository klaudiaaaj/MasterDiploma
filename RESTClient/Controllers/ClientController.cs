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
            // Send an HTTP GET request to an external REST API to fetch all data
            var response = await _httpClient.GetAsync("http://host.docker.internal:8080/api/publisher/RESTDataProvider/GetAll");

            // Check if the HTTP response is successful
            if (response.IsSuccessStatusCode)
            {
                // Read the content of the response as a string
                var data = await response.Content.ReadAsStringAsync();

                // Return the retrieved data as a successful response
                return Ok(data);
            }
            else
            {
                // Log an error message with the response status
                _logger.LogError("Error", response.ToString());

                // Return a response with the same status code as the external API response
                return StatusCode((int)response.StatusCode);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDataById(string id)
        {
            // Send an HTTP GET request to an external REST API
            var response = await _httpClient.GetAsync($"http://host.docker.internal:8080/api/publisher/RESTDataProvider/GetById/{id}");

            // Check if the HTTP response is successful
            if (response.IsSuccessStatusCode)
            {
                // Read the content of the response as a string
                var data = await response.Content.ReadAsStringAsync();

                // Log the retrieved data
                _logger.LogInformation(data.ToString());

                // Return the retrieved data as a successful response
                return Ok(data);
            }
            else
            {
                // Log an error message with the response status
                _logger.LogError("Error", response.ToString());

                // Return a response with the same status code as the external API response
                return StatusCode((int)response.StatusCode);
            }
        }

    }
}
