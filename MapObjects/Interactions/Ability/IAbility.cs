public interface IAbility
{
    AbilityEffect GetAbilityEffect(HexCoordinates source, HexCoordinates target);
    void Initialize(IAbilityUser user);
}