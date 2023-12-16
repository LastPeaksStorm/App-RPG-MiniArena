using AutoMapper;
using dotnet_rpg.Dtos.Fight;
using dotnet_rpg.Dtos.Skill;
using dotnet_rpg.Dtos.User;
using dotnet_rpg.Dtos.Weapon;

namespace dotnet_rpg
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Character, GetCharacterDto>();
            CreateMap<AddCharacterDto, Character>();
            CreateMap<Weapon, GetWeaponDto>();
            CreateMap<AddSkillDto, Skill>();
            CreateMap<Skill, GetSkillDto>();
            CreateMap<Character, HighScoreDto>();
            CreateMap<GetUserDto, User>();
        }
    }
}
