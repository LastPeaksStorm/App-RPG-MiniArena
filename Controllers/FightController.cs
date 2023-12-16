using dotnet_rpg.Dtos.Fight;
using dotnet_rpg.Services.Fight;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class FightController : ControllerBase
    {
        private readonly IFightService _fightService;

        public FightController(IFightService fightService)
        {
            _fightService = fightService;
        }

        [HttpPost("WeaponAttack")]
        public async Task<ActionResult<ServiceResponse<WeaponAttackResultDto>>> WeaponAttack(WeaponAttackDto weaponAttack)
        {
            var response = await _fightService.WeaponAttack(weaponAttack);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("SkillAttack")]
        public async Task<ActionResult<ServiceResponse<SkillAttackResultDto>>> SkillAttack(SkillAttackDto skillAttack)
        {
            var response = await _fightService.SkillAttack(skillAttack);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("Fight")]
        public async Task<ActionResult<ServiceResponse<SkillAttackResultDto>>> Fight(FightRequestDto fightRequest)
        {
            var response = await _fightService.Fight(fightRequest);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("ScoreList")]
        public async Task<ActionResult<ServiceResponse<List<HighScoreDto>>>> Score()
        {
            var response = await _fightService.Score();
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        
    }
}
