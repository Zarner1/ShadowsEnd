using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class player_health_system : MonoBehaviour
{
    [Header("UI Bağlantıları")]
    public TMP_Text healthText;     
    public Image healthBarImage;     
    public Gradient healthGradient;  

    [Header("Can Değerleri")]
    public float maxHealth = 100f;
    public float currentHealth;

    // Karakter kontrol scriptine referans
    private Character_Control charControl;

    void Start()
    {
        currentHealth = maxHealth;
        charControl = GetComponent<Character_Control>(); 
        UpdateHealthUI();
    }

    // --- BURASI GÜNCELLENDİ ---
    // Artık 3. parametre olarak 'knockbackForce' alıyor. 
    // Varsayılan değeri 0 olduğu için eski düşmanlar (okçu, iskelet vb.) hata vermeden çalışmaya devam eder.
    public void TakeDamage(float amount, Transform attacker = null, float knockbackForce = 0f)
    {
        bool isBlockingSuccess = false;

        // 1. BLOK KONTROLÜ
        // Eğer oyuncu blok yapıyorsa ve saldıran belli ise
        if (charControl != null && charControl.isBlocking && attacker != null)
        {
            // Düşmanın yönü ile karakterin baktığı yönü kıyasla
            Vector2 directionToAttacker = (attacker.position - transform.position).normalized;
            float dotProduct = Vector2.Dot(transform.right, directionToAttacker);

            // Eğer düşman karakterin önündeyse blok başarılıdır
            if (dotProduct > 0)
            {
                isBlockingSuccess = true;
            }
        }

        // --- SENARYO A: BLOK BAŞARILI ---
        if (isBlockingSuccess)
        {
            Debug.Log("🛡️ Hasar Bloklandı (Yarım Hasar)!");
            
            // YARIM HASAR AL
            currentHealth -= (amount / 2f);

            // Eğer itme gücü varsa NORMAL ŞİDDETTE uygula
            if (knockbackForce > 0)
            {
                Vector2 knockbackDir = (transform.position - attacker.position).normalized;
                charControl.ApplyKnockback(knockbackDir, knockbackForce);
            }
        }
        // --- SENARYO B: BLOK YOK ---
        else
        {
            // TAM HASAR AL
            currentHealth -= amount;

            // Eğer itme gücü varsa 2 KAT ŞİDDETLE uygula (Ceza)
            if (knockbackForce > 0 && attacker != null)
            {
                Vector2 knockbackDir = (transform.position - attacker.position).normalized;
                charControl.ApplyKnockback(knockbackDir, knockbackForce * 2f);
            }
        }

        // Canın eksiye düşmesini engelle
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (currentHealth == 0)
        {
            Die(); 
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth.ToString("F0")}/{maxHealth}";
        }

        if (healthBarImage != null)
        {
            float healthPercentage = currentHealth / maxHealth;
            healthBarImage.fillAmount = healthPercentage;
            healthBarImage.color = healthGradient.Evaluate(healthPercentage);
        }
    }

    void Die()
    {
        Debug.Log("Oyuncu Öldü!");
        if (charControl != null)
        {
            charControl.TriggerDeath();
        }
    }
}