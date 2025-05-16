using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenSenseMapApiService.Models;
using OpenSenseMapApiService.Services;


namespace OpenSenseMapProxyApi.Services
{
    public class OpenSenseMapService : IOpenSenseMapService
    {
        private readonly HttpClient _httpClient;

        public OpenSenseMapService(HttpClient httpClient, IOptions<OpenSenseMapOptions> options)
        {
            _httpClient = httpClient;
             var baseUrl = options.Value.BaseUrl;
            _httpClient.BaseAddress = new Uri(baseUrl);

        }

        public async Task<string> RegisterUserAsync(OpenSenseMapUser user)
        {
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("users/register", content);

            var responseContent = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode
                ? responseContent
                : $"Error: {response.StatusCode} - {responseContent}";
        }
        public async Task<LoginResponseDto> LoginUserAsync(LoginRequestDto loginRequest)
        {

            var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json"); 

            var response = await _httpClient.PostAsync("users/sign-in", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Login failed: {response.StatusCode}, {error}");
            }
            var responseStream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(responseStream);
            var root = document.RootElement;

            var token = root.GetProperty("token").GetString();
            var user = root.GetProperty("user").ToString(); 

            return new LoginResponseDto
            {
                Token = token,
                User = user
            };
        }

        public async Task<NewSenseBoxResponseDto> CreateSenseBoxAsync(NewSenseBoxRequestDto boxRequest, string jwtToken)
        {
            var content = new StringContent(JsonConvert.SerializeObject(boxRequest), Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.PostAsync("/boxes", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"SenseBox creation failed: {response.StatusCode}, {error}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<NewSenseBoxResponseDto>(responseString);
            return result;
        }

        public async Task<SenseBoxResponseDto> GetSenseBoxByIdAsync(string boxId)
        {
            var url = $"/boxes/{boxId}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to retrieve SenseBox: {response.StatusCode}, {error}");
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<SenseBoxResponseDto>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }
        public async Task<bool> LogoutAsync(string jwtToken)
        {
            var url = $"/users/sign-out";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Logout failed: {response.StatusCode}, {error}");
        }


    }
}