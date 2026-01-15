using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTO;
using DbModels;
using Models.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static DbRepos.AdminDbRepos;

namespace MyApp.Namespace
{
    public class FriendsAndPetsFilteredModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FriendsAndPetsFilteredModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [BindProperty(SupportsGet = true)]
        public string SelectedCountry { get; set; }
        public List<string> CountryList { get; set; }

        public ResponsePageDto<IAddress> ?FriendsData { get; set; }
        public IEnumerable<IGrouping<string, IAddress>> FriendsByCountry { get; set; }
        public string ErrorMessage { get; set; }
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            Converters = 
            {
                new AbstractConverter<csFriend, IFriend>(),
                new AbstractConverter<Address, IAddress>(),
                new AbstractConverter<Pet, IPet>(),
                new AbstractConverter<Quote, IQuote>()
            }
        };

        public async Task OnPostCallFriendsAndPetsFilteredAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiUrl");
                string url = $"api/Addresses/GetAllAddresses?seeded=true&flat=false&filter=&pageNr=0&pageSize=1000";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    FriendsData = JsonConvert.DeserializeObject<ResponsePageDto<IAddress>>(content, _jsonSettings);

                    FriendsByCountry = FriendsData.PageItems
                        .Where(a => a.Country != null && a.Country != null && !string.IsNullOrEmpty(a.Country))
                        .GroupBy(a => a.Country)
                        .ToList();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Status: {response.StatusCode} - {errorContent}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Exception: {ex.Message}";
            }
        }

        public async Task OnGet()
        {
            await OnPostCallFriendsAndPetsFilteredAsync();
            CountryList = FriendsByCountry.Select(g => g.Key).ToList();
        }
    }
}
