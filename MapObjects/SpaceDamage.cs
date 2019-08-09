/// <summary>
/// ugly kludge data, held here to be read by the UI, AI, and BattleMap to determine what the fuck an effect is doing
/// </summary>
public class SpaceDamage
{
    public HexCoordinates ImpactedTile;
    public HexDirection ImpactDirection; // direction the effect is coming from, for flanking or other shit
    public WeaponDamageType WeaponDamageType;
    public float Damage = 0;
    public HexDirection DirectionalMod; //direction things get pushed or turned towards
    public int PushDistance = 0; //use negative values for pulling
    public bool IsProjectile = false;
    public bool ChangeFacing = false;
    public bool ApplyModifier
    {
        get { return AppliedMod != null; }
    }
    public bool RemoveModifier
    {
        get { return RemovedMod != null; }
    }
    public ConstantModifier AppliedMod;

    public bool _IsSpecial = false; //kludge that instead tells stuff to do some special action instead
    internal ConstantModifier RemovedMod;

    public SpaceDamage(HexCoordinates target, HexDirection direction, WeaponDamageType damageType)
    {
        ImpactedTile = target;
        ImpactDirection = direction;
        DirectionalMod = direction;
        WeaponDamageType = damageType;
    }

    public override string ToString()
    {
        string ret = string.Format("Impact Tile: {0} Dir: {1} Dam: {2} DMod: {3} PDist: {4}", 
            ImpactedTile.ToString(), ImpactDirection.ToString(), Damage.ToString(), DirectionalMod.ToString(), PushDistance.ToString());
        return ret;
    }
}
