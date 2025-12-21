using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class health_system : MonoBehaviour
{
    [Header("UI Bağlantıları")]
    public TMP_Text healthText;     
    public Image healthBarImage;     
    public Gradient healthGradient;  

    [Header("Can Değerleri")]
    public float maxHealth = 100f;
    public float currentHealth;

    // Karakter kontrol scriptine referans (Sadece oyuncu için)
    private Character_Control charControl;

    void Start()
    {
        currentHealth = maxHealth;
        charControl = GetComponent<Character_Control>(); 
        UpdateHealthUI();
    }

    // Hasar Alma Fonksiyonu
    public void TakeDamage(float amount, Transform attacker = null)
    {
        // 1. OYUNCU İÇİN BLOK KONTROLÜ
        // Eğer hasar alan şey oyuncuysa ve blok yapıyorsa:
        if (charControl != null && charControl.isBlocking && attacker != null)
        {
            // Düşman önde mi diye bak
            Vector2 directionToAttacker = (attacker.position - transform.position).normalized;
            float dotProduct = Vector2.Dot(transform.right, directionToAttacker);

            if (dotProduct > 0)
            {
                Debug.Log("🛡️ Hasar Bloklandı!");
                return; // Hasarı iptal et
            }
        }

        // Hasarı Uygula
        currentHealth -= amount;

        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        // CAN 0 OLDUYSA ÖLDÜR
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
        // Sadece UI elemanları atanmışsa çalışsın (Düşmanlarda UI olmayabilir)
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

    // --- ÖLÜM YÖNETİMİ (BURASI GÜNCELLENDİ) ---
    void Die()
    {
        // 1. Ölen şey OYUNCU mu?
        if (charControl != null)
        {
            Debug.Log("Oyuncu Öldü!");
            charControl.TriggerDeath();
            return;
        }

        // 2. Ölen şey BOSS (KRAL) mı? (YENİ EKLENDİ)
        enemy_king_movement king = GetComponent<enemy_king_movement>();
        if (king != null)
        {
            Debug.Log("Kral Öldü!");
            king.TriggerDeath();
            return;
        }

        // 3. Ölen şey DEV İSKELET mi?
        enemy_giant_skeleton_movement giantSkeleton = GetComponent<enemy_giant_skeleton_movement>();
        if (giantSkeleton != null)
        {
            giantSkeleton.TriggerDeath();
            return;
        }

        // 4. Ölen şey BÜYÜCÜ (WIZARD) mi?
        enemy_wizard_movement wizard = GetComponent<enemy_wizard_movement>();
        if (wizard != null)
        {
            wizard.TriggerDeath();
            return;
        }

        // 5. Ölen şey ŞÖVALYE mi?
        enemy_knight_movement knight = GetComponent<enemy_knight_movement>();
        if (knight != null)
        {
             knight.TriggerDeath();
             return;
        }
        
        // 6. Ölen şey OKÇU mu?
        Enemy_archer_movement archer = GetComponent<Enemy_archer_movement>();
        if (archer != null)
        {
            archer.TriggerDeath();
            return;
        }

        // 7. Hiçbiri değilse (Kutu, varil vb.) direkt yok et
        Destroy(gameObject);
    }
}