using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MysticForestCharacterControllerRB : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float characterSpeed = 5;
    [SerializeField] private float movementThreshold = 0.01f;
    [Space(10)]
    [Tooltip("If false the character can move diagonally.")]
    [SerializeField] private bool fourDirectionMovement = true;
    [Space(10)]
    [Tooltip("If false only horizontal animations are played.")]
    [SerializeField] private bool fourDirectionAnimation = true;

    #region AnimatorHashValues
    private static readonly int HorizontalMovement = Animator.StringToHash("HorizontalMovement");
    private static readonly int VerticalMovement = Animator.StringToHash("VerticalMovement");
    private static readonly int Moving = Animator.StringToHash("Moving");
    private static readonly int PreviousHorizontalMovement = Animator.StringToHash("PreviousHorizontalMovement");
    private static readonly int PreviousVerticalMovement = Animator.StringToHash("PreviousVerticalMovement");
    private static readonly int Die = Animator.StringToHash("Die");
    #endregion
    
    private Vector2 _movement;
    private Vector2 _previousMovement;
    
    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    
    private readonly List<Vector2> _heldDirections = new List<Vector2>();
    
    private AudioManager _audioManager;

    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioManager = FindFirstObjectByType<AudioManager>();
    }

    void Update()
    {
        if (PauseMenu.isPaused) return;
        if (Time.timeScale == 0f)
        {
            // Svuota le direzioni quando il gioco è in pausa per evitare movimenti indesiderati alla ripresa.
            if (_heldDirections.Count > 0)
            {
                _heldDirections.Clear();
            }
            _movement = Vector2.zero;
            return;
        }
        if (fourDirectionMovement)
        {
            _movement = GetFourDirectionInput();
        }
        else
        {
            _movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        HandleSpriteFlip();
    }
    
    private Vector2 GetFourDirectionInput()
    {
        // ---- Handle Key Down Events ----
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            AddDirection(Vector2.up);

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            AddDirection(Vector2.down);

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            AddDirection(Vector2.left);

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            AddDirection(Vector2.right);

        // ---- Handle Key Up Events ----
        if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W))
            RemoveDirection(Vector2.up);

        if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
            RemoveDirection(Vector2.down);

        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
            RemoveDirection(Vector2.left);

        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D))
            RemoveDirection(Vector2.right);

        // ---- Return current active direction ----
        if (_heldDirections.Count > 0)
            return _heldDirections[_heldDirections.Count - 1]; // last pressed

        return Vector2.zero;
    }

    private void AddDirection(Vector2 dir)
    {
        // Remove if already in list (prevents duplicates)
        _heldDirections.Remove(dir);

        // Add to end → highest priority
        _heldDirections.Add(dir);
    }

    private void RemoveDirection(Vector2 dir)
    {
        _heldDirections.Remove(dir);
    }


    private void HandleSpriteFlip()
    {
        // Flip based on last known horizontal direction
        float x = _movement.x != 0 ? _movement.x : _previousMovement.x;

        if (Math.Abs(x) > movementThreshold)
        {
            _spriteRenderer.flipX = (x < 0); 
        }
    }
    
    private void FixedUpdate()
    {
        float ds = characterSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(_rb.position + ds*_movement);
    }

    private void LateUpdate()
    {
        UpdateSound(_movement);
        if (fourDirectionAnimation)
        {
            UpdateAnimator(_movement);
        }
        else
        {
            UpdateAnimatorTwoDirections(_movement);
        }
    }
    
    private void UpdateSound(Vector2 movement)
    {
        bool moving = movement.x!=0 || movement.y!=0;
        if (moving)
        {
            if (!_audioManager.IsWalkingSoundPlaying())
            {
                _audioManager.PlayWalkingSound();
            }
        }
        else
        {
            // stop walking sound
            _audioManager.StopWalkingSound();
        }
    }
    
    private void UpdateAnimatorTwoDirections(Vector2 movement)
    {
        bool moving = movement.x!=0 || movement.y!=0;

        if (moving)
        {
            if (movement.y != 0)
            {
                // it is moving vertically so we have to use previous x movement
                _animator.SetFloat(HorizontalMovement,_previousMovement.x);
                print(_previousMovement);
            }
            else
            {
                
                _animator.SetFloat(HorizontalMovement,movement.x);    
                _previousMovement = movement;
            }
            
        }
        else
        {
            _animator.SetFloat(PreviousHorizontalMovement, _previousMovement.x);
            _animator.SetFloat(PreviousVerticalMovement, 0f);
        }

        _animator.SetBool(Moving,moving);
    }

    private void UpdateAnimator(Vector2 movement)
    {
        bool moving = movement.x!=0 || movement.y!=0;

        if (moving)
        {
            
            _animator.SetFloat(HorizontalMovement,movement.x);
            _animator.SetFloat(VerticalMovement,movement.y);
            _previousMovement = movement;
        }
        else
        {
            _animator.SetFloat(PreviousHorizontalMovement, _previousMovement.x);
            _animator.SetFloat(PreviousVerticalMovement, _previousMovement.y);
        }

        _animator.SetBool(Moving,moving);
    }

    public void Death()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }
}

