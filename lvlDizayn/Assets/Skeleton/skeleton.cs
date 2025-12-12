using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Hedef ve Hız")]
    public Transform target;          // Takip edilecek hedef (Player)
    public float moveSpeed = 3f;      // Hareket hızı
    [Tooltip("Sprite'ın varsayılan olarak baktığı yön. Sağa bakıyorsa 1, Sola bakıyorsa -1.")]
    public float defaultFacingDirection = -1f; 

    [Header("Menzil Ayarları")]
    public float followRange = 8f;    // Oyuncuyu takip etmeye başlayacağı genel menzil (X)
    public float attackRange = 1.5f;  // Vurmaya başlayacağı yatay mesafe
    [Tooltip("Oyuncu ile düşman arasındaki kabul edilebilir maksimum dikey fark.")]
    public float verticalTolerance = 0.1f; // Y ekseninde maksimum fark (Takip ve Saldırı için)
    
    [Header("Saldırı Hızı ve Hasar")] 
    public float attackCooldown = 2f; 
    public int attackDamage = 10;     
    private float timeSinceLastAttack; 

    [Header("Bileşenler")]
    private Animator anim;
    private Rigidbody2D rb;
    private Vector3 initialScale; 

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale; 
        timeSinceLastAttack = attackCooldown; 

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void Update()
{
    if (target == null) return;
    
    timeSinceLastAttack += Time.deltaTime;

    float horizontalDistance = Mathf.Abs(target.position.x - transform.position.x);
    float verticalDifference = Mathf.Abs(target.position.y - transform.position.y);

    // Y eksenindeki fark toleransın dışındaysa, Idle yap ve çık (takip etme)
    // Eğer karakter yerdeyse (Y=0) ve düşman da yerdeyse (Y=0), verticalDifference = 0 olur.
    // Bu yüzden bu kontrol, SADECE oyuncu zıpladığında çalışır.
    if (verticalDifference > verticalTolerance)
    {
        Idle();
        return;
    }

    // --- BURADA HAREKET MANTIĞI DEVAM EDİYOR ---
    
    // 1. GENEL MENZİL KONTROLÜ
    if (horizontalDistance <= followRange) 
    {
        // 2. SALDIRI MENZİLİ KONTROLÜ
        if (horizontalDistance <= attackRange)
        {
            TryAttack();
        }
        // 3. KOVALAMA MENZİLİ
        else
        {
            FollowTarget();
        }
    }
    else
    {
        // Menzil dışında (Idle)
        Idle();
    }
}

    void FollowTarget()
    {
        float direction = Mathf.Sign(target.position.x - transform.position.x);
        
        // Hareketi gerçekleştir (FİZİK HATASI GİDERİLDİ: rb.velocity)
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y); 

        // Animasyon ve Yön Çevirme
        anim.SetBool("IsRunning", true); 
        anim.SetBool("IsAttacking", false);
        
        float finalFlip = defaultFacingDirection * direction;
        float targetScaleX_Reversed = initialScale.x * -finalFlip;
        transform.localScale = new Vector3(targetScaleX_Reversed, initialScale.y, initialScale.z);
    }

    void TryAttack()
    {
        // Durma (FİZİK HATASI GİDERİLDİ: rb.velocity)
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        
        // Yön Çevirme
        float direction = Mathf.Sign(target.position.x - transform.position.x);
        float finalFlip = defaultFacingDirection * direction;
        float targetScaleX_Reversed = initialScale.x * -finalFlip;
        transform.localScale = new Vector3(targetScaleX_Reversed, initialScale.y, initialScale.z);
        
        // Cooldown Kontrolü
        bool isCurrentlyAttacking = anim.GetCurrentAnimatorStateInfo(0).IsName("skeleton_attack"); 
        
        if (timeSinceLastAttack >= attackCooldown)
        {
            Attack();
            timeSinceLastAttack = 0f; 
        }
        else if (isCurrentlyAttacking)
        {
             return;
        }
        else
        {
            anim.SetBool("IsAttacking", false);
            anim.SetBool("IsRunning", false);
        }
    }

    void Attack()
    {
        anim.SetBool("IsAttacking", true);
        anim.SetBool("IsRunning", false);
    }

    void Idle()
    {
        // Durma (FİZİK HATASI GİDERİLDİ: rb.velocity)
        rb.linearVelocity = Vector2.zero; 
        
        anim.SetBool("IsRunning", false);
        anim.SetBool("IsAttacking", false);
    }
    
    // ANIMASYON OLAYI İLE HASAR VERME
    public void HitTarget()
    {
        float horizontalDistance = Mathf.Abs(target.position.x - transform.position.x);
        float verticalDifference = Mathf.Abs(target.position.y - transform.position.y);
        
        // HASAR VERME ŞARTI: Dikey fark kabul edilebilir tolerans içinde olmalı.
        if (horizontalDistance <= attackRange + 0.1f && verticalDifference <= verticalTolerance) 
        {
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }
}