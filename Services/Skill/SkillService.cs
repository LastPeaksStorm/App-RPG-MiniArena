using AutoMapper;
using dotnet_rpg.Dtos.CharacterSkill;
using dotnet_rpg.Dtos.Skill;
using System.Security.Claims;
using dotnet_rpg.Models;

namespace dotnet_rpg.Services.Skill
{
    public class SkillService : ISkillService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SkillService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<List<GetSkillDto>>> AddNewSkill(AddSkillDto newSkill)
        {
            var response = new ServiceResponse<List<GetSkillDto>>();
            try
            {
                var skillAlreadyExists = await _context.Skills.AnyAsync(s =>
                                s.Name.Equals(newSkill.Name)
                                && s.Damage.Equals(newSkill.Damage));

                if (skillAlreadyExists)
                    throw new Exception("Skill alreay exists.");

                var skill = _mapper.Map<Models.Skill>(newSkill);

                _context.Skills.Add(skill);
                await _context.SaveChangesAsync();

                response.Data = await _context.Skills.Select(s => _mapper.Map<GetSkillDto>(s)).ToListAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<GetCharacterDto>> AddSkillToCharacter(AddCharacterSkillDto newCharacterSkill)
        {
            var response = new ServiceResponse<GetCharacterDto>();
            try
            {
                var character = await _context.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.Id == newCharacterSkill.CharacterId &&
                        c.User!.Id == GetUserId());

                if (character is null)
                {
                    response.Success = false;
                    response.Message = "Character not found or current user has no permission to utilize him.";
                    return response;
                }

                var skill = await _context.Skills
                    .FirstOrDefaultAsync(s => s.Id == newCharacterSkill.SkillId);

                if (skill is null)
                {
                    response.Success = false;
                    response.Message = "Skill not found.";
                    return response;
                }

                var characterAlreadyHasTheSkill = character.Skills!.Any(s => s.Id.Equals(newCharacterSkill.SkillId));

                if (characterAlreadyHasTheSkill)
                {
                    response.Success = false;
                    response.Message = "Character already has this skill.";
                    return response;
                }

                character.Skills!.Add(skill);
                await _context.SaveChangesAsync();
                response.Data = _mapper.Map<GetCharacterDto>(character);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        private int GetUserId()
        {
            return int.Parse(_httpContextAccessor.HttpContext!.User
            .FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
