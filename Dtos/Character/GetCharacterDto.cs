using dotnet_rpg.Dtos.Skill;
using dotnet_rpg.Dtos.Weapon;
using dotnet_rpg.Models;

namespace dotnet_rpg.Dtos.Character
{
    public class GetCharacterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int HitPoints { get; set; }
        public int ManaPoints { get; set; }
        public int Defense { get; set; }
        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Intelligence { get; set; }
        public RpgClass Classification { get; set; }
        public BaseAttribute BaseAttribute { get; set; }
        public GetWeaponDto? Weapon { get; set; }
        public List<GetSkillDto>? Skills { get; set; }
        public int Fights { get; set; }
        public int Victories { get; set; }
        public int Defeats { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
    }
}
