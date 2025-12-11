using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Hedef ve Hız")]
    public Transform target;          // Takip edilecek hedef (Player)
    public float moveSpeed = 3f;      // Hareket hızı
    [Tooltip("Sprite'ın varsayılan olarak baktığı yön. Sağa bakıyorsa 1, Sola bakıyorsa -1.")]
    public float defaultFacingDirection = -1f; 

    [Header("Menzil Ayarları")]
    public float followRange = 8f;    // Oyuncuyu takip etmeye başlayacağı menzil (X ekseninde)
    public float attackRange = 1.5f;  // Vurmaya başlayacağı menzil
    
    [Header("Saldırı Hızı Ayarları")]
    public float attackCooldown = 2f; // Saniyede kaç defa vurabileceği (2 saniyede bir vurur)
    private float timeSinceLastAttack; // Son saldırıdan bu yana geçen süre

    [Header("Bileşenler")]
    private Animator anim;
    private Rigidbody2D rb;
    private Vector3 initialScale;     // Küçülme sorununu çözmek için varsayılan ölçek

    void Start()
    {
        // Gerekli bileşenleri al
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Varsayılan ölçeği kaydet (Küçülme sorununu çözer)
        initialScale = transform.localScale; 
        timeSinceLastAttack = attackCooldown; // Oyuna başlar başlamaz vurabilmesi için

        // Hedefi (Player'ı) otomatik bul (Eğer Player objesinin Tag'i "Player" ise)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    void Update()
    {
        if (target == null) return;
        
        // Saldırı soğuma süresini güncelle
        timeSinceLastAttack += Time.deltaTime;

        float distanceToTarget = Mathf.Abs(target.position.x - transform.position.x);

        if (distanceToTarget <= followRange)
        {
            if (distanceToTarget > attackRange)
            {
                FollowTarget();
            }
            else
            {
                // Saldırı menzilinde dur ve saldırı mekaniğini başlat
                TryAttack();
            }
        }
        else
        {
            Idle();
        }
    }

    void FollowTarget()
    {
        float direction = Mathf.Sign(target.position.x - transform.position.x);
        
        // Hareketi gerçekleştir
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y); 

        // Animasyonu Çalıştır
        anim.SetBool("IsRunning", true); 
        anim.SetBool("IsAttacking", false);
        
        // Yön Çevirme (initialScale ve defaultFacingDirection kullanarak)
        float finalFlip = defaultFacingDirection * direction;
        
        // Yönü ters çevirme sorununu çözmek için * -finalFlip kullanıldı.
        float targetScaleX_Reversed = initialScale.x * -finalFlip;
        
        transform.localScale = new Vector3(targetScaleX_Reversed, initialScale.y, initialScale.z);
    }

   void TryAttack()
{
    // Saldırı menzilinde iken her zaman:
    
    // 1. Durma ve Yön Çevirme (Cooldown'dayken de hedefe baksın)
    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    
    float direction = Mathf.Sign(target.position.x - transform.position.x);
    float finalFlip = defaultFacingDirection * direction;
    float targetScaleX_Reversed = initialScale.x * -finalFlip;
    transform.localScale = new Vector3(targetScaleX_Reversed, initialScale.y, initialScale.z);
    
    // Animatörde şu anda Attack animasyonu oynuyor mu?
    // NOT: isAttacking yerine bu metodu kullanmak daha kesindir:
    bool isCurrentlyAttacking = anim.GetCurrentAnimatorStateInfo(0).IsName("skeleton_attack"); 
    // "YourAttackStateName" yerine Attack animasyonunuzun adını yazmalısınız.
    
    // Eğer şu anda saldırı animasyonu oynuyorsa, hiçbir şey yapma (Animasyonun bitmesini bekle)
    // Ve cooldown henüz dolmadıysa:
    if (isCurrentlyAttacking || timeSinceLastAttack < attackCooldown)
    {
        // Saldırı animasyonu bitene veya cooldown dolana kadar sadece Idle'ı göster
        anim.SetBool("IsAttacking", false);
        anim.SetBool("IsRunning", false);
        return; // Fonksiyondan çık
    }
    
    // Eğer buraya ulaşıldıysa: Cooldown dolmuştur ve animasyon bitmiştir.
    
    Attack(); // Saldırıyı Tetikle
    timeSinceLastAttack = 0f; // Zamanlayıcıyı sıfırla
}
    void Attack()
    {
        // Saldırı Animasyonunu Başlatma
        anim.SetBool("IsAttacking", true);
        anim.SetBool("IsRunning", false);
    }

    void Idle()
    {
        // Tamamen dur
        rb.linearVelocity = Vector2.zero; 
        
        // Idle Animasyonunu Çalıştır
        anim.SetBool("IsRunning", false);
        anim.SetBool("IsAttacking", false);
    }
}