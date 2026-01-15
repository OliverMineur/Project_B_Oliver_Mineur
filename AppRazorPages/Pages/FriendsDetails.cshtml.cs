using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTO;
using Models.Interfaces;
using Newtonsoft.Json;
using static DbRepos.AdminDbRepos;

namespace MyApp.Namespace
{
    public class FriendsDetailsModel : PageModel
    {

        private readonly IHttpClientFactory _httpClientFactory;

        public FriendsDetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [BindProperty]
        public string FriendGuid { get; set; } = string.Empty;

        public ResponseItemDto<IFriend> ?FriendsData { get; set; }
        public IFriend ?FriendDetails { get; set; }
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
        public async Task OnPostCallFriendDetails()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiUrl");
                string url = $"api/Friends/GetFriend?id={Uri.EscapeDataString(FriendGuid)}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    FriendsData = JsonConvert.DeserializeObject<ResponseItemDto<IFriend>>(content, _jsonSettings);

                    FriendDetails = FriendsData.Item;
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
        public void OnGet()
        {
            
        }
    }
}
