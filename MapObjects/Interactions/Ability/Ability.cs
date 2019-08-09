using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates an AbilityEffect, and contains logic for gerating said AbilityEffects. All coords are relative to source.
/// </summary>

//I would rather have this broken down into something like a conditional trigger/effect system, but this will be fine for now. No need for extra complexity...
[CreateAssetMenu]
public abstract class Ability : ScriptableObject, IAbility
{
    public bool Initialized = false;
    public UnityEngine.UI.Image AbilityIcon;
    public int CooldownLength = 0;
    public bool BasicAbility = false;
    public int UseLimit = -1;
    public int TimesUsed = 0;
    public string AbilityName = "DefaultAbilityName";
    public WeaponDamageType RequiredWeapon;
    [SerializeField]
    protected int _CooldownCurrent = 0;
    public int CooldownCurrent { get { return _CooldownCurrent; } set { if (value < 0) { value = 0; } _CooldownCurrent = value; } }
    public bool IsMovement = false;

    public IAbilityUser AbilityUser;
    public EntityMapping SourceMap { get { return FindObjectOfType<EntityMapping>(); } }// prefer not to do this but needed for now

    public abstract List<HexCoordinates> GetTargetSpaces(HexCoordinates source, HexDirection dir, EntityMapping entityMap, TileListBase tileMap, IMapCollisionDetection collide);
    public abstract AbilityEffect GetAbilityEffect(HexCoordinates source, HexCoordinates target);

    public bool OnCooldown()
    {
        if (CooldownCurrent <= 0)
        { return false; }
        return true;
    }
    public void TurnChange(object obj, TurnChanged e)
    {
        CooldownCurrent -= 1;
    }
    public void SetCooldown()
    {
        CooldownCurrent = CooldownLength;
    }

    public void OnDestroy()
    {
        /*
        if (SourceMap != null)
        {
            MapBoard register = SourceMap.MyBoard as MapBoard;
            if (register == null) { throw new System.Exception("NOT GOOD"); }
            register.TurnChangedEvent -= TurnChange;
        }
        */
    }

    public void Initialize(IAbilityUser user)
    {
        this.AbilityUser = user;
        Initialized = true;
    }
}

