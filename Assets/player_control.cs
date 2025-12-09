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

    [Header("Combat Settings (YENİ)")]
    public Transform attackPoint; // Kılıcın vuruş noktası
    public float attackRange = 0.8f; // Kılıcın menzili
    public LayerMask enemyLayers; // Hangi katman düşman?
    public int slashDamage = 20; // Vurulacak hasar miktarı

    private float currentSpeed = 0.0f;
    private bool isGrounded;
    private bool isCrouching;
    private float resetTime = 0.2f;
    private float lastClickTime;
    private bool swordSlash;

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

        // YER KONTROLÜ
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        HandleMovement();
        HandleJump();
        HandleCrouch();
        HandleAttack(); // Sol tık
        UpdateAnimations();
        HandleSpecialAttack(); // Sağ tık (Special)

        if (Time.time - lastClickTime >= resetTime)
        {
            clickCount = 0;
        }
    }

    public void TriggerDeath()
    {
        if (isDead) return; 

        isDead = true; 
        
        _rigidbody2D.velocity = Vector2.zero; 
        currentSpeed = 0;
        
        _animator.SetBool("isDead", true);
    }

private void HandleMovement()
    {
        // SAĞA GİTME
        if (Input.GetKey(KeyCode.D) && !Input.GetMouseButton(1) && !Input.GetKey(KeyCode.S) && !Input.GetMouseButton(0))
        {
            currentSpeed = moveSpeed;
            
            // _spriteRenderer.flipX = false; // BU SATIRI İPTAL EDİYORUZ
            
            // Karakterin yönünü fiziksel olarak sağa (0 derece) çevir
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        // SOLA GİTME
        else if (Input.GetKey(KeyCode.A) && !Input.GetMouseButton(1) && !Input.GetKey(KeyCode.S) && !Input.GetMouseButton(0))
        {
            currentSpeed = -moveSpeed;
            
            // _spriteRenderer.flipX = true; // BU SATIRI İPTAL EDİYORUZ

            // Karakterin yönünü fiziksel olarak sola (180 derece) çevir
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        // DURMA
        else
        {
            currentSpeed = 0.0f;
        }

        _rigidbody2D.velocity = new Vector2(currentSpeed, _rigidbody2D.velocity.y);
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !Input.GetMouseButton(1) && !Input.GetKey(KeyCode.S))
        {
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, jumpForce);
        }
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.S) && isGrounded)
        {
            isCrouching = true;
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            isCrouching = false;
        }
    }

    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickCount++;
            lastClickTime = Time.time;
        }
    }

    private void HandleSpecialAttack()
    {
        if (Input.GetMouseButtonDown(1))
        {
            swordSlash = true;
            // Saldırı animasyonu başladığında hasarı hesapla
            SlashAttack(); 
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            swordSlash = false;
        }
    }

    // YENİ: Hasar Verme Fonksiyonu
    void SlashAttack()
    {
        if (attackPoint == null) return;

        // 1. attackPoint etrafındaki düşmanları tespit et
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // 2. Bulunan her düşmana hasar bilgisini gönder
        foreach(Collider2D enemy in hitEnemies)
        {
            // Düşman scriptini bul
            enemy_knight_movement enemyScript = enemy.GetComponent<enemy_knight_movement>();
            
            if(enemyScript != null)
            {
                // Düşmanın kendi hasar alma fonksiyonunu çağır (Defans kontrolü orada yapılacak)
                enemyScript.ReceiveDamage(slashDamage);
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
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        // YENİ: Editörde saldırı menzilini görmek için kırmızı çember
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}