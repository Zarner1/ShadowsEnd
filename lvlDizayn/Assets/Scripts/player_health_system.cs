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
        charControl = GetComponent<Character_Control>(); // Scripti buluyoruz
        UpdateHealthUI();
    }

    // GÜNCELLENMİŞ HASAR FONKSİYONU
    // attacker: Hasarı vuran düşmanın Transform'u (Opsiyonel)
    public void TakeDamage(float amount, Transform attacker = null)
    {
        // 1. Blok Kontrolü Yap
        if (charControl != null && charControl.isBlocking && attacker != null)
        {
            // Düşmanın yönü ile karakterin baktığı yönü kıyaslıyoruz.
            // Düşman nerede? (DüşmanPozisyonu - BenimPozisyonum)
            Vector2 directionToAttacker = (attacker.position - transform.position).normalized;

            // Dot Product (Nokta Çarpımı) kullanarak yön hesabı:
            // transform.right -> Karakterin şu an baktığı yön (Sağ veya Sol)
            // Eğer sonuç > 0 ise düşman karakterin ÖNÜNDE demektir.
            float dotProduct = Vector2.Dot(transform.right, directionToAttacker);

            if (dotProduct > 0)
            {
                // Düşman önde ve blok yapıyoruz -> HASARI ENGELLE
                Debug.Log("🛡️ Hasar Bloklandı!");
                
                // İstersen burada bloklama sesi veya efekti çalabilirsin.
                return; // Fonksiyondan çık, can düşmesin.
            }
        }

        // Bloklanmadıysa normal hasar işlemine devam et
        currentHealth -= amount;

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
        float healthPercentage = currentHealth / maxHealth;

        if (healthText != null)
        {
            healthText.text = $"{currentHealth.ToString("F0")}/{maxHealth}";
        }

        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = healthPercentage;
            healthBarImage.color = healthGradient.Evaluate(healthPercentage);
        }
    }

    void Die()
    {
        Debug.Log("Oyuncu Öldü!");
        GetComponent<Character_Control>().TriggerDeath();
    }
}