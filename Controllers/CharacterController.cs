using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterController: ControllerBase
    {
        private readonly ICharacterService _characterService;

        public CharacterController(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        [HttpGet]
        [Route("All")]
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDTO>>>> Get()
        {
            return Ok(await _characterService.GetAllCharacters());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<GetCharacterDTO>>> GetSingle(int id)
        {
             return Ok(await _characterService.GetCharacterById(id));
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDTO>>>> AddCharacter(AddCharacterDTO newCharacter)
        {
             return Ok(await _characterService.AddCharacter(newCharacter));
        }

        [HttpPut]
        public async Task<ActionResult<ServiceResponse<GetCharacterDTO>>> UpdateCharacter(UpdateCharacterDTO updatedCharacter)
        {
             var updateResponse = await _characterService.UpdateCharacter(updatedCharacter);

             if (updateResponse.Data is null)
             {
                 return NotFound(updateResponse);
             }
             else
             {
                 return Ok(updateResponse);
             }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDTO>>>> DeleteCharacter(int id)
        {
             var deleteResponse = await _characterService.DeleteCharacter(id);

             if (deleteResponse.Data is null)
             {
                 return NotFound(deleteResponse); 
             }
             else
             {
                 return Ok(deleteResponse);
             }
        }

        [AllowAnonymous]
        [HttpPost("ReviveAll")]
        public async Task<ActionResult<ServiceResponse<string>>> ReviveAllCharacters()
        {
            return Ok(await _characterService.ReviveAllCharacters());
        }
    }
}