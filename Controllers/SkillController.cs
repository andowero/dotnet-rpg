using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SkillController : ControllerBase
    {
        private readonly ISkillService _skillService;
        public SkillController(ISkillService skillService)
        {
            _skillService = skillService;
        }

        [AllowAnonymous]
        [HttpGet("All")]
        public async Task<ActionResult<ServiceResponse<List<GetSkillDTO>>>> GetAllSkills()
        {
            return Ok(await _skillService.GetAllSkills());
        }

        [HttpPost("AddCharacterSkill")]
        public async Task<ActionResult<ServiceResponse<GetCharacterDTO>>> AddCharacterSkill(AddCharacterSkillDTO addSkill)
        {
            return Ok (await _skillService.AddCharacterSkill(addSkill));
        }
    }
}