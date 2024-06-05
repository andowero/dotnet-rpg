using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.DTOs.Fight;

namespace dotnet_rpg.Services.FightService
{
    public interface IFightService
    {
        public Task<ServiceResponse<AttackResultDTO>> AttackWithWeapon(WeaponAttackDTO weaponAttack);
        public Task<ServiceResponse<AttackResultDTO>> AttackWithSkill(SkillAttackDTO skillAttack);
        public Task<ServiceResponse<FightResultDTO>> Deathmatch(DeathMatchRequestDTO deathmatchRequest);
        public Task<ServiceResponse<List<GetHiscoreDTO>>> GetHiscore();
    }
}