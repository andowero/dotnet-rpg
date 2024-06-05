using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Data;

namespace dotnet_rpg.DTOs.Skill
{
    public class AddCharacterSkillDTO
    {
        public int SkillId { get; set; }
        public int CharacterId { get; set; }
    }
}