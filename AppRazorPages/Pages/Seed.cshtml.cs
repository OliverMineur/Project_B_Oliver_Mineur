using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualBasic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MyApp.Namespace
{
    public class SeedModel : PageModel
    {

        private readonly IHttpClientFactory _httpClientFactory;

        public SeedModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public string SeedResult { get; set; } = string.Empty;

        [BindProperty]
        public int Count { get; set; } = 100;

        public string RemoveSeedResult { get; set; } = string.Empty;

        public async Task OnPostCallSeedAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiUrl");

                string url = $"api/Admin/Seed?count={Count}";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    SeedResult = "Seeding successful!";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    SeedResult = $"Error: {response.StatusCode} - {errorContent}";
                }

                SeedResult = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                SeedResult = $"Exception: {ex.Message}";
            }


        }

        
        public async Task OnPostCallRemoveSeedAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiUrl");

                string url = $"api/Admin/RemoveSeed?seeded=true";
                var response = await client.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    RemoveSeedResult = "Seed removal successful!";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    RemoveSeedResult = $"Error: {response.StatusCode} - {errorContent}";
                }
            }
            catch (Exception ex)
            {
                RemoveSeedResult = $"Exception: {ex.Message}";
            }
        }
            

        public void OnGet()
        {
        }
    }
}
