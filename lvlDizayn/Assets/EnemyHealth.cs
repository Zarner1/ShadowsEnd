using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Can Ayarları")]
    public int maxHealth = 50; // Düşmanın maksimum canı
    private int currentHealth;  // Mevcut can

    [Header("Ölüm Ayarları")]
    [Tooltip("Ölüm animasyonu ne kadar sürede biter? Bu süre sonunda düşman yok edilir.")]
    public float deathDuration = 1.5f; 
    
    private Animator anim;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Mermi veya oyuncu saldırısı tarafından çağrılacak metot
    public void TakeDamage(int damageAmount)
    {
        // Canı azalt
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " hasar aldı! Kalan Can: " + currentHealth);

        // Can sıfıra veya altına düştüyse öl
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Ölüm animasyonunu tetikle
        if (anim != null)
        {
            // Animator'da "Die" adında bir Trigger (Tetikleyici) parametresi olmalıdır.
            anim.SetTrigger("isDie");
        }
        
        // Düşmanı etkisiz hale getir
        // 1. Düşmanın hareketini durdur
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // 2. Düşmanın çarpıştırıcısını kapat (Oyuncu artık çarpmasın/vurmasın)
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // 3. Düşmanın AI scriptini kapat (Artık takip etmesin/saldırmasın)
        EnemyAI aiScript = GetComponent<EnemyAI>();
        if (aiScript != null)
        {
            aiScript.enabled = false;
        }

        // Düşmanı, ölüm animasyonu süresi bittikten sonra sahneden tamamen kaldır.
        Destroy(gameObject, deathDuration);
    }
}