using AutoMapper;
using dotnet_rpg.Models;
using System.Security.Claims;

namespace dotnet_rpg.Services.Character
{
    public class CharacterService : ICharacterService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CharacterService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User.
            FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string GetUserRole() => _httpContextAccessor.HttpContext!.User.
            FindFirstValue(ClaimTypes.Role)!;

        public async Task<ServiceResponse<List<GetCharacterDto>>> AddNewCharacter(AddCharacterDto character)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            try
            {
                var newCharacter = _mapper.Map<Models.Character>(character);
                newCharacter.User = await _context.Users.FirstOrDefaultAsync(u => u.Id.Equals(GetUserId()));

                _context.Characters.Add(newCharacter);
                await _context.SaveChangesAsync();

                serviceResponse.Data = await _context.Characters.Where(c => c.User!.Id.Equals(GetUserId())).Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            try
            {
                var dbCharacters = GetUserRole() == "Admin"
                        ? await _context.Characters.ToListAsync()
                        : await _context.Characters.Where(c => c.User!.Id == GetUserId()).Include(c => c.User).ToListAsync();

                serviceResponse.Data = dbCharacters.Where(c => c.User!.Id.Equals(GetUserId())).Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
                serviceResponse.Message = "Characters successfully obatined.";
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            try
            {
                var dbCharacter = await _context.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.User!.Id.Equals(GetUserId()) && c.Id.Equals(id));
                if (dbCharacter is null)
                    throw new Exception("Character not found or current user has no permission to utilize him.");

                serviceResponse.Data = _mapper.Map<GetCharacterDto>(dbCharacter);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();

            try
            {
                var character = await _context.Characters.FirstOrDefaultAsync(c => c.User!.Id.Equals(GetUserId()) && c.Id.Equals(updatedCharacter.Id));
                if (character is null)
                    throw new Exception("Character not found or current user has no permision to utilize him.");

                character.Name = updatedCharacter.Name;
                character.HitPoints = updatedCharacter.HitPoints;
                character.ManaPoints = updatedCharacter.ManaPoints;
                character.Defense = updatedCharacter.Defense;
                character.Strength = updatedCharacter.Strength;
                character.Agility = updatedCharacter.Agility;
                character.Intelligence = updatedCharacter.Intelligence;
                character.Classification = updatedCharacter.Classification;
                character.BaseAttribute = updatedCharacter.BaseAttribute;

                await _context.SaveChangesAsync();

                serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            try
            {
                var character = await _context.Characters.FirstOrDefaultAsync(c => c.User!.Id.Equals(GetUserId()) && c.Id.Equals(id));
                if (character is null)
                    throw new Exception("Character not found or current user has no permision to utilize him.");

                _context.Remove(character);

                await _context.SaveChangesAsync();

                serviceResponse.Data = await _context.Characters.Where(c => c.User!.Id.Equals(GetUserId())).Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

    }

}
