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
    
    [Header("Attack Types")]
    // --- 1. NORMAL SALDIRI AYARLARI (SOL TIK) ---
    public int normalDamage = 10;       // Normal saldırı hasarı
    public float normalAttackCooldown = 0.167f; // 0.167 saniye bekleme süresi
    private float nextNormalAttackTime = 0f;

    // --- 2. SLASH (ÖZEL) SALDIRI AYARLARI (SAĞ TIK) ---
    public int slashDamage = 20;        // Özel saldırı hasarı
    public float specialAttackDuration = 0.5f;

    [Header("Knockback Settings")]
    public float upwardKnockbackStrength = 5f; 

    private float currentSpeed = 0.0f;
    private bool isGrounded;
    private bool isCrouching;
    private float resetTime = 0.2f;
    private float lastClickTime;
    
    // Durum Kontrolü
    private bool swordSlash;          // Sağ tık animasyonu için
    private bool isSpecialAttacking = false;
    
    public bool isBlocking = false; 
    private bool isDead = false;

    private bool isKnockedBack = false;

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
        
        // Saldırıları ayırdık:
        HandleNormalAttack();  // Sol Tık
        HandleSlashAttack();   // Sağ Tık

        UpdateAnimations();

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
        if (isKnockedBack) return;

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
        if (isKnockedBack) return; 

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
        if (isKnockedBack) return;

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

    // --- SOL TIK: NORMAL SALDIRI YÖNETİMİ ---
    private void HandleNormalAttack()
    {
        if (isKnockedBack) return;

        // Sol Tık (0)
        if (Input.GetMouseButtonDown(0) && !isSpecialAttacking && !isBlocking)
        {
            // Cooldown kontrolü (0.167 saniye geçti mi?)
            if (Time.time >= nextNormalAttackTime)
            {
                clickCount++;
                lastClickTime = Time.time;
                
                // Bir sonraki vuruş zamanını belirle
                nextNormalAttackTime = Time.time + normalAttackCooldown;

                // Normal saldırıyı gerçekleştir
                NormalAttack();
                
                // Eğer normal saldırı için bir animasyon trigger'ın varsa buraya ekleyebilirsin:
                // _animator.SetTrigger("NormalAttack");
            }
        }
    }

    // --- SAĞ TIK: SLASH (ÖZEL) SALDIRI YÖNETİMİ ---
    private void HandleSlashAttack()
    {
        if (isKnockedBack) return;

        // Sağ Tık (1)
        if (Input.GetMouseButtonDown(1) && !isSpecialAttacking && !isBlocking)
        {
            StartCoroutine(SlashAttackRoutine());
        }
    }

    IEnumerator SlashAttackRoutine()
    {
        isSpecialAttacking = true; 
        swordSlash = true; // Animasyon için bool

        // Özel saldırı fonksiyonunu çağır
        SlashAttack(); 

        yield return new WaitForSeconds(specialAttackDuration);

        swordSlash = false;        
        isSpecialAttacking = false; 
    }

    // ==========================================================
    // --- 1. NORMAL SALDIRI FONKSİYONU (SOL TIK İÇİN) ---
    // ==========================================================
    public void NormalAttack()
    {
        if (attackPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach(Collider2D enemy in hitEnemies)
        {
            // Şövalye Kontrolü (Savunma yapıyor mu?)
            enemy_knight_movement knight = enemy.GetComponent<enemy_knight_movement>();
            if (knight != null)
            {
                knight.ReceiveDamage(normalDamage); // Normal hasar miktarını gönder
                continue; 
            }

            // Diğer Düşmanlar
            health_system enemyHealth = enemy.GetComponent<health_system>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(normalDamage, transform);
            }
        }
    }

    // ==========================================================
    // --- 2. SLASH SALDIRI FONKSİYONU (SAĞ TIK İÇİN) ---
    // ==========================================================
    public void SlashAttack()
    {
        if (attackPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach(Collider2D enemy in hitEnemies)
        {
            // Şövalye Kontrolü
            enemy_knight_movement knight = enemy.GetComponent<enemy_knight_movement>();
            if (knight != null)
            {
                knight.ReceiveDamage(slashDamage); // Yüksek hasar miktarını gönder
                continue; 
            }

            // Diğer Düşmanlar
            health_system enemyHealth = enemy.GetComponent<health_system>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(slashDamage, transform);
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
        _animator.SetBool("swordSlash", swordSlash); // Sağ tık animasyonu
        _animator.SetBool("isBlocking", isBlocking);
        
        _animator.SetBool("isHurt", isKnockedBack);
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

    public void ApplyKnockback(Vector2 direction, float force)
    {
        isKnockedBack = true;
        _rigidbody2D.linearVelocity = Vector2.zero; 
        
        float pushDirX = (direction.x >= 0) ? 1f : -1f;
        Vector2 knockbackVector = new Vector2(pushDirX * force, upwardKnockbackStrength);

        _rigidbody2D.AddForce(knockbackVector, ForceMode2D.Impulse);
        StartCoroutine(ResetKnockbackRoutine());
    }

    IEnumerator ResetKnockbackRoutine()
    {
        yield return new WaitForSeconds(0.2f);
        isKnockedBack = false;
    }
}