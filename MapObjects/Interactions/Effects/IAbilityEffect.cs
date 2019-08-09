using System.Collections.Generic;

public interface IAbilityEffect
{
    int Count { get; }

    SpaceDamage GetDamage(int index);
    List<SpaceDamage> GetDamageList();
}