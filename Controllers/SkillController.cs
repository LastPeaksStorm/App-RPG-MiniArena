using dotnet_rpg.Dtos.CharacterSkill;
using dotnet_rpg.Dtos.Skill;
using dotnet_rpg.Services.Skill;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SkillController : ControllerBase
    {
        private readonly ISkillService _skillService;

        public SkillController(ISkillService skillService)
        {
            _skillService = skillService;
        }

        [HttpPost("AddNew")]
        public async Task<ActionResult<ServiceResponse<List<GetSkillDto>>>> AddNewSkill(AddSkillDto newSkill)
        {
            var response = await _skillService.AddNewSkill(newSkill);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpPost("AddSkillToCharacter")]
        public async Task<ActionResult<ServiceResponse<GetCharacterDto>>> AddSkillToCharacter(AddCharacterSkillDto newCharacterSkill)
        {
            var response = await _skillService.AddSkillToCharacter(newCharacterSkill);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
