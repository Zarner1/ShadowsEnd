using UnityEngine;

public class Character_Control : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 5.0f; 
    public float downForceMultiplier = 2.5f; // EKLENDİ: Ek aşağı kuvvet çarpanı

    [Header("Attack Settings")]
    public int clickCount = 0;
    public float resetTime = 0.2f;

    [Header("Ground Check Settings")]
    public Transform groundCheck; 
    public LayerMask groundLayer; 
    public float groundCheckRadius = 0.05f; // Yarıçapı düşman algılamamak için küçültüldü

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
        // Null kontrolü ile başlatma (hata olasılığını azaltır)
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        lastClickTime = Time.time;

        // Bileşenlerin atanıp atanmadığını kontrol et
        if (groundCheck == null)
            Debug.LogError("GroundCheck objesi atanmamış! Lütfen Inspector'dan atayın.");
    }

    void Update()
    {
        // Sadece giriş (input) ve animasyon güncellemeleri burada kalır

        // --- YATAY HAREKET İÇİN HIZI BELİRLE ---
        HandleMovementInput(); 

        // --- ZIPLAMA ---
        HandleJumpInput();

        // --- YUVARLANMA ---
        HandleCrouchInput();

        // --- SALDIRI VE ÖZEL SALDIRI ---
        HandleAttackInput();

        // --- ANİMASYON ---
        UpdateAnimations();

        // Tıklama sayacı sıfırlama
        if (Time.time - lastClickTime >= resetTime)
        {
            clickCount = 0;
        }
    }
    
    // FixedUpdate, fizik işlemleri (Rigidbody) için kullanılır.
    void FixedUpdate()
    {
        // YER KONTROLÜ: isGrounded değerini fizik döngüsünde kontrol et
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // --- FİZİKSEL HAREKET ---
        ApplyMovementPhysics();

        // --- DOWNFORCE (Ek Aşağı Kuvvet) ---
        ApplyDownforce();
    }


    // Yatay hareket için hız belirleme (Input)
    private void HandleMovementInput()
    {
        float moveInput = Input.GetAxisRaw("Horizontal"); // A ve D tuşlarından gelen değeri al

        // Eğer saldırı veya yuvarlanma yapılmıyorsa hareket et
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
    
    // Yatay hareket kuvvetini Rigidbody'ye uygula (Physics)
    private void ApplyMovementPhysics()
    {
        _rigidbody2D.linearVelocity = new Vector2(currentSpeed, _rigidbody2D.linearVelocity.y);
    }

    // Zıplama Girişi (Input)
    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !Input.GetMouseButton(1) && !Input.GetKey(KeyCode.S))
        {
            // Rigidbody'ye dikey bir kuvvet uygula
            _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocity.x, jumpForce);
            
            // Çift zıplamayı önlemek için anında yerde değilmiş gibi işaretle
            isGrounded = false; 
        }
    }

    // Yuvarlanma Girişi (Input)
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

    // Saldırı Girişleri (Input)
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
    
    // Downforce (Ek Aşağı Kuvvet) Uygulama
  private void ApplyDownforce()
{
    // Yerde değilsek (havadaysak)
    if (!isGrounded)
    {
        // YENİ KONTROL: Karakterin dikey hızı 0.5f'ten küçükse (yani zirveye çok yakınsa veya iniyorsa)
        if (_rigidbody2D.linearVelocity.y < 0.5f) // Örneğin 0.5f kullanın
        {
            // Rigidbody'ye aşağı yönde ek kuvvet uygula.
            _rigidbody2D.AddForce(Vector2.down * downForceMultiplier, ForceMode2D.Force);
        }
    }
}

    // Animasyonları güncelleme
    private void UpdateAnimations()
    {
        _animator.SetFloat("speed", Mathf.Abs(currentSpeed));
        _animator.SetBool("isJumping", !isGrounded); // Zıplama (veya havada olma)
        _animator.SetBool("isCrouching", isCrouching);
        _animator.SetInteger("clickCount", clickCount);
        _animator.SetBool("isGrounded", isGrounded);
        _animator.SetBool("swordSlash", swordSlash);
    }

    // Editörde yer kontrol çemberini görebilmek için
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}