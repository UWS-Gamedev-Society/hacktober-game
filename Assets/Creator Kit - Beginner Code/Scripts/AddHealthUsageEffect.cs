using System.Collections;
using System.Collections.Generic;
using CreatorKitCode;
using UnityEngine;

public class AddHealthUsageEffect : UsableItem.UsageEffect
{
    public int HealthPurcentageAmount = 20;
    
    public override bool Use(CharacterData user)
    {
        if (user.Stats.CurrentHealth == user.Stats.stats.health)
            return false;

        VFXManager.PlayVFX(VFXType.Healing, user.transform.position);

        user.Stats.ChangeHealth( Mathf.FloorToInt(HealthPurcentageAmount/100.0f * user.Stats.stats.health) );

        return true;
    }
}