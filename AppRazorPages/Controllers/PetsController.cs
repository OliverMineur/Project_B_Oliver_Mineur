using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Models.Interfaces;
using Models.DTO;
using Services.Interfaces;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PetsController : Controller
    {
        readonly IPetsService _service = null;
        readonly ILogger<PetsController> _logger = null;

        [HttpDelete("{id}")]
        [ActionName("DeletePet")]
        [ProducesResponseType(200, Type = typeof(IPet))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> DeleteItem(string id)
        {
            try
            {
                var idArg = Guid.Parse(id);

                _logger.LogInformation($"{nameof(DeleteItem)}: {nameof(idArg)}: {idArg}");
                
                var item = await _service.DeletePetAsync(idArg);
                if (item == null) throw new ArgumentException ($"Item with id {id} does not exist");
        
                _logger.LogInformation($"item {idArg} deleted");
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(DeleteItem)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        public PetsController(IPetsService service, ILogger<PetsController> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}

