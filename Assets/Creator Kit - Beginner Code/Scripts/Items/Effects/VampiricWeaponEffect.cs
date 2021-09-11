using System.Collections;
using System.Collections.Generic;
using CreatorKitCode;
using UnityEngine;

public class VampiricWeaponEffect : Weapon.WeaponAttackEffect
{
    public int PercentageHealthStolen;
    
    public override string GetDescription()
    {
        return $"Convert {PercentageHealthStolen}% of physical damage into Health";
    }

    public override void OnPostAttack(CharacterData target, CharacterData user, Weapon.AttackData data)
    {
        int amount = Mathf.FloorToInt(data.GetDamage(StatSystem.DamageType.Physical) * (PercentageHealthStolen / 100.0f));
        user.Stats.ChangeHealth(amount);
    }
}
