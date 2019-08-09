public class SpecialEffect : SpaceDamage
{
    public SpecialEffect(HexCoordinates target, HexDirection direction, WeaponDamageType damageType) : base(target, direction, damageType)
    {
        ImpactedTile = target;
        ImpactDirection = direction;
        DirectionalMod = direction;
        WeaponDamageType = damageType;

        _IsSpecial = true;
    }
    public System.Action KludgeAction;
}