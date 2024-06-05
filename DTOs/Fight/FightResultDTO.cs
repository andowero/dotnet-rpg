using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.DTOs.Fight
{
    public class FightResultDTO
    {
        public List<AttackResultDTO> Attacks { get; set; } = new List<AttackResultDTO>();
    }
}