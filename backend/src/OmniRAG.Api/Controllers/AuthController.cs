using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace OmniRAG.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("dev-token")]
        public async Task<IActionResult> GetDevelopmentToken()
        {
            var tenantId = _config["AzureAD:TenantId"];
            var clientId = _config["AzureAD:ClientId"];
            var audience = _config["AzureAD:Audience"] ?? $"api://{clientId}";

            // Fetch Client Secret seamlessly from Key Vault at runtime!
            var clientSecret = _config["AzureAD:ClientSecret"];

            if (string.IsNullOrEmpty(clientSecret))
            {
                return BadRequest("Client Secret could not be resolved from Key Vault configuration.");
            }

            var requestUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId!),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("scope", $"{audience}/.default"),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(requestUrl, formContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { Error = "OAuth verification failed.", Details = responseContent });
            }

            using var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;

            return Ok(new
            {
                AccessToken = root.GetProperty("access_token").GetString(),
                TokenType = root.GetProperty("token_type").GetString(),
                ExpiresInSeconds = root.GetProperty("expires_in").GetInt32()
            });
        }
    }
}
