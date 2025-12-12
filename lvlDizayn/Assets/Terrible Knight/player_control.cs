using UnityEngine;

public class Character_Control : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 5.0f; 
    public float downForceMultiplier = 2.5f; 

    [Header("Attack Settings")]
    public float attackRange = 0.5f;   // Saldırının ne kadar uzağa ulaşacağı
    public int lightAttackDamage = 10; // Normal kombo vuruş hasarı
    public int heavyAttackDamage = 25; // Özel saldırı (swordSlash) hasarı
    public Transform attackPoint;      // Saldırının başlayacağı nokta
    public LayerMask enemyLayer;       // Sadece düşmanları algılaması için LayerMask
    public int clickCount = 0;
    public float resetTime = 0.2f;

    [Header("Ground Check Settings")]
    public Transform groundCheck;   
    public LayerMask groundLayer; 
    public float groundCheckRadius = 0.05f; 

    // Durum Değişkenleri
    private float currentSpeed = 0.0f;
    private bool isGrounded;
    private bool isCrouching;
    private float lastClickTime;
    private bool swordSlash;

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

        if (groundCheck == null)
            Debug.LogError("GroundCheck objesi atanmamış! Lütfen Inspector'dan atayın.");
        if (attackPoint == null)
            Debug.LogError("AttackPoint objesi atanmamış! Lütfen Inspector'dan atayın.");
    }

    void Update()
    {
        HandleMovementInput(); 
        HandleJumpInput();
        HandleCrouchInput();
        HandleAttackInput();
        UpdateAnimations();

        // Tıklama sayacı sıfırlama
        if (Time.time - lastClickTime >= resetTime)
        {
            clickCount = 0;
        }
    }
    
    // FixedUpdate, fizik işlemleri için kullanılır.
    void FixedUpdate()
    {
        // YER KONTROLÜ
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // FİZİKSEL HAREKET
        ApplyMovementPhysics();

        // DOWNFORCE (Ek Aşağı Kuvvet)
        ApplyDownforce();
    }


    private void HandleMovementInput()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        // Saldırı, yuvarlanma veya özel saldırı yapılmıyorsa hareket et
        if (!Input.GetMouseButton(1) && !Input.GetKey(KeyCode.S) && !Input.GetMouseButton(0))
        {
            currentSpeed = moveInput * moveSpeed;

            if (moveInput > 0)
                _spriteRenderer.flipX = false;
            else if (moveInput < 0)
                _spriteRenderer.flipX = true;
        }
        else
        {
            currentSpeed = 0.0f;
        }
    }
    
    // Yatay hareket kuvvetini Rigidbody'ye uygula
    private void ApplyMovementPhysics()
    {
        // Unity'de hız ataması (velocity)
        _rigidbody2D.linearVelocity = new Vector2(currentSpeed, _rigidbody2D.linearVelocity.y);
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !Input.GetMouseButton(1) && !Input.GetKey(KeyCode.S))
        {
            // Zıplama
            _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocity.x, jumpForce);
            isGrounded = false; 
        }
    }

    private void HandleCrouchInput()
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

    private void HandleAttackInput()
    {
        // Normal Saldırı
        if (Input.GetMouseButtonDown(0))
        {
            clickCount++;
            lastClickTime = Time.time;
        }
        
        // Özel Saldırı
        swordSlash = Input.GetMouseButton(1);
    }
    
    private void ApplyDownforce()
    {
        // Yerde değilsek (havadaysak) ve dikey hızımız düşmeye başlamışsa
        if (!isGrounded && _rigidbody2D.linearVelocity.y < 0.5f)
        {
            // Rigidbody'ye aşağı yönde ek kuvvet uygula.
            _rigidbody2D.AddForce(Vector2.down * downForceMultiplier, ForceMode2D.Force);
        }
    }

    // --- ÖNEMLİ: HASAR VERME FONKSİYONU ---
    // Bu, saldırı animasyonunun tam isabet karesinde çağrılacaktır (Animation Event).
    public void PerformAttack()
    {
        if (attackPoint == null) return;

        int currentDamage = 0;

        // Hasar miktarını belirle
        if (swordSlash)
        {
            currentDamage = heavyAttackDamage;
        }
        else if (clickCount > 0)
        {
            currentDamage = lightAttackDamage;
        }
        
        if (currentDamage == 0) return;

        // 1. Saldırı menzilindeki düşmanları algıla
        // HATA DÜZELTİLDİ: OverlapCircle yerine OverlapCircleAll kullanıldı.
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        // 2. Algılanan her düşmana hasar ver
        foreach (Collider2D enemy in hitEnemies)
        {
            // Düşmanın EnemyHealth scriptini al
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            
            if (enemyHealth == null)
            {
                // Eğer EnemyHealth scripti objede değilse, Parent objede olup olmadığını kontrol et
                enemyHealth = enemy.GetComponentInParent<EnemyHealth>();
            }

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(currentDamage);
                Debug.Log("Vurulan Düşman: " + enemy.name + " Hasar: " + currentDamage);
            }
        }
    }

    // Animasyonları güncelleme
    private void UpdateAnimations()
    {
        _animator.SetFloat("speed", Mathf.Abs(currentSpeed));
        _animator.SetBool("isJumping", !isGrounded);
        _animator.SetBool("isCrouching", isCrouching);
        _animator.SetInteger("clickCount", clickCount);
        _animator.SetBool("isGrounded", isGrounded);
        _animator.SetBool("swordSlash", swordSlash);
    }

    // Editörde yer ve saldırı kontrol çemberlerini görebilmek için
    void OnDrawGizmosSelected()
    {
        // Yer Kontrol Alanı
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        // Saldırı Alanı
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}