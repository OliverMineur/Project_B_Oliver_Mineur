using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTO;
using Models.Interfaces;
using Newtonsoft.Json;
using static DbRepos.AdminDbRepos;

namespace MyApp.Namespace
{
    public class EditFriendAddressModel : PageModel
    {

        public List<string> errors = new List<string>();
        private readonly IHttpClientFactory _httpClientFactory;
        public EditFriendAddressModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [BindProperty]
        public Guid AddressGuid { get; set; } = Guid.Empty;
        [BindProperty]
        public Guid FriendGuid { get; set; } = Guid.Empty; 

        public ResponseItemDto<IFriend> ?FriendsData { get; set; }
        public List<Guid> FriendsOnAdress { get; set; } = new List<Guid>();
        [BindProperty]
        public csFriend FriendDetails { get; set; }
        public bool ShowSuccessMessage { get; set; } = false;
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

        public async Task<IActionResult> OnPostCallEditFriendAddress()
        {
            try
            {
                var streetaddress = Request.Form["streetaddress"];
                var zipcode = Request.Form["zipcode"];
                var city = Request.Form["city"];
                var country = Request.Form["country"];

                var nameRegex = new Regex(@"^[A-Za-zÀ-ÖØ-öø-ÿ' -]{2,50}$");
                var streetRegex = new Regex(@"^[A-Za-zÀ-ÖØ-öø-ÿ' -]+(?: [A-Za-zÀ-ÖØ-öø-ÿ' -]+)* \d+[A-Za-z]?$");

                if (string.IsNullOrWhiteSpace(streetaddress) || !streetRegex.IsMatch(streetaddress))
                {
                    errors.Add("Street address contains invalid characters.");
                }
                if (string.IsNullOrWhiteSpace(city) || !nameRegex.IsMatch(city))
                {
                    errors.Add("City contains invalid characters.");
                }
                if (string.IsNullOrWhiteSpace(country) || !nameRegex.IsMatch(country))
                {
                    errors.Add("Country contains invalid characters.");
                }
                if (errors.Any())
                {
                    return BadRequest(string.Join(" ", errors));
                }

        
                var selected = TempData["FriendsOnAdress"]?.ToString();
                var selectedList = selected == null
                    ? new List<Guid>()
                    : JsonConvert.DeserializeObject<List<Guid>>(selected);

                var editedFriendaddress = new AddressCuDto
                {
                    AddressId = AddressGuid,
                    FriendsId = selectedList,
                    StreetAddress = streetaddress,
                    ZipCode = int.Parse(zipcode),
                    City = city,
                    Country = country
                };

                var client = _httpClientFactory.CreateClient("ApiUrl");
                string url = $"api/Addresses/UpdateAddress/{editedFriendaddress.AddressId}";
                var jsonContent = JsonConvert.SerializeObject(editedFriendaddress, _jsonSettings);
                var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PutAsync(url, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    ShowSuccessMessage = true;
                    return Page();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Status: {response.StatusCode} - {errorContent}";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Exception: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCallGetFriend()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiUrl");
                string url = $"api/Friends/GetFriend?id={Uri.EscapeDataString(FriendGuid.ToString())}&flat=false";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    FriendsData = JsonConvert.DeserializeObject<ResponseItemDto<IFriend>>(content, _jsonSettings);

                    if (FriendsData?.Item?.Address == null)
                    {
                        ErrorMessage = "Friend has no address.";
                        return Page();
                    }

                    AddressGuid = FriendsData.Item.Address.AddressId;
                    FriendsOnAdress = FriendsData.Item.Address.Friends.Select(f => f.FriendId).ToList();
                    TempData["FriendsOnAdress"] = JsonConvert.SerializeObject(FriendsOnAdress);
                    return Page();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Status: {response.StatusCode} - {errorContent}";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Exception: {ex.Message}";
                return Page();
            }
        }
        public async Task OnGetAsync()
        {
        }
    }
}
