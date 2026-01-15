using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTO;
using Models.Interfaces;
using Newtonsoft.Json;
using static DbRepos.AdminDbRepos;

namespace MyApp.Namespace
{
    public class DeleteFriendInfoModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteFriendInfoModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [BindProperty]
        public string FriendGuid { get; set; } = string.Empty;
        [BindProperty]
        public Guid PetId { get; set; } 
        [BindProperty]
        public Guid AddressId { get; set; }
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

        public async Task OnPostCallDeletePet()
        {
            
            var client = _httpClientFactory.CreateClient("ApiUrl");
            string url = $"api/Pets/DeletePet/{PetId}";
            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                await OnPostCallFriendDetails();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Status: {response.StatusCode} - {errorContent}";
            }
        }

        public async Task OnPostCallDeleteAddress()
        {
            
            var client = _httpClientFactory.CreateClient("ApiUrl");
            string url = $"api/Addresses/DeleteItem/{AddressId}";
            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                await OnPostCallFriendDetails();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Status: {response.StatusCode} - {errorContent}";
            }
        }

        public async Task OnPostCallFriendDetails()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiUrl");
                string url = $"api/Friends/GetFriend?id={Uri.EscapeDataString(FriendGuid)}&flat=false";
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
