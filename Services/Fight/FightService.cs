using AutoMapper;
using dotnet_rpg.Dtos.Fight;
using dotnet_rpg.Models;
using dotnet_rpg.Services.Character;
using Microsoft.Extensions.Http;
using System.Diagnostics.Eventing.Reader;
using System.Net.Mail;
using System.Reflection;
using System.Security.Claims;

namespace dotnet_rpg.Services.Fight
{
    public class FightService : IFightService
    {
        static readonly Random r = new();
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public FightService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User.
            FindFirstValue(ClaimTypes.NameIdentifier)!);

        private static void DoWeaponAttack(Models.Character attacker, Models.Character opponent, out int damageToOpponent, out int damageToAttacker)
        {
            damageToOpponent = attacker!.Weapon == null ? 0 : attacker!.Weapon!.Damage + r.Next(-10, 10) - opponent!.Defense;
            damageToAttacker = opponent!.Weapon == null ? 0 : opponent!.Weapon!.Damage + r.Next(-10, 10) - attacker!.Defense;
            damageToOpponent = damageToOpponent < 0 ? 0 : damageToOpponent;
            damageToAttacker = damageToAttacker < 0 ? 0 : damageToAttacker;

            if (damageToOpponent > 0)
                opponent.HitPoints -= damageToOpponent;
            if (damageToAttacker > 0)
                attacker.HitPoints -= damageToAttacker;
        }

        private static int DoSkillAttack(Models.Character? opponent, Models.Skill? skill)
        {
            var damage = skill!.Damage + r.Next(-10, 10) - opponent!.Defense;
            damage = damage < 0 ? 0 : damage;

            if (damage > 0)
                opponent.HitPoints -= damage;
            return damage;
        }

        public async Task<ServiceResponse<WeaponAttackResultDto>> WeaponAttack(WeaponAttackDto weaponAttack)
        {
            var response = new ServiceResponse<WeaponAttackResultDto>();
            try
            {
                var attacker = await _context.Characters
                    .Include(c => c.Weapon)
                    .FirstOrDefaultAsync(c => c.Id.Equals(weaponAttack.AttackerId) && c.User!.Id.Equals(GetUserId()));

                var opponent = await _context.Characters
                    .Include(c => c.Weapon)
                    .FirstOrDefaultAsync(c => c.Id.Equals(weaponAttack.OpponentId));

                if (opponent is null || attacker is null)
                    throw new Exception("Character not found or current user has no permission to utilize him.");

                else if (opponent == attacker)
                {
                    response.Success = false;
                    response.Message = "Masochist bro is tryna Suicido :-{";
                    return response;
                }

                DoWeaponAttack(attacker, opponent!, out int damageToOpponent, out int damageToAttacker);

                if (opponent!.HitPoints <= 0)
                {
                    opponent.HitPoints = 0;
                    response.Message = $"{attacker.Name} pwned {opponent.Name}\'s head for freee";
                    attacker.Kills++;
                    opponent.Deaths++;
                }
                if (attacker.HitPoints <= 0)
                {
                    attacker.HitPoints = 0;
                    response.Message = $"{opponent.Name} pwned {attacker.Name}\'s head for freee";
                    attacker.Deaths++;
                    opponent.Kills++;
                }

                attacker.HitPoints = 800;
                opponent.HitPoints = 800;
                await _context.SaveChangesAsync();

                response.Data = new WeaponAttackResultDto
                {
                    Attacker = attacker.Name,
                    Opponent = opponent.Name,
                    AttackerHP = attacker.HitPoints,
                    OpponentHP = opponent.HitPoints,
                    DamageDealtToOpponent = damageToOpponent,
                    DamageDealtToAttacker = damageToAttacker
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<SkillAttackResultDto>> SkillAttack(SkillAttackDto skillAttack)
        {
            var response = new ServiceResponse<SkillAttackResultDto>();
            try
            {
                var attacker = await _context.Characters
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.Id.Equals(skillAttack.AttackerId) && c.User!.Id.Equals(GetUserId()));

                var opponent = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id.Equals(skillAttack.OpponentId));

                if (opponent is null || attacker is null)
                    throw new Exception("Character not found or current user has no permission to utilize him.");

                else if (opponent == attacker)
                {
                    response.Success = false;
                    response.Message = "Masochist bro is tryna Suicido :-{";
                    return response;
                }

                var skill = attacker.Skills!.FirstOrDefault(s => s.Id.Equals(skillAttack.SkillId));

                if (skill is null)
                    throw new Exception($"{attacker.Name} doesn't know that skill.");

                var damage = DoSkillAttack(opponent, skill);

                if (opponent.HitPoints <= 0)
                {
                    opponent.HitPoints = 0;
                    response.Message = $"{attacker.Name} pwned {opponent.Name}\'s head for freee";
                    attacker.Kills++;
                    opponent.Deaths++;
                }

                attacker.HitPoints = 800;
                opponent.HitPoints = 800;
                await _context.SaveChangesAsync();

                response.Data = new SkillAttackResultDto
                {
                    Attacker = attacker.Name,
                    Opponent = opponent.Name,
                    AttackerHP = attacker.HitPoints,
                    OpponentHP = opponent.HitPoints,
                    DamageDealt = damage
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<FightResultDto>> Fight(FightRequestDto newFight)
        {
            var response = new ServiceResponse<FightResultDto>
            {
                Data = new FightResultDto
                {
                    FightLogs = new List<string>()
                }
            };

            try
            {
                var hasAuthorizedCharacter = false;

                var characters = await _context.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.Skills)
                    .Include(c => c.User)
                    .Where(c => newFight.CharacterIds.Contains(c.Id))
                    .Distinct()
                    .ToListAsync();
                characters = characters.ToList();

                foreach (var fighter in characters)
                {
                    if (fighter.User!.Id == GetUserId())
                        hasAuthorizedCharacter = true;
                }

                if (!hasAuthorizedCharacter)
                    throw new Exception("To organize a fight u must have at least one of your fighters in!");

                var fightEnds = false;

                string attackerName = "";
                string opponentName = "";

                var fighters = characters.ToList();
                while (!fightEnds)
                {
                    var fightersAlive = fighters.Where(f => f.HitPoints > 0).Count();

                    if (fightersAlive == 1)
                    {
                        var winner = fighters.First(f => f.HitPoints > 0);
                        var others = fighters.Where(f => f != winner).ToList();
                        others.ForEach(f => f.Defeats++);

                        response.Data!.FightLogs.Add($"His name is... {winner.Name}! DOO-DUROO DOOOOO!!!");
                        winner.Victories++;
                        fightEnds = true;
                        break;
                    }

                    else if (fightersAlive == 0)
                    {
                        var winners = fighters.Where(f => f.Name.Equals(attackerName) || f.Name.Equals(opponentName));
                        var others = fighters.Where(f => !winners.Contains(f)).ToList();
                        others.ForEach(f => f.Defeats++);
                        response.Data!.FightLogs.Add($"WOW! {attackerName!} and {opponentName} have a DRAW!!!");
                        fightEnds = true;
                        break;
                    }

                    foreach (var attacker in fighters)
                    {
                        if (attacker.HitPoints == 0)
                            continue;

                        attackerName = attacker.Name;
                        var opponents = fighters.Where(f => f.Id != attacker.Id && f.HitPoints > 0).ToList();

                        var currentOpponent = opponents[r.Next(opponents.Count)];
                        opponentName = currentOpponent.Name;

                        string weaponUsed;

                        var coin = r.Next(2) == 0;
                        if (coin)
                        {
                            weaponUsed = attacker.Weapon == null ? "hands" : attacker.Weapon!.Name;
                            DoWeaponAttack(attacker, currentOpponent, out int damageToOpponent, out int damageToAttacker);
                            response.Data!.FightLogs.Add(
                                $"{attacker.Name} attacked {currentOpponent.Name} with his {weaponUsed}, " +
                                $"dealt {damageToOpponent} damage " +
                                $"and got {damageToAttacker} damage back.");
                        }
                        else
                        {
                            var skills = attacker.Skills!;
                            int damageDealt;
                            if (skills.Count == 0)
                            {
                                weaponUsed = "Incompetence";
                                damageDealt = 0;
                            }
                            else
                            {
                                var currentSkill = skills[r.Next(skills.Count)];
                                weaponUsed = currentSkill.Name;
                                damageDealt = DoSkillAttack(currentOpponent, currentSkill);
                            }
                            response.Data!.FightLogs.Add($"{attacker.Name} attacked {currentOpponent.Name} " +
                                $"with skill: {weaponUsed} and dealt {damageDealt} damage.");
                        }

                        if (currentOpponent.HitPoints <= 0)
                        {
                            currentOpponent.HitPoints = 0;
                            response.Data!.FightLogs.Add($"{attacker.Name} pwned {currentOpponent.Name}\'s head for RESPECT.");
                            attacker.Kills++;
                            currentOpponent.Deaths++;
                        }
                        else
                            response.Data!.FightLogs.Add($"{currentOpponent.Name} has {currentOpponent.HitPoints} HP left.");

                        if (attacker.HitPoints <= 0)
                        {
                            attacker.HitPoints = 0;
                            response.Data!.FightLogs.Add($"{currentOpponent.Name} pwned {attacker.Name}\'s head for RESPECT.");
                            currentOpponent.Kills++;
                            attacker.Deaths++;
                        }
                        else
                            response.Data!.FightLogs.Add($"{attacker.Name} has {attacker.HitPoints} HP left.");
                    }

                }
                foreach (var character in characters)
                {
                    character.Fights++;
                    character.HitPoints = 800;
                }
                await _context.SaveChangesAsync();

                response.Message = "Fight ended successfully!";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<List<HighScoreDto>>> Score()
        {
            var response = new ServiceResponse<List<HighScoreDto>>();

            try
            {
                var fighters = await _context.Characters
                    .Where(c => c.Fights > 0)
                    .OrderByDescending(c => c.Victories)
                    .ThenBy(c => c.Defense)
                    .ThenByDescending(c => c.Kills)
                    .ThenBy(c => c.Deaths)
                    .ToListAsync();

                if (fighters is null)
                    throw new Exception("No character had even a single fight!");

                response.Data = fighters!.Select(f => _mapper.Map<HighScoreDto>(f)).ToList();
                response.Message = $"Current WORLD Champion: {fighters[0].Name}";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
