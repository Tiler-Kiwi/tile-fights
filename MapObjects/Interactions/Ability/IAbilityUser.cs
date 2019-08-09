using System;
using System.Collections.Generic;

public interface IAbilityUser 
{
    List<Ability> MyAbilities { get; }
    void AssignAbility(Ability ability);
    void RemoveAbility(Ability ability);

    WeaponDamageType WeaponDamageType { get; }
    float BaseDamage { get; }
    int AttackRange { get; }
    HexCoordinates? Step(HexCoordinates source, HexDirection dir, EntityMapping sourceMap);
    //Actor Actor { get; } //try to avoid using, used right now for faction check
                         // assigning methods to get numbers rather than just holding values...?
    Faction Faction { get; }
    HexCoordinates? AbilitySourceHex { get; }
    string name { get; set; }
    //EventHandler<ActorTurnOver> TurnOverEvent { get; set; }
}
