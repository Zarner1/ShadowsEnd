using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Control : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 5.0f;
    public int clickCount = 0;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    [Header("Combat Settings")]
    public Transform attackPoint; 
    public float attackRange = 0.8f; 
    public LayerMask enemyLayers; 
    public int slashDamage = 20; 
    public float specialAttackDuration = 0.5f;

    private float currentSpeed = 0.0f;
    private bool isGrounded;
    private bool isCrouching;
    private float resetTime = 0.2f;
    private float lastClickTime;
    
    // Durum Kontrolü
    private bool swordSlash;
    private bool isSpecialAttacking = false;
    
    // DEĞİŞİKLİK: 'private' yerine 'public' yaptık ki health scripti bunu okuyabilsin.
    public bool isBlocking = false; 

    private bool isDead = false;

    // Components
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        lastClickTime = Time.time;
    }

    void Update()
    {
        if (isDead) return; 

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        HandleBlocking(); 
        HandleMovement();
        HandleJump();
        HandleCrouch();
        HandleAttack();
        UpdateAnimations();
        HandleSpecialAttack();

        if (Time.time - lastClickTime >= resetTime)
        {
            clickCount = 0;
        }
    }

    public void TriggerDeath()
    {
        if (isDead) return; 
        isDead = true; 
        _rigidbody2D.linearVelocity = Vector2.zero; 
        currentSpeed = 0;
        _animator.SetBool("isDead", true);
    }

    private void HandleBlocking()
    {
        if (isGrounded && !isSpecialAttacking)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                isBlocking = true;
                currentSpeed = 0; 
            }
            else
            {
                isBlocking = false;
            }
        }
        else
        {
            isBlocking = false;
        }
    }

    private void HandleMovement()
    {
        if (Input.GetKey(KeyCode.D) && !isSpecialAttacking && !isBlocking && !Input.GetKey(KeyCode.S) && !Input.GetMouseButton(0))
        {
            currentSpeed = moveSpeed;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (Input.GetKey(KeyCode.A) && !isSpecialAttacking && !isBlocking && !Input.GetKey(KeyCode.S) && !Input.GetMouseButton(0))
        {
            currentSpeed = -moveSpeed;
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            currentSpeed = 0.0f;
        }
        _rigidbody2D.linearVelocity = new Vector2(currentSpeed, _rigidbody2D.linearVelocity.y);
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isSpecialAttacking && !isBlocking && !Input.GetKey(KeyCode.S))
        {
            _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocity.x, jumpForce);
        }
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.S) && isGrounded) isCrouching = true;
        if (Input.GetKeyUp(KeyCode.S)) isCrouching = false;
    }

    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && !isSpecialAttacking && !isBlocking)
        {
            clickCount++;
            lastClickTime = Time.time;
        }
    }

    private void HandleSpecialAttack()
    {
        if (Input.GetMouseButtonDown(1) && !isSpecialAttacking && !isBlocking)
        {
            StartCoroutine(SpecialAttackRoutine());
        }
    }

    IEnumerator SpecialAttackRoutine()
    {
        isSpecialAttacking = true; 
        swordSlash = true;         

        SlashAttack(); 

        yield return new WaitForSeconds(specialAttackDuration);

        swordSlash = false;        
        isSpecialAttacking = false; 
    }

    void SlashAttack()
    {
        if (attackPoint == null) return;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach(Collider2D enemy in hitEnemies)
        {
            enemy_knight_movement ai = enemy.GetComponent<enemy_knight_movement>();
            if (ai != null)
            {
                ai.ReceiveDamage(slashDamage);
            }
            else
            {
                health_system health = enemy.GetComponent<health_system>();
                if (health != null) health.TakeDamage(slashDamage);
            }
        }
    }

    private void UpdateAnimations()
    {
        _animator.SetFloat("speed", Mathf.Abs(currentSpeed));
        _animator.SetBool("isJumping", !isGrounded);
        _animator.SetBool("isCrouching", isCrouching);
        _animator.SetInteger("clickCount", clickCount);
        _animator.SetBool("isGrounded", isGrounded);
        _animator.SetBool("swordSlash", swordSlash);
        _animator.SetBool("isBlocking", isBlocking);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}