namespace dotnet_rpg.Dtos.User
{
    public class GetUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; }
        public List<GetCharacterDto>? Characters { get; set; }
    }
}
