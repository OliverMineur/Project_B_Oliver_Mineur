using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTO;
using DbModels;
using Models.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static DbRepos.AdminDbRepos;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace MyApp.Namespace
{
    public class FriendsByCountryModel : PageModel
    {

        [BindProperty]
        public int pagenr { get; set; } = 0;

        private readonly IHttpClientFactory _httpClientFactory;

        public FriendsByCountryModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public ResponsePageDto<IFriend> ?FriendsData { get; set; }
        public IEnumerable<IGrouping<string, IFriend>> FriendsByCountry { get; set; }
        public string ErrorMessage { get; set; }

        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
        Converters = {
            
                new AbstractConverter<csFriend, IFriend>(),
                new AbstractConverter<Address, IAddress>(),
                new AbstractConverter<Pet, IPet>(),
                new AbstractConverter<Quote, IQuote>()
            }
        };

        public async Task<IActionResult> OnPostCallPreviousPage()
        {
            pagenr--;
            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCallNextPage()
        {
            pagenr++;
            await OnGetAsync();
            return Page();
        }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiUrl");
                string url = "api/Friends/GetAllFriends?seeded=true&flat=false&pageNr=0&pageSize=1000";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    FriendsData = JsonConvert.DeserializeObject<ResponsePageDto<IFriend>>(content, _jsonSettings);

                    FriendsByCountry = FriendsData.PageItems
                        .Where(f => f.Address != null && !string.IsNullOrEmpty(f.Address.Country))
                        .GroupBy(f => f.Address.Country)
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
    }
}
