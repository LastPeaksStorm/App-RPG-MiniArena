namespace dotnet_rpg.Dtos.Fight
{
    public class FightRequestDto
    {
        public List<int> CharacterIds { get; set; }
        public FightRequestDto()
        {
            CharacterIds = new List<int>();
        }
    }
}
