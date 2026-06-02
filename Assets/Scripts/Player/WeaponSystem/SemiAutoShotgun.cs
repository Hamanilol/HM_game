public class SemiAutoShotgun : BaseWeapon
{
    // Inspector:
    //   isAutomatic = false
    //   isShotgun = true
    //   isPumpAction = false
    //   reloadsOneByOne = true   <-- still loads individual shells
    //   pelletsPerShot, spreadAngle as desired

    protected override bool CanFireWhileReloading()
    {
        return true; // Can fire during shell reload
    }

    protected override void Fire()
    {
        InterruptReload();  // Stop inserting shells when firing
        base.Fire();
    }
}