using dotnet_rpg.Dtos.Fight;

namespace dotnet_rpg.Services.Fight
{
    public interface IFightService
    {
        Task<ServiceResponse<WeaponAttackResultDto>> WeaponAttack(WeaponAttackDto weaponAttack);
        Task<ServiceResponse<SkillAttackResultDto>> SkillAttack(SkillAttackDto skillAttack);
        Task<ServiceResponse<FightResultDto>> Fight(FightRequestDto newFight);
        Task<ServiceResponse<List<HighScoreDto>>> Score();
    }
}
