namespace dotnet_rpg.Dtos.Fight
{
    public class FightResultDto
    {
        public List<string> FightLogs { get; set; }
        public FightResultDto()
        {
            FightLogs = new List<string>();
        }
    }
}
