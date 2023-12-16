namespace dotnet_rpg.Dtos.Fight
{
    public class HighScoreDto
    {
        public string Name { get; set; } = string.Empty;
        public int Fights { get; set; }
        public int Victories { get; set; }
        public int Defeats { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
    }
}
