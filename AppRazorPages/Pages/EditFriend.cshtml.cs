using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using Models;
using Models.DTO;
using Models.Interfaces;
using Newtonsoft.Json;
using static DbRepos.AdminDbRepos;
namespace MyApp.Namespace
{
    public class EditFriendModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public List<string> errors = new List<string>();

        public EditFriendModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [BindProperty]
        public string FriendGuid { get; set; } = string.Empty;
        public bool ShowSuccessMessage { get; set; } = false;
        [BindProperty]
        public FriendCuDto ?FriendToEdit { get; set; }

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


        public async Task<IActionResult> OnPostCallEditFriend()
        {
            try
            {
                var friendGuid = Request.Form["FriendGuid"];
                var firstName = Request.Form["FirstName"];
                var lastName = Request.Form["LastName"];
                var date = Request.Form["Birthday"];
                DateTime? dateOfBirth = string.IsNullOrWhiteSpace(date)
                    ? (DateTime?)null
                    : DateTime.Parse(date);

                var email = Request.Form["Email"];

                var nameRegex = new Regex(@"^[A-Za-zÀ-ÖØ-öø-ÿ' -]{2,50}$");

                if (string.IsNullOrWhiteSpace(firstName) || !nameRegex.IsMatch(firstName))
                {
                    errors.Add("First name contains invalid characters.");
                }
                if (string.IsNullOrWhiteSpace(lastName) || !nameRegex.IsMatch(lastName))
                {
                    errors.Add("Last name contains invalid characters.");
                }
                if(DateTime.TryParse(date, out DateTime dob))
                {
                    if(dob > DateTime.Now)
                    {
                        errors.Add("Date of birth cannot be in the future.");
                    }
                }
                else
                {
                    errors.Add("Invalid date format for date of birth.");
                }
                if (errors.Any())
                {
                    return BadRequest(string.Join(" ", errors));
                }
                var editedFriend = new FriendCuDto
                {
                    FriendId = Guid.Parse(friendGuid),
                    FirstName = firstName,
                    LastName = lastName,
                    Birthday = dateOfBirth,
                    Email = email
                };

                var client = _httpClientFactory.CreateClient("ApiUrl");
                string url = $"api/Friends/UpdateFriend/{editedFriend.FriendId}";
                var jsonContent = JsonConvert.SerializeObject(editedFriend, _jsonSettings);
                var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PutAsync(url, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    ShowSuccessMessage = true;
                    await OnPostCallGetFriend();
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
        public async Task OnPostCallGetFriend()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiUrl");
                string url = $"api/Friends/GetFriend?id={Uri.EscapeDataString(FriendGuid)}&flat=true";
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
