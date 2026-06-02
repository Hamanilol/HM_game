using UnityEngine;
using System.Collections;

public abstract class BaseWeapon : MonoBehaviour
{
    [Header("Weapon ID")]
    public string weaponName = "Unnamed";

    [Header("Ammo")]
    public int maxAmmo = 30;        // Magazine / tube capacity
    public int currentAmmo;
    public int reserveAmmo = 90;    // Ammo carried

    [Header("Fire Settings")]
    public float fireRate = 0.1f;   // Seconds between shots
    public bool isAutomatic = false;
    public float damage = 25f;
    public float range = 100f;

    [Header("Reload")]
    public float reloadTime = 2.5f;          // For magazine reload (full)
    public bool reloadsOneByOne = false;     // Set true for shotguns
    public float singleShellReloadTime = 0.5f;

    [Header("Shotgun Settings")]
    public bool isShotgun = false;
    public int pelletsPerShot = 8;
    public float spreadAngle = 5f;           // Degrees of cone

    [Header("Pump Action")]
    public bool isPumpAction = false;
    public float pumpDuration = 0.5f;        // Time needed to cycle the pump

    [Header("Recoil")]
    public Vector2 recoilPerShot = new Vector2(-2f, 0.5f); // x = pitch, y = yaw amplitude

    [Header("References")]
    public Transform muzzlePoint;
    public GameObject muzzleFlashPrefab;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip pumpSound;
    public Animator weaponAnimator;          // Optional weapon animator

    // State
    protected float nextFireTime = 0f;
    protected bool isReloading = false;
    public bool needsPump = false;        // True after firing a pump-action

    // Recoil accumulator (camera reads this)
    public Vector3 recoilRequest { get; set; }

    // Reload coroutine reference for shell-by-shell interruption
    protected Coroutine reloadCoroutine;

    protected virtual void Start()
    {
        currentAmmo = maxAmmo;
    }

    // Called by external input (semi‑auto)
    public virtual void TryFire()
    {
        if (Time.time < nextFireTime) return;
        if (isReloading && !CanFireWhileReloading()) return;
        if (isPumpAction && needsPump) return;

        if (currentAmmo <= 0)
        {
            // Auto‑reload if we can
            if (!isReloading)
                StartCoroutine(Reload());
            return;
        }

        Fire();
        nextFireTime = Time.time + fireRate;
    }

    // Override for automatic weapons to call TryFire continuously in Update
    protected virtual void Update()
    {
        if (isAutomatic && Input.GetButton("Fire1") && Time.time >= nextFireTime)
            TryFire();
    }

    protected virtual void Fire()
    {
        currentAmmo--;
        AddRecoil();
        PlayFireEffects();

        if (isShotgun)
            PerformShotgunRaycast();
        else
            PerformRaycast();

        // For pump-action: require pump before next shot
        if (isPumpAction)
        {
            needsPump = true;
            // Pump can be called manually by player or automatically after a delay
            // We'll provide a public Pump() method; the player script can call it on input.
        }
    }

    // Standard single hitscan raycast
    protected virtual void PerformRaycast()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log($"Hit {hit.collider.name} for {damage} damage");
            // Apply damage, spawn impact effects
        }
    }

    // Shotgun cone blast
    protected virtual void PerformShotgunRaycast()
    {
        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector3 direction = Camera.main.transform.forward;
            direction = Quaternion.Euler(Random.Range(-spreadAngle, spreadAngle),
                                         Random.Range(-spreadAngle, spreadAngle), 0) * direction;

            if (Physics.Raycast(Camera.main.transform.position, direction, out RaycastHit hit, range))
            {
                Debug.Log($"Pellet hit {hit.collider.name}");
                // Apply pellet damage
            }
        }
    }

    protected virtual void PlayFireEffects()
    {
        if (muzzleFlashPrefab && muzzlePoint)
            Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);

        if (fireSound)
            AudioSource.PlayClipAtPoint(fireSound, muzzlePoint ? muzzlePoint.position : transform.position);
    }

    protected virtual void AddRecoil()
    {
        float randomYaw = Random.Range(-recoilPerShot.y, recoilPerShot.y);
        recoilRequest += new Vector3(recoilPerShot.x, randomYaw, 0f);
    }

    // ----- Reloading -----

    public virtual IEnumerator Reload()
    {
        if (isReloading) yield break;
        if (currentAmmo == maxAmmo) yield break;

        // Shell‑by‑shell reload
        if (reloadsOneByOne)
        {
            reloadCoroutine = StartCoroutine(ShellByShellReload());
            yield return reloadCoroutine;  // Wait until full or interrupted
        }
        else
        {
            // Magazine reload
            isReloading = true;
            if (reloadSound) AudioSource.PlayClipAtPoint(reloadSound, transform.position);
            if (weaponAnimator) weaponAnimator.SetTrigger("Reload");

            yield return new WaitForSeconds(reloadTime);

            int ammoNeeded = maxAmmo - currentAmmo;
            int ammoToAdd = Mathf.Min(ammoNeeded, reserveAmmo);
            currentAmmo += ammoToAdd;
            reserveAmmo -= ammoToAdd;

            isReloading = false;
        }
    }

    protected virtual IEnumerator ShellByShellReload()
    {
        isReloading = true;
        while (currentAmmo < maxAmmo && reserveAmmo > 0)
        {
            // Play shell insert animation/sound
            if (reloadSound) AudioSource.PlayClipAtPoint(reloadSound, transform.position);

            yield return new WaitForSeconds(singleShellReloadTime);

            currentAmmo++;
            reserveAmmo--;
        }
        isReloading = false;
    }

    // Interrupt shell-by-shell reload (e.g., when firing)
    public virtual void InterruptReload()
    {
        if (reloadsOneByOne && isReloading && reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            isReloading = false;
        }
    }

    // Can the weapon fire while reloading? (shotguns usually can)
    protected virtual bool CanFireWhileReloading()
    {
        // Override for shotguns that allow firing during shell reload
        return (reloadsOneByOne && isReloading);
    }

    // ----- Pump Action -----

    public virtual void Pump()
    {
        if (!isPumpAction || !needsPump) return;

        needsPump = false;
        if (pumpSound) AudioSource.PlayClipAtPoint(pumpSound, transform.position);
        if (weaponAnimator) weaponAnimator.SetTrigger("Pump");

        // Optionally delay the next fire slightly
        nextFireTime = Mathf.Max(nextFireTime, Time.time + pumpDuration);
    }

    // Other utilities
    public virtual void SetAiming(bool aiming) { /* Override for ADS */ }
}