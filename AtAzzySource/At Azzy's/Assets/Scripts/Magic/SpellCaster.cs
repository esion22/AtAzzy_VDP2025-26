using PixelAdventures.Scripts.Completed_Scripts;
using UnityEngine;

/* This class handles spell casting mechanics, including raycasting to detect interactable objects,
 * highlighting them, and casting spell projectiles or directly interacting with them based on player input.
 */
public class SpellCaster : MonoBehaviour
{
    // LayerMask to specify which layers are interactable
    [SerializeField] LayerMask interactableLayer;

    // Spell settings
    [Header("Spell Settings")] 
    [SerializeField] private bool enableSpellProjectiles = true;
    [SerializeField] private GameObject spellProjectilePrefab;
    [SerializeField] private float projectileMaxSpeed = 20f;
    [SerializeField] private float projectileAcceleration = 20f;
    
    // Reference to the game camera
    [Header("References")] 
    [SerializeField] private Camera gameCamera;
    
    [Header("Sfx")]
    [SerializeField] private AudioClip fireSpellSfx;
    [SerializeField] private AudioClip waterSpellSfx;
    [SerializeField] private AudioClip growthSpellSfx;
    [SerializeField] private AudioClip cutSpellSfx;

    // Reference to the last hit GameObject and its original color
    private Collider2D _lastHitCollider;
    
    // Reference to the AudioManager
    private AudioManager _audioManager;
    
    // Which spell is currently selected
    public SpellEnum currentSpell = SpellEnum.FIRE;

    
    /*
     * Initialize references
     */
    private void Awake()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
            Debug.LogError("SpellCaster: no main camera found in the scene.");
        }
        _audioManager = FindObjectOfType<AudioManager>();
    }
    
    /*
     * Update is called once per frame
     */
    void Update()
    {
        // Do nothing if the game is paused
        if (Time.timeScale == 0f) return;
        
        
        // Get mouse position in world space
        Vector3 mouseWorldPosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f;

        // Perform raycast in that direction
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero, 0f, interactableLayer);

        // Get the currently hit collider
        Collider2D currentHitCollider = hit.collider;

        // Handle highlighting
        if (currentHitCollider != _lastHitCollider)
        {
            // Remove the highlight from the last hit collider
            if (_lastHitCollider != null)
            {
                Highlightable oldHighlightable = _lastHitCollider.GetComponent<Highlightable>();
                if (oldHighlightable != null)
                {
                    oldHighlightable.Unhighlight();
                }
            }
            
            // Highlight the current hit collider
            if (currentHitCollider != null)
            {
                Highlightable newHighlightable = currentHitCollider.GetComponent<Highlightable>();
                if (newHighlightable != null)
                {
                    newHighlightable.Highlight();
                }
            }
            
            // update the last hit collider
            _lastHitCollider = currentHitCollider;
        }
        
        // if we scroll the mouse wheel, change the current spell
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll < 0f)
        {
            // Scroll up: next spell, skipping NONE
            int nextSpell = ((int)currentSpell + 1);
            if (nextSpell >= System.Enum.GetValues(typeof(SpellEnum)).Length - 1) //if it becomes null
            {
                nextSpell = 0; // Wrap around to the first actual spell (FIRE)
            }
            currentSpell = (SpellEnum)nextSpell;
        }
        else if (scroll > 0f)
        {
            // Scroll down: previous spell, skipping NONE
            int prevSpell = ((int)currentSpell - 1);
            if (prevSpell < 0) // If it becomes negative
            {
                prevSpell = System.Enum.GetValues(typeof(SpellEnum)).Length - 2; // Wrap around to the last spell, skipping NONE
            }
            currentSpell = (SpellEnum)prevSpell;
        }
        // Cast spell projectile on button press
        if (Input.GetButtonDown("Spell Fire") || Input.GetButtonDown("Spell Water") || Input.GetButtonDown("Spell Growth") ||
            Input.GetButtonDown("Spell Cut"))
        {
            // get the pressed button
            SpellEnum selectedSpell = SpellEnum.NONE;

            // Determine which spell button was pressed
            if (Input.GetButtonDown("Spell Fire"))
            {
                selectedSpell = SpellEnum.FIRE;
            }
            else if (Input.GetButtonDown("Spell Water"))
            {
                selectedSpell = SpellEnum.WATER;
            }
            else if (Input.GetButtonDown("Spell Cut"))
            {
                selectedSpell = SpellEnum.CUT;
            }
            else if (Input.GetButtonDown("Spell Growth"))
            {
                selectedSpell = SpellEnum.GROWTH;
            }

            // Update the current spell
            currentSpell = selectedSpell;
        }
        
        // if the player presse the left mouse button, cast the spell
        if (Input.GetMouseButtonDown(0))
        {
            // play the corresponding sound effect
            switch (currentSpell)
            {
                case SpellEnum.FIRE:
                    _audioManager.PlaySFX(fireSpellSfx);
                    break;
                case SpellEnum.WATER:
                    _audioManager.PlaySFX(waterSpellSfx);
                    break;
                case SpellEnum.GROWTH:
                    _audioManager.PlaySFX(growthSpellSfx);
                    break;
                case SpellEnum.CUT:
                    _audioManager.PlaySFX(cutSpellSfx);
                    break;
            }
            // If projectiles are enabled, instantiate and launch the projectile
            if (spellProjectilePrefab != null && enableSpellProjectiles)
            {
                // instantiate at the center of the player's sprite
                Vector2 spriteCenter = GetComponent<SpriteRenderer>().bounds.center;
                
                Vector3 spriteCenter3D = new Vector3(spriteCenter.x, spriteCenter.y, 0f);
                
                // Calculate direction from sprite center to mouse position
                Vector3 direction = (mouseWorldPosition - spriteCenter3D).normalized;
                
                // Instantiate the spell projectile at the player's position
                GameObject projectileGO = Instantiate(spellProjectilePrefab, spriteCenter, Quaternion.identity);
                
                // Initialize the projectile with direction, speed, acceleration, and spell type
                SpellProjectile projectile = projectileGO.GetComponent<SpellProjectile>();
                projectile.Initialize(direction, projectileMaxSpeed, projectileAcceleration, currentSpell);
            }
            else
            {
                // If projectiles are disabled, directly interact with the hovered object
                if (hit.collider != null)
                {
                    // Get the ISpellInteractable component from the hit object
                    ISpellInteractable interactable = hit.collider.GetComponent<ISpellInteractable>();
                    if (interactable != null)
                    {
                        // Call the OnSpellHit method on the interactable object
                        interactable.OnSpellHit(currentSpell);
                    }
                }
            }
        }
    }
}
