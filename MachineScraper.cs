using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LaundryBot
{
    public class MachineScraper
    {
        private const string ApiBaseUrl = "https://ls.bilantek.com/api/terminalstate/";
        private readonly HttpClient _httpClient = new HttpClient();
        
        public MachineScraper()
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(30); 
            
            _httpClient.DefaultRequestHeaders.Add("TenantCode", "lcapp_androsiuk");
        }

        public async Task<TerminalState> GetTerminalStateAsync(string terminalNumber)
        {
            var requestUrl = $"{ApiBaseUrl}{terminalNumber}";
        
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
            
                response.EnsureSuccessStatusCode();
            
                var jsonResponse = await response.Content.ReadAsStringAsync();

                var terminalState = JsonSerializer.Deserialize<TerminalState>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            
                if (terminalState == null)
                {
                    throw new Exception("Не вдалося десеріалізувати дані API.");
                }

                return terminalState;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Помилка запиту до API для терміналу {terminalNumber}. Перевірте підключення або номер терміналу. Деталі: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Невідома помилка при отриманні статусу: {ex.Message}");
            }
        }
    }
}