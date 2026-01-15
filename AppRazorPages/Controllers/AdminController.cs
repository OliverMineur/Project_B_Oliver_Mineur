using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

using Models.DTO;
using Services.Interfaces;
using Configuration;
using Configuration.Options;
using Microsoft.Extensions.Options;

namespace AppRazorPages.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    
    public class AdminController : Controller
    {
        readonly DatabaseConnections _dbConnections;
        readonly IAdminService _service;
        readonly ILogger<AdminController> _logger;
        readonly VersionOptions _versionOptions;
        readonly EnvironmentOptions _environmentOptions;

        [HttpGet()]
        [ActionName("Seed")]
        [ProducesResponseType(200, Type = typeof(GstUsrInfoAllDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> Seed(int count)
        {
            try
            {
                if(count == 0)
                {
                    count = 100;
                }
                _logger.LogInformation($"{nameof(Seed)}: {nameof(count)}: {count}");
                var info = await _service.SeedAsync(count);
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(Seed)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet()]
        [ActionName("RemoveSeed")]
        [ProducesResponseType(200, Type = typeof(GstUsrInfoAllDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> RemoveSeed(string seeded = "true")
        {
            try
            {
                bool seededArg = bool.Parse(seeded);

                _logger.LogInformation($"{nameof(RemoveSeed)}: {nameof(seededArg)}: {seededArg}");
                var info = await _service.RemoveSeedAsync(seededArg);
                
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(RemoveSeed)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        public AdminController(IAdminService service, ILogger<AdminController> logger,
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