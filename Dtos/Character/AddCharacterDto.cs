using dotnet_rpg.Models;

namespace dotnet_rpg.Dtos.Character
{
    public class AddCharacterDto
    {
        public string Name { get; set; } = string.Empty;
        public int HitPoints { get; set; }
        public int ManaPoints { get; set; }
        public int Defense { get; set; }
        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Intelligence { get; set; }
        public RpgClass Classification { get; set; }
        public BaseAttribute BaseAttribute { get; set; }
    }
}
