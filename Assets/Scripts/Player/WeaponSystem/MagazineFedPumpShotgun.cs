public class MagazineFedPumpShotgun : BaseWeapon
{
    // Inspector:
    //   isAutomatic = false
    //   isShotgun = true
    //   isPumpAction = true
    //   reloadsOneByOne = false  <-- detachable box magazine, full reload
    //   reloadTime = 2.5s (full magazine swap)
    //   pumpDuration = 0.5s

    // No need to override Reload – base magazine reload is fine.
    // Must pump after each shot – base class already handles that.

    // Allow firing while reloading? Usually not with a magazine.
    protected override bool CanFireWhileReloading()
    {
        return false;
    }
}