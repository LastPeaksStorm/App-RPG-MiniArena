using dotnet_rpg.Dtos.CharacterSkill;
using dotnet_rpg.Dtos.Skill;

namespace dotnet_rpg.Services.Skill
{
    public interface ISkillService
    {
        public Task<ServiceResponse<List<GetSkillDto>>> AddNewSkill(AddSkillDto newSkill);
        public Task<ServiceResponse<GetCharacterDto>> AddSkillToCharacter(AddCharacterSkillDto newSkill);
    }
}
