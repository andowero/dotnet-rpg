using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Data;
using dotnet_rpg.DTOs.Fight;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace dotnet_rpg.Services.FightService
{
    public class FightService_DB : IFightService
    {
        private interface IFightAttributer
        {
            public int AttackDamage();
            public int AttackAttribute();
            public string AttackName();
        }
        private class WeaponAttackAttributer : IFightAttributer
        {
            private Character? _attacker;
            public WeaponAttackAttributer (Character? attacker)
            {
                _attacker = attacker;
            }
            public int AttackAttribute()
            {
                return _attacker!.Strength;
            }
            public int AttackDamage()
            {
                return _attacker!.Weapon is null ? 0 : _attacker.Weapon.Damage;
            }

            public string AttackName()
            {
                if (_attacker!.Weapon is null)
                {
                    return "bare hands";
                }
                else
                {
                    return _attacker.Weapon.Name;
                }
            }
        }
        private class SkillAttackAttributer : IFightAttributer
        {
            private Character? _attacker;
            private readonly int _skillId;
            public SkillAttackAttributer (Character? attacker, int skillId)
            {
                _skillId = skillId;
                _attacker = attacker;            }
            public int AttackAttribute()
            {
                return _attacker!.Intelligence;
            }
            public int AttackDamage()
            {
                var skill = _attacker!.Skills!.FirstOrDefault(s => s.Id == _skillId);
                if (skill is null)
                {
                    throw new Exception($"Character {_attacker.Name} doesn't have skill (id={_skillId})");
                }
                return skill.Damage;
            }

            public string AttackName()
            {
                return _attacker!.Skills!.FirstOrDefault(s => s.Id == _skillId)!.Name;
            }
        }

        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        private readonly Random _random = new();
        public FightService_DB(DataContext dataContext, IMapper mapper)
        {
            this._mapper = mapper;
            this._dataContext = dataContext;
        }
        private ServiceResponse<AttackResultDTO> Attack(Character? attacker, Character? defender, IFightAttributer fightAttributes)
        {
            var response = new ServiceResponse<AttackResultDTO>();

            try
            {
                if (attacker is null || defender is null)
                {
                    response.Success = false;
                    response.Message = "one of characters doesn't exist";
                    return response;
                }

                if (attacker.HitPoints <= 0 || defender.HitPoints <= 0)
                {
                    response.Success = false;
                    response.Message = $"Character {(attacker.HitPoints <= 0 ? attacker : defender).Name} is dead.";
                    return response;
                }

                int weaponDamage = attacker.Weapon is null ? 0 : attacker.Weapon.Damage;
                int damage = 
                    fightAttributes.AttackDamage()
                    + _random.Next(fightAttributes.AttackAttribute())
                    - _random.Next(defender.Defence);

                if (damage < 0) {damage = 0;}

                defender.HitPoints -= damage;
                if (defender.HitPoints <= 0)
                {
                    response.Data = new AttackResultDTO() 
                        {
                            AttackResult = $"{attacker.Name} killed {defender.Name} with {fightAttributes.AttackName()}. Hooray!"
                        };
                }
                else
                {
                    response.Data = new AttackResultDTO() 
                        {
                            AttackResult = $"{attacker.Name} ({attacker.HitPoints} hp) attacked {defender.Name} ({defender.HitPoints} hp) with {fightAttributes.AttackName()} for {(damage>0?damage:0)} damage."
                        };
                }

                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public ServiceResponse<AttackResultDTO> AttackWithWeapon(Character? attacker, Character? defender)
        {
            var weaponAttributer = new WeaponAttackAttributer(attacker);
            return Attack(attacker, defender, weaponAttributer);
        }

        public ServiceResponse<AttackResultDTO> AttackWithSkill(Character? attacker, int skillId, Character? defender)
        {
            var skillAttributer = new SkillAttackAttributer(attacker, skillId);
            return Attack(attacker, defender, skillAttributer);
        }

        private async Task<Character?> GetDefender(int defenderId)
        {
            return await _dataContext.Characters.FirstOrDefaultAsync(c => c.Id == defenderId);
        }

        public async Task<ServiceResponse<AttackResultDTO>> AttackWithWeapon(WeaponAttackDTO weaponAttack)
        {
            var attacker = await _dataContext.Characters
                    .Include(c => c.Weapon)
                    .FirstOrDefaultAsync(c => c.Id == weaponAttack.AttackerId);
            var defender = await GetDefender(weaponAttack.DefenderId);
            var response = AttackWithWeapon(attacker, defender);
            await _dataContext.SaveChangesAsync();
            return response;
        }

        public async Task<ServiceResponse<AttackResultDTO>> AttackWithSkill(SkillAttackDTO skillAttack)
        {
            var attacker = _dataContext.Characters
                    .Include(c => c.Skills)
                    .FirstOrDefault(c => c.Id == skillAttack.AttackerId);
            var defender = await GetDefender(skillAttack.DefenderId);
            var response = AttackWithSkill(attacker, skillAttack.SkillId, defender);
            await _dataContext.SaveChangesAsync();
            return response;
        }

        public async Task<ServiceResponse<FightResultDTO>> Deathmatch(DeathMatchRequestDTO deathmatchRequest)
        {
            var response = new ServiceResponse<FightResultDTO>();
            try
            {
                var fightResult = new FightResultDTO();
                var fighters = await _dataContext.Characters
                    .Include(c => c.Skills)
                    .Include(c => c.Weapon)
                    .Where(c => deathmatchRequest.CharacterIds.Contains(c.Id) && c.HitPoints > 0)
                    .ToListAsync();

                if (fighters.IsNullOrEmpty())
                {
                    response.Message = "No characters selected...";
                    response.Success = true;
                    return response;
                }
                else if (fighters.Count < 2)
                {
                    response.Message = "Too few characters selected...";
                    response.Success = true;
                    return response;
                }

                var attacker = fighters[0];
                while (fighters.Count > 1)
                {
                    var randomAttackerIndex = _random.Next(fighters.Count);
                    attacker = fighters[randomAttackerIndex];
                    var randomDefenderIndex = randomAttackerIndex;
                    while (randomAttackerIndex == randomDefenderIndex)
                    {
                        randomDefenderIndex = _random.Next(fighters.Count);
                    }
                    var defender = fighters[randomDefenderIndex];
                    
                    var attackWithWeapon = false;

                    if (_random.Next(2) == 0)
                    {
                        //attack with weapon
                        attackWithWeapon = true;
                    }

                    if (attackWithWeapon && attacker.Weapon is null && attacker.Skills is not null && attacker.Skills.Count > 0)
                    {
                        attackWithWeapon = false;
                    }

                    if (!attackWithWeapon && (attacker.Skills is null || attacker.Skills.Count <= 0))
                    {
                        attackWithWeapon = true;
                    }

                    ServiceResponse<AttackResultDTO> attackResponse;

                    if (attackWithWeapon)
                    {
                        attackResponse = AttackWithWeapon(attacker, defender);
                    }
                    else
                    {
                        var skillIndex = _random.Next(attacker.Skills!.Count);
                        var skillId = attacker.Skills![skillIndex].Id;
                        attackResponse = AttackWithSkill(attacker, skillId, defender);
                    }

                    if (!attackResponse.Success)
                    {
                        response.Success = false;
                        response.Message = attackResponse.Message;
                        return response;
                    }

                    fightResult.Attacks.Add(attackResponse.Data!);

                    if (defender.HitPoints <= 0)
                    {
                        defender.Defeats ++;
                        defender.Fights ++;
                        fighters.Remove(defender);
                    }
                }
                attacker.Victories ++;
                attacker.Fights ++;

                await _dataContext.SaveChangesAsync();

                response.Data = fightResult;
                response.Success = true;
                response.Message = $"{attacker.Name} won the DEATHMATCH";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<List<GetHiscoreDTO>>> GetHiscore()
        {
            var characters = await _dataContext.Characters
                                    .Where(c => c.Fights > 0)
                                    .OrderByDescending(c => c.Victories)
                                    .ThenBy(c => c.Defeats)
                                    .ThenByDescending(c => c.Fights)
                                    .ToListAsync();

            return new ServiceResponse<List<GetHiscoreDTO>>() {Data = _mapper.Map<List<GetHiscoreDTO>>(characters)};
        }
    }
}