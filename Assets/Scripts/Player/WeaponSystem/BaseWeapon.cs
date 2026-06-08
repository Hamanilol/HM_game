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

    // Visual recoil runtime
    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private Vector3 currentVisualTranslation;
    private Vector3 currentVisualRotation;
    private Vector3 translationVelocity;
    private Vector3 rotationVelocity;

    // State
    protected float nextFireTime = 0f;
    protected bool isReloading = false;
    public bool needsPump = false;        // True after firing a pump-action

    // Recoil accumulator (camera reads this)
    public Vector3 recoilRequest { get; set; }
    
    // Store last hit point for visual synchronization
    protected Vector3 lastHitPoint;

    // Reload coroutine reference for shell-by-shell interruption
    protected Coroutine reloadCoroutine;

    public static float GlobalDamageMultiplier = 1.0f;

    // The camera this weapon should aim/raycast through. Resolved from the weapon's
    // parent hierarchy (each player parents weapons under their own camera), falling
    // back to Camera.main. This makes split-screen co-op aim correctly per player
    // instead of every weapon using the single Camera.main.
    private Camera _ownerCamera;
    protected Camera OwnerCamera
    {
        get
        {
            if (_ownerCamera == null)
                _ownerCamera = GetComponentInParent<Camera>();
            if (_ownerCamera == null)
                _ownerCamera = Camera.main;
            return _ownerCamera;
        }
    }

    [Header("Visual Recoil (Weapon Model)")]
    public Vector3 visualRecoilTranslation = new Vector3(0f, 0.02f, -0.05f); // X=right, Y=up, Z=forward (backward negative)
    public Vector3 visualRecoilRotation    = new Vector3(-3f, 0f, 0f);       // X=pitch, Y=yaw, Z=roll
    public float   visualRecoilRandomHorizontal = 0.01f;                    // Random horizontal jitter
    public float   visualRecoilSpringFrequency  = 10f;                      // How fast the spring oscillates
    [Range(0f, 1f)]
    public float   visualRecoilSpringDamping    = 0.4f;                     // 0 = no damping, 1 = critically damped (no overshoot)

    private static float Spring(float current, float target, ref float velocity, float frequency, float damping, float deltaTime)
    {
        float angularFrequency = frequency * 2f * Mathf.PI;
        float f = 1f + 2f * deltaTime * damping * angularFrequency;
        float oo = angularFrequency * angularFrequency;
        float hoo = deltaTime * oo;
        float hhoo = deltaTime * hoo;
        float detInv = 1f / (f + hhoo);
        float detX = f * current + deltaTime * velocity + hhoo * target;
        float detV = velocity + hoo * (target - current);
        velocity = detV * detInv;
        return detX * detInv;
    }

    protected virtual void Start()
    {
        currentAmmo = maxAmmo;

        // Capture the original local transform (relative to WeaponHolder)
        baseLocalPosition = transform.localPosition;
        baseLocalRotation = transform.localRotation;

        // Initialize offsets to zero
        currentVisualTranslation = Vector3.zero;
        currentVisualRotation = Vector3.zero;
        translationVelocity = Vector3.zero;
        rotationVelocity = Vector3.zero;
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
        if (isAutomatic && Input.GetMouseButton(0) && Time.time >= nextFireTime)
            TryFire();

        // --- Visual recoil spring ---
        float dt = Time.deltaTime;

        // Translation
        currentVisualTranslation.x = Spring(currentVisualTranslation.x, 0f, ref translationVelocity.x, visualRecoilSpringFrequency, visualRecoilSpringDamping, dt);
        currentVisualTranslation.y = Spring(currentVisualTranslation.y, 0f, ref translationVelocity.y, visualRecoilSpringFrequency, visualRecoilSpringDamping, dt);
        currentVisualTranslation.z = Spring(currentVisualTranslation.z, 0f, ref translationVelocity.z, visualRecoilSpringFrequency, visualRecoilSpringDamping, dt);

        // Rotation
        currentVisualRotation.x = Spring(currentVisualRotation.x, 0f, ref rotationVelocity.x, visualRecoilSpringFrequency, visualRecoilSpringDamping, dt);
        currentVisualRotation.y = Spring(currentVisualRotation.y, 0f, ref rotationVelocity.y, visualRecoilSpringFrequency, visualRecoilSpringDamping, dt);
        currentVisualRotation.z = Spring(currentVisualRotation.z, 0f, ref rotationVelocity.z, visualRecoilSpringFrequency, visualRecoilSpringDamping, dt);

        // Apply to the weapon's transform
        transform.localPosition = baseLocalPosition + currentVisualTranslation;
        transform.localRotation = baseLocalRotation * Quaternion.Euler(currentVisualRotation);
    }

    protected virtual void Fire()
    {
        currentAmmo--;
        AddRecoil();
        AddVisualRecoil();
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
        if (OwnerCamera == null) OwnerCamera = GetComponentInParent<Camera>();
        if (OwnerCamera == null) return;

        Ray ray = new Ray(OwnerCamera.transform.position, OwnerCamera.transform.forward);
        RaycastHit hit;
        Vector3 targetPoint;

        // Ignore the player's own layer
        int layerMask = ~LayerMask.GetMask("Player");

        if (Physics.Raycast(ray, out hit, range, layerMask))
        {
            targetPoint = hit.point;
            // Apply damage to any enemy hit (searches the hit object and its parents).
            Abdulrahman.EnemySystem.EnemyHealth.DealDamageToEnemies(hit.collider.gameObject, damage * GlobalDamageMultiplier);
            Debug.Log($"[BaseWeapon] Hit: {hit.collider.gameObject.name} at {hit.point}");
        }
        else
        {
            targetPoint = ray.GetPoint(range); // max range point if nothing hit
        }

        lastHitPoint = targetPoint;

        // Now spawn a visual tracer from the muzzle to the targetPoint
        SpawnTracer(muzzlePoint.position, targetPoint);
    }

    // Shotgun cone blast
    protected virtual void PerformShotgunRaycast()
    {
        Camera cam = OwnerCamera;
        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector3 direction = cam.transform.forward;
            direction = Quaternion.Euler(Random.Range(-spreadAngle, spreadAngle),
                                        Random.Range(-spreadAngle, spreadAngle), 0) * direction;

            Vector3 origin = cam.transform.position;
            Ray ray = new Ray(origin, direction);
            RaycastHit hit;
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out hit, range))
            {
                targetPoint = hit.point;
                // Each pellet deals a fraction of the total damage.
                Abdulrahman.EnemySystem.EnemyHealth.DealDamageToEnemies(hit.collider.gameObject, (damage / pelletsPerShot) * GlobalDamageMultiplier);
            }
            else
                targetPoint = ray.GetPoint(range);

            // Tracer from muzzle, but the visual spread will be slightly off from actual hit.
            // This is acceptable for shotguns.
            SpawnTracer(muzzlePoint.position, targetPoint);
        }
    }

    protected virtual void PlayFireEffects()
    {
        if (weaponAnimator != null)
            weaponAnimator.SetTrigger("Fire");

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

    public bool IsAiming { get; protected set; }

    // Other utilities
    public virtual void SetAiming(bool aiming) 
    {
        IsAiming = aiming;
    }

    // Animation Event Hooks
    public virtual void OnAnimationShoot() { }
    public virtual void OnAnimationCasingRelease() { }

    protected virtual void SpawnTracer(Vector3 from, Vector3 to)
    {
        // Simple line renderer, or instantiate a tracer prefab
        // Using a temporary line for demo purposes:
        Debug.DrawLine(from, to, Color.red, 0.1f);

        // For a proper effect, you could instantiate a trail renderer or
        // particle system that travels from `from` to `to`.
        // Example: if (tracerPrefab) { ... Instantiate and set start/end ... }
    }

    protected virtual void AddVisualRecoil()
    {
        Vector3 translationImpulse = visualRecoilTranslation;
        translationImpulse.x += Random.Range(-visualRecoilRandomHorizontal, visualRecoilRandomHorizontal);

        currentVisualTranslation += translationImpulse;
        currentVisualRotation += visualRecoilRotation;
    }
}