using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WebApplication3.DTOs;

namespace WebApplication3.SyncDataServices.http
{
    public class HttpCommandDataClients : ICommandDataClients
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public HttpCommandDataClients(HttpClient client, IConfiguration configuration)
        {
            _httpClient = client;
            _configuration = configuration;
        }

        public async Task SendPlatformToCommand(PlatformReadDto platform)
        {
            var httpContent = new StringContent(
                JsonSerializer.Serialize(platform),
                Encoding.UTF8,
                "application/json"
                );

            var response = await _httpClient.PostAsync($"{_configuration["CommandServiceUrl"]}api/c/Platforms/", httpContent);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("--> Sync post to command service was OK");
            }
            else
            {
                Console.WriteLine("--> Sync post to command service was NOT OK");
            }

        }
    }
}