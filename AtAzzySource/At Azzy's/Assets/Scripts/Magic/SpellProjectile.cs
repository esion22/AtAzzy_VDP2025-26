using PixelAdventures.Scripts.Completed_Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

/* 
 * This class represents a spell projectile that moves in a specified direction,
 * accelerates to a maximum speed, and interacts with objects it collides with.
 */
public class SpellProjectile : MonoBehaviour
{
    private Vector2 _direction;
    private SpellEnum _spellType;

    private float _maxSpeed;
    private float _acceleration;
    private float _currentSpeed;
    [SerializeField] private int ttl;
    
    [Header("Spell sprites")]
    [SerializeField] private Sprite fireSprite;
    [SerializeField] private Sprite waterSprite;
    [SerializeField] private Sprite growthSprite;
    [SerializeField] private Sprite cutSprite;
    
    [Header("Spell sound effects")]
    [SerializeField] private AudioClip fireSpellSFX;
    [SerializeField] private AudioClip waterSpellSFX;
    [SerializeField] private AudioClip growthSpellSFX;
    [SerializeField] private AudioClip cutSpellSFX;
    [SerializeField] private bool playHit;

    [Header("Animation")] [SerializeField] private float scaleUpDuration;

    [Header("Hit Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip fireHit;
    [SerializeField] private AnimationClip waterHit;
    [SerializeField] private AnimationClip growthHit;
    [SerializeField] private AnimationClip cutHit;
    
    
    private float _scaleUpTimer = 0f;
    private bool _isScalingUp = true;
    
    private AudioManager _audioManager;

    /*
     * Initializes the spell projectile with the specified parameters.
     * @param direction The direction in which the projectile will move.
     * @param maxSpeed The maximum speed of the projectile.
     * @param acceleration The acceleration of the projectile.
     * @param spell The type of spell this projectile represents.
     */
    public void Initialize(Vector2 direction, float maxSpeed, float acceleration, SpellEnum spell)
    {
        this._direction = direction.normalized;
        this._maxSpeed = Mathf.Max(0f, maxSpeed);
        this._acceleration = Mathf.Max(0f, acceleration);
        this._spellType = spell;
        
        _audioManager = FindObjectOfType<AudioManager>();
        
        
        // Set the appropriate sprite based on the spell type
        // If no sprite is assigned, use a default color
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        switch (spell)
        {
            case SpellEnum.FIRE:
                if (fireSprite != null)
                {
                    spriteRenderer.sprite = fireSprite;
                }
                else
                {
                    Debug.LogWarning("No Fire Sprite Found");
                    spriteRenderer.color = Color.red;
                }
                break;
            case SpellEnum.WATER:
                if (waterSprite != null)
                {
                    spriteRenderer.sprite = waterSprite;
                    // if the scene is the farm scene
                    var scene = SceneManager.GetActiveScene();
                    if (scene.name == "FarmScene")
                    {
                        // change the layer of the spell to "Farming Magic Interactable"
                        int waterMagicFarmLayer = LayerMask.NameToLayer("Farming Magic Interactable WATER");
                        gameObject.layer = waterMagicFarmLayer;
                    }
                }
                else
                {
                    Debug.LogWarning("No Water Sprite Found");
                    spriteRenderer.color = Color.blue;
                }
                break;
            case SpellEnum.GROWTH:
                if (growthSprite != null)
                {
                    spriteRenderer.sprite = growthSprite;
                    // if the scene is the farm scene
                    var scene = SceneManager.GetActiveScene();
                    if (scene.name == "FarmScene")
                    {
                        // change the layer of the spell to "Farming Magic Interactable"
                        int waterMagicFarmLayer = LayerMask.NameToLayer("Farming Magic Interactable GROWTH");
                        gameObject.layer = waterMagicFarmLayer;
                    }
                }
                else
                {
                    Debug.LogWarning("No Growth Sprite Found");
                    spriteRenderer.color = Color.green;
                }
                break;
            case SpellEnum.CUT:
                if (cutSprite != null)
                {
                    spriteRenderer.sprite = cutSprite;
                }
                else
                {
                    Debug.LogWarning("No Cut Sprite Found");
                    spriteRenderer.color = Color.black;
                }
                break;
        }

        // Set the rotation of the projectile to face the direction
        // make sure to offset by -90 degrees on the z axis
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /* 
     * Unity Start method called on the frame when the script is enabled
     */
    void Start()
    {
        // Start at scale 0
        transform.localScale = Vector3.zero;
        
        // Disable animator - no animation during travel
        animator.enabled = false;
        
        // Auto-destroy the projectile after 5 seconds to prevent memory leaks
        Destroy(gameObject, ttl);
    }

    /*
     * Unity Update method called once per frame
     */
    void Update()
    {
        // Handle scale-up animation
        UpdateScaleUp();
        // Move the projectile in the specified direction
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, _maxSpeed, _acceleration * Time.deltaTime);
        transform.position += _currentSpeed * Time.deltaTime * (Vector3)_direction;
    }
    
    /*
     * Handles the scale-up animation of the projectile
     */
    private void UpdateScaleUp()
    {
        if (!_isScalingUp) return;

        _scaleUpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_scaleUpTimer / scaleUpDuration);
    
        // Smooth scaling using easing
        float scale = Mathf.Lerp(0f, 1f, t);
        transform.localScale = Vector3.one * scale;

        if (t >= 1f)
        {
            _isScalingUp = false;
        }
    }

    /*
     * Unity method called when the projectile collides with another collider
     * @param other The collider that the projectile has collided with
     */
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the hit object implements ISpellInteractable
        ISpellInteractable hitInteractable = other.GetComponent<ISpellInteractable>();
        if (hitInteractable != null)
        {
            // Stop the projectile movement
            _maxSpeed = 0f;
            _currentSpeed = 0f;
            
            // Disable the collider to prevent multiple hits
            GetComponent<Collider2D>().enabled = false;
            
            // Get the correct animation clip directly
            AnimationClip hitClip = null;
            switch (_spellType)
            {
                case SpellEnum.FIRE:
                    hitClip = fireHit;
                    if (playHit) _audioManager.PlaySFXVolumeReduced(fireSpellSFX, 0.5f);
                    break;
                case SpellEnum.WATER:
                    hitClip = waterHit;
                    if (playHit) _audioManager.PlaySFXVolumeReduced(waterSpellSFX, 0.5f);
                    break;
                case SpellEnum.GROWTH:
                    hitClip = growthHit;
                    if (playHit) _audioManager.PlaySFXVolumeReduced(growthSpellSFX, 0.5f);
                    break;
                case SpellEnum.CUT:
                    hitClip = cutHit;
                    if (playHit) _audioManager.PlaySFXVolumeReduced(cutSpellSFX, 0.5f);
                    break;
            }

            // Call the OnSpellHit method on the hit object
            hitInteractable.OnSpellHit(this._spellType);

            // Play animation using Animation component instead
            PlayHitAnimationDirectly(hitClip);
        }
        else
        {
            // Check for SpriteRenderer or TilemapRenderer with "Walls" or "Decorations" sorting layer
            bool shouldStop = false;
            string sortingLayer = null;

            SpriteRenderer spriteRenderer = other.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                sortingLayer = spriteRenderer.sortingLayerName;
                shouldStop = sortingLayer == "Walls" || sortingLayer == "Decorations";
            }
            else
            {
                UnityEngine.Tilemaps.TilemapRenderer tilemapRenderer =
                    other.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>();
                if (tilemapRenderer != null)
                {
                    sortingLayer = tilemapRenderer.sortingLayerName;
                    shouldStop = sortingLayer == "Walls" || sortingLayer == "Decorations";
                }
            }

            if (shouldStop)
            {
                // Stop movement
                _maxSpeed = 0f;
                _currentSpeed = 0f;

                // Disable collider
                GetComponent<Collider2D>().enabled = false;

                // Get the hit animation clip
                AnimationClip hitClip = null;
                switch (_spellType)
                {
                    case SpellEnum.FIRE:
                        hitClip = fireHit;
                        if (playHit) _audioManager.PlaySFXVolumeReduced(fireSpellSFX, 0.5f);
                        break;
                    case SpellEnum.WATER:
                        hitClip = waterHit;
                        if (playHit) _audioManager.PlaySFXVolumeReduced(waterSpellSFX, 0.5f);
                        break;
                    case SpellEnum.GROWTH:
                        hitClip = growthHit;
                        if (playHit) _audioManager.PlaySFXVolumeReduced(growthSpellSFX, 0.5f);
                        break;
                    case SpellEnum.CUT:
                        hitClip = cutHit;
                        if (playHit) _audioManager.PlaySFXVolumeReduced(cutSpellSFX, 0.5f);
                        break;
                }

                // Play the hit animation (this will destroy after animation completes)
                PlayHitAnimationDirectly(hitClip);
            }
        }
    }
    
    private void PlayHitAnimationDirectly(AnimationClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SpellProjectile] No hit animation clip found!");
            Destroy(gameObject, 0.5f);
            return;
        }

        // Create a NEW instance of the override controller
        AnimatorOverrideController overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

        // Get all current overrides
        var overrides = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);

        Debug.Log($"[SpellProjectile] Found {overrides.Count} clips to override");
        foreach (var pair in overrides)
        {
            Debug.Log($"[SpellProjectile] Original clip: {pair.Key?.name ?? "NULL"}");
        }

        // Override ALL clips with the new one (simple approach since you only have one state)
        for (int i = 0; i < overrides.Count; i++)
        {
            overrides[i] = new System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, clip);
        }

        overrideController.ApplyOverrides(overrides);

        // Apply the controller
        animator.runtimeAnimatorController = overrideController;

        // Enable and play
        animator.enabled = true;
        animator.Play("Hit", 0, 0f);

        Debug.Log($"[SpellProjectile] Playing animation: {clip.name}, length: {clip.length}s");

        Destroy(gameObject, clip.length);
    }

}