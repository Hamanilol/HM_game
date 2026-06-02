public class PumpActionShotgun : BaseWeapon
{
    // Inspector settings:
    //   isAutomatic = false
    //   isShotgun = true
    //   isPumpAction = true
    //   reloadsOneByOne = true   <-- tube-fed, loads one shell at a time
    //   pelletsPerShot, spreadAngle as desired
    //   singleShellReloadTime ~ 0.5s

    // Firing while reloading is allowed – CanFireWhileReloading() returns true in base.

    protected override bool CanFireWhileReloading()
    {
        // Allow firing even while inserting shells
        return isReloading;
    }

    // If the player fires during reload, interrupt the shell loading
    protected override void Fire()
    {
        // Interrupt shell-by-shell reload if active
        InterruptReload();
        base.Fire();
    }
}