using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.DTOs.Fight;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FightController : ControllerBase
    {
        private readonly IFightService _fightService;
        public FightController(IFightService fightService)
        {
            _fightService = fightService;
        }

        [HttpPost]
        [Route("Attack/Weapon")]
        public async Task<ActionResult<ServiceResponse<AttackResultDTO>>> AttackWithWeapon(WeaponAttackDTO weaponAttack)
        {
            return Ok(await _fightService.AttackWithWeapon(weaponAttack));
        }

        [HttpPost]
        [Route("Attack/Skill")]
        public async Task<ActionResult<ServiceResponse<AttackResultDTO>>> AttackWithSkill(SkillAttackDTO skillAttack)
        {
            return Ok(await _fightService.AttackWithSkill(skillAttack));
        }

        [HttpPost]
        [Route("DeathMatch")]
        public async Task<ActionResult<ServiceResponse<FightResultDTO>>> DoDeathMatch(DeathMatchRequestDTO deathmatchRequest)
        {
            return Ok(await _fightService.Deathmatch(deathmatchRequest));
        }

        [HttpGet("Hiscore")]
        public async Task<ActionResult<ServiceResponse<List<GetHiscoreDTO>>>> GetHiscore()
        {
            return Ok(await _fightService.GetHiscore());
        }
    }
}