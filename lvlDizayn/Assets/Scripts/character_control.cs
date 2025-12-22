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
    public LayerMask enemyLayers; // Unity'den "Enemy" layer'ını seçmeyi unutma!
    public int slashDamage = 20; 
    public float specialAttackDuration = 0.5f;

    [Header("Knockback Settings")]
    public float upwardKnockbackStrength = 5f; // Oyuncuyu havaya kaldırma gücü

    private float currentSpeed = 0.0f;
    private bool isGrounded;
    private bool isCrouching;
    private float resetTime = 0.2f;
    private float lastClickTime;
    
    // Durum Kontrolü
    private bool swordSlash;
    private bool isSpecialAttacking = false;
    
    public bool isBlocking = false; 
    private bool isDead = false;

    // Savruluyor mu?
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

        // Durumları yönet
        HandleBlocking(); 
        HandleMovement();
        HandleJump();
        HandleCrouch();
        HandleAttack();
        HandleSpecialAttack();

        // Animasyonları güncelle
        UpdateAnimations();

        // Tıklama sayacını sıfırla (Combo sistemi için)
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
        
        // Fizik etkileşimlerini kapatmak istersen:
        // _rigidbody2D.bodyType = RigidbodyType2D.Static;
    }

    private void HandleBlocking()
    {
        // Savrulurken blok açıp kapatamayalım
        if (isKnockedBack) return;

        if (isGrounded && !isSpecialAttacking)
        {
            // Shift tuşuna basılı tutunca blok yap
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                isBlocking = true;
                currentSpeed = 0; // Blok yaparken hareket edemesin
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
        // Savruluyorsak kontrol bizde değil
        if (isKnockedBack) return; 

        // Hareket tuşlarına basılıyor mu? (Saldırı ve blok yoksa)
        if (Input.GetKey(KeyCode.D) && !isSpecialAttacking && !isBlocking && !Input.GetKey(KeyCode.S) && !Input.GetMouseButton(0))
        {
            currentSpeed = moveSpeed;
            transform.rotation = Quaternion.Euler(0, 0, 0); // Sağa dön
        }
        else if (Input.GetKey(KeyCode.A) && !isSpecialAttacking && !isBlocking && !Input.GetKey(KeyCode.S) && !Input.GetMouseButton(0))
        {
            currentSpeed = -moveSpeed;
            transform.rotation = Quaternion.Euler(0, 180, 0); // Sola dön
        }
        else
        {
            currentSpeed = 0.0f;
        }
        
        // Hızı uygula (Y ekseni hızını koruyarak)
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

    private void HandleAttack()
    {
        if (isKnockedBack) return;

        // Sol tık saldırısı
        if (Input.GetMouseButtonDown(0) && !isSpecialAttacking && !isBlocking)
        {
            clickCount++;
            lastClickTime = Time.time;
            
            // Animasyon Event'i kullanmıyorsan saldırıyı buradan tetikleyebilirsin
            // Ancak genelde animasyonun belirli karesinde SlashAttack() çağrılır.
            // Şimdilik animasyondan çağrıldığını varsayıyoruz.
        }
    }

    private void HandleSpecialAttack()
    {
        if (isKnockedBack) return;

        // Sağ tık özel saldırı
        if (Input.GetMouseButtonDown(1) && !isSpecialAttacking && !isBlocking)
        {
            StartCoroutine(SpecialAttackRoutine());
        }
    }

    IEnumerator SpecialAttackRoutine()
    {
        isSpecialAttacking = true; 
        swordSlash = true;        

        // Özel saldırıda da hasar ver
        SlashAttack(); 

        yield return new WaitForSeconds(specialAttackDuration);

        swordSlash = false;        
        isSpecialAttacking = false; 
    }

    // --- HASAR VERME FONKSİYONU ---
    // Bu fonksiyonu Animation Event ile çağırmanı tavsiye ederim.
// --- HASAR VERME FONKSİYONU (GÜNCELLENDİ) ---
    public void SlashAttack()
    {
        if (attackPoint == null) return;

        // Çember içindeki düşmanları bul
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach(Collider2D enemy in hitEnemies)
        {
            // --- 1. ÖNCELİK: ŞÖVALYE KONTROLÜ ---
            // Eğer vurduğumuz şey bir "Knight" ise, onun kendi scriptini çağır.
            // Çünkü bloklama mantığı orada yazıyor.
            enemy_knight_movement knight = enemy.GetComponent<enemy_knight_movement>();
            if (knight != null)
            {
                // Şövalyenin kendi hasar alma fonksiyonunu çağırıyoruz.
                // O fonksiyon içeride "defend" (blok) durumunu kontrol edecek.
                knight.ReceiveDamage(slashDamage);
                
                // Şövalye bulunduğuna göre döngünün bu turunu bitir (Aşağıdaki health_system'e tekrar vurmasın)
                continue; 
            }

            // --- 2. ÖNCELİK: KRAL (BOSS) KONTROLÜ ---
            /* Eğer Kral'ın da özel bir savunma mekaniği varsa burayı açabilirsin
            enemy_king_movement king = enemy.GetComponent<enemy_king_movement>();
            if (king != null)
            {
                // king.TakeDamage(slashDamage);
                // continue;
            }
            */

            // --- 3. ÖNCELİK: STANDART DÜŞMANLAR (İskelet, Büyücü, vs.) ---
            // Eğer özel bir scripti yoksa (veya blok mekaniği yoksa) direkt canına vur.
            health_system enemyHealth = enemy.GetComponent<health_system>();

            if (enemyHealth != null)
            {
                // Hasar ver (Saldırgan olarak kendimizi 'transform' gönderiyoruz)
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
        _animator.SetBool("swordSlash", swordSlash);
        _animator.SetBool("isBlocking", isBlocking);
        
        // Savrulma animasyonu varsa ekle:
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

    // --- KNOCKBACK (SAVRULMA) FONKSİYONU ---
    public void ApplyKnockback(Vector2 direction, float force)
    {
        // Kontrolü kilitle
        isKnockedBack = true;

        // Mevcut hızı sıfırla (Uçup gitmemek için)
        _rigidbody2D.linearVelocity = Vector2.zero; 
        
        // Yönü belirle (Sadece X ekseni: Sağa mı Sola mı?)
        float pushDirX = (direction.x >= 0) ? 1f : -1f;

        // Çapraz Vektör: Geriye it + Yukarı kaldır
        Vector2 knockbackVector = new Vector2(pushDirX * force, upwardKnockbackStrength);

        // Gücü uygula
        _rigidbody2D.AddForce(knockbackVector, ForceMode2D.Impulse);

        // Kontrolü geri vermek için bekle
        StartCoroutine(ResetKnockbackRoutine());
    }

    IEnumerator ResetKnockbackRoutine()
    {
        // 0.2 saniye boyunca oyuncu kontrolü kaybeder (Havada süzülür)
        yield return new WaitForSeconds(0.2f);
        isKnockedBack = false;
    }
}