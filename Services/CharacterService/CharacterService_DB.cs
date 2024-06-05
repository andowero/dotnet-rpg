using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using dotnet_rpg.Data;

namespace dotnet_rpg.Services.CharacterService
{

    public class CharacterService_DB : ICharacterService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _dataContext;
        public IHttpContextAccessor _httpContextAccessor { get; }

        public CharacterService_DB(IMapper mapper, DataContext dataContext, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _dataContext = dataContext;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<ServiceResponse<List<GetCharacterDTO>>> AddCharacter(AddCharacterDTO newCharacter)
        {
            var character = _mapper.Map<Character>(newCharacter);
            character.User = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());
            await _dataContext.AddAsync(character);
            await _dataContext.SaveChangesAsync();
            return await GetAllCharacters();
        }

        public async Task<ServiceResponse<List<GetCharacterDTO>>> DeleteCharacter(int id)
        {
            ServiceResponse<List<GetCharacterDTO>> serviceResponse;

            var userCharacter = await _dataContext.Characters.FirstOrDefaultAsync(c => c.Id == id && c.User!.Id == GetUserId());
            if (userCharacter == null)
            {
                serviceResponse = new ServiceResponse<List<GetCharacterDTO>>();
                serviceResponse.Success = false;
                serviceResponse.Message = $"Character id={id} not found...";
            }
            else
            {
                _dataContext.Remove(userCharacter);
                await _dataContext.SaveChangesAsync();
                serviceResponse = await GetAllCharacters();
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDTO>>> GetAllCharacters()
        {
            var dbCharacters = await _dataContext.Characters
                .Include(c=>c.Weapon)
                .Include(c=>c.Skills)
                .Where(c => c.User!.Id == GetUserId()).ToListAsync();
            return new ServiceResponse<List<GetCharacterDTO>> {Data = _mapper.Map<List<GetCharacterDTO>>(dbCharacters)};
        }

        public async Task<ServiceResponse<GetCharacterDTO>> GetCharacterById(int id)
        {
            var dbCharacter = await _dataContext.Characters.FirstOrDefaultAsync(c => c.Id == id && c.User!.Id == GetUserId());
            var serviceResponse = new ServiceResponse<GetCharacterDTO>() 
            {
                Data = _mapper.Map<GetCharacterDTO>(dbCharacter)
            };
            if (dbCharacter is null)
            {
                serviceResponse.Success = false;
                serviceResponse.Message="Character not found!!!";
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDTO>> UpdateCharacter(UpdateCharacterDTO updateCharacter)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDTO>();
            var dbCharacter =
                await _dataContext.Characters
                    .Include(c => c.User)
                    .SingleOrDefaultAsync(c => c.Id == updateCharacter.Id);

            if (dbCharacter is not null && dbCharacter.User!.Id == GetUserId())
            {
                dbCharacter.Name = updateCharacter.Name ?? dbCharacter.Name;
                dbCharacter.HitPoints = updateCharacter.HitPoints ?? dbCharacter.HitPoints;
                dbCharacter.Strength = updateCharacter.Strength ?? dbCharacter.Strength;
                dbCharacter.Defence = updateCharacter.Defence ?? dbCharacter.Defence;
                dbCharacter.Intelligence = updateCharacter.Intelligence ?? dbCharacter.Intelligence;
                dbCharacter.Class = updateCharacter.Class ?? dbCharacter.Class;

                await _dataContext.SaveChangesAsync();

                serviceResponse.Data = _mapper.Map<GetCharacterDTO>(dbCharacter);
            }
            else
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"Character (Id={updateCharacter.Id}) not found!!!";
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<string>> ReviveAllCharacters()
        {
            var characters = await _dataContext.Characters.Where(c => c.HitPoints != 100).ToListAsync();
            foreach (var character in characters)
            {
                character.HitPoints = 100;
            }
            await _dataContext.SaveChangesAsync();
            return new() {Success = true, Data = "All character revived to 100 hp."};
        }
    }
}