using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Models.Interfaces;
using Models.DTO;
using Services.Interfaces;
using Configuration;
using Configuration.Options;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Text.RegularExpressions;

namespace AppRazorPages.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    
    public class FriendsController : Controller
    {
        readonly DatabaseConnections _dbConnections;
        readonly IFriendsService _service;
        readonly ILogger<FriendsController> _logger;
        readonly VersionOptions _versionOptions;
        readonly EnvironmentOptions _environmentOptions;

        [HttpGet()]
        [ActionName("GetAllFriends")]
        [ProducesResponseType(200, Type = typeof(ResponsePageDto<IFriend>))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> GetAllFriends(string seeded = "true", string flat = "true", string filter = null, string pageNr = "0", string pageSize = "10")
        {
            try
            {
                bool seededArg = bool.Parse(seeded);
                bool flatArg = bool.Parse(flat);
                int pageNrArg = int.Parse(pageNr);
                int pageSizeArg = int.Parse(pageSize);

                if(!string.IsNullOrEmpty(filter) && !Regex.IsMatch(filter, @"^[a-zA-Z0-9\s]*$"))
                {
                    throw new ArgumentException("Filter can only contain letters (a-z), numbers (0-9), and spaces.");
                }

                _logger.LogInformation($"{nameof(GetAllFriends)}: {nameof(seededArg)}: {seededArg}, {nameof(flatArg)}: {flatArg}, " +
                    $"{nameof(pageNrArg)}: {pageNrArg}, {nameof(pageSizeArg)}: {pageSizeArg}");

                var response = await _service.ReadFriendsAsync(seededArg, flatArg, filter?.Trim().ToLower(), pageNrArg, pageSizeArg);     
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(GetAllFriends)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet()]
        [ActionName("GetFriend")]
        [ProducesResponseType(200, Type = typeof(IFriend))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        public async Task<IActionResult> ReadItem(string id = null, string flat = "false")
        {
            try
            {
                var idArg = Guid.Parse(id);
                bool flatArg = bool.Parse(flat);

                _logger.LogInformation($"{nameof(ReadItem)}: {nameof(idArg)}: {idArg}, {nameof(flatArg)}: {flatArg}");
                
                var item = await _service.ReadFriendAsync(idArg, flatArg);
                if (item == null) throw new ArgumentException ($"Item with id {id} does not exist");

                return Ok(item);         
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ReadItem)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        //PUT: api/friends/updatefriend/id
        //Body: csFriendCUdto in Json
        [HttpPut("{id}")]
        [ActionName("UpdateFriend")]
        [ProducesResponseType(200, Type = typeof(IFriend))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> UpdateFriend(string id, [FromBody] FriendCuDto item)
        {
            try
            {
                item.EnsureValidity();

                var idArg = Guid.Parse(id);
                _logger.LogInformation($"{nameof(UpdateFriend)}: {nameof(idArg)}: {idArg}");
                if (item.FriendId != idArg) throw new ArgumentException("Id mismatch");

                var _item = await _service.UpdateFriendAsync(item);
                _logger.LogInformation($"item {idArg} updated");
               
                return Ok(_item);             
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(UpdateFriend)}: {ex.Message}");
                return BadRequest($"Could not update. Error {ex.Message}");
            }
        }

        public FriendsController(IFriendsService service, ILogger<FriendsController> logger,
                DatabaseConnections dbConnections, IOptions<VersionOptions> versionOptions,
                IOptions<EnvironmentOptions> environmentOptions)
        {
            _service = service;
            _logger = logger;
            _dbConnections = dbConnections;
            _versionOptions = versionOptions.Value;
            _environmentOptions = environmentOptions.Value;
        }
    }
}