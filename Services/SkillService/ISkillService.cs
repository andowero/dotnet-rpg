using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.DTOs.Skill;

namespace dotnet_rpg.Services.SkillService
{
    public interface ISkillService
    {
        public Task<ServiceResponse<GetCharacterDTO>> AddCharacterSkill(AddCharacterSkillDTO newSkill);
        public Task<ServiceResponse<List<GetSkillDTO>>> GetAllSkills();
    }
}