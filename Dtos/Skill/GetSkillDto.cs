namespace dotnet_rpg.Dtos.Skill
{
    public class GetSkillDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Damage { get; set; }
    }
}
