using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using dotnet_rpg.Data;
using dotnet_rpg.DTOs.Skill;

namespace dotnet_rpg.Services.SkillService
{
    public class SkillService_DB : ISkillService
    {
        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IMapper _mapper;
        public SkillService_DB(DataContext dataContext, IHttpContextAccessor httpContext, IMapper mapper)
        {
            _mapper = mapper;
            _httpContext = httpContext;
            _dataContext = dataContext;
            
        }
        public async Task<ServiceResponse<GetCharacterDTO>> AddCharacterSkill(AddCharacterSkillDTO newSkill)
        {
            var response = new ServiceResponse<GetCharacterDTO>();

            var skill = await _dataContext.Skills.FirstOrDefaultAsync(x => x.Id == newSkill.SkillId);
            if (skill == null)
            {
                response.Success = false;
                response.Message = $"No such skill (id={newSkill.SkillId}) exists...";
                return response;
            }
            var currentCharacterId = int.Parse(_httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var character = await _dataContext.Characters
                .Include(c => c.Weapon)
                .Include(c => c.Skills)
                .FirstOrDefaultAsync
                (
                    x =>
                        x.Id == newSkill.CharacterId
                        && x.User!.Id == currentCharacterId
                );
            if (character == null)
            {
                response.Success = false;
                response.Message = $"No such character (id={newSkill.CharacterId}) exists...";
                return response;
            }
            
            character.Skills!.Add(skill);
            await _dataContext.SaveChangesAsync();

            response.Success = true;
            response.Data = _mapper.Map<GetCharacterDTO>(character);
            return response;
        }

        public async Task<ServiceResponse<List<GetSkillDTO>>> GetAllSkills()
        {
            return new ServiceResponse<List<GetSkillDTO>> {Data=_mapper.Map<List<GetSkillDTO>>(await _dataContext.Skills.ToListAsync())};
        }
    }
}