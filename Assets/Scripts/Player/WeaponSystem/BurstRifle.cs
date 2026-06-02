using UnityEngine;
using System.Collections;

public class BurstRifle : BaseWeapon
{
    public int burstCount = 3;
    public float burstDelay = 0.06f;

    private int shotsRemaining = 0;

    protected override void Fire()
    {
        // Start a burst only if we aren't already mid-burst
        if (shotsRemaining <= 0)
        {
            shotsRemaining = burstCount;
            StartCoroutine(FireBurst());
        }
    }

    IEnumerator FireBurst()
    {
        while (shotsRemaining > 0)
        {
            base.Fire();
            shotsRemaining--;
            yield return new WaitForSeconds(burstDelay);
        }
    }
}