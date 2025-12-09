using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro kütüphanesi

public class player_health_system : MonoBehaviour
{
    [Header("UI Bağlantıları")]
    public TMP_Text healthText;      // Can yazısı (100/100)
    public Image healthBarImage;     // Can barı görseli
    public Gradient healthGradient;  // Renk geçişi (Yeşil -> Kırmızı)

    [Header("Can Değerleri")]
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        // Oyun başında canı fulle
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    // Hasar alma fonksiyonu
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        // Can 0'ın altına düşmesin
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (currentHealth == 0)
        {
            Die(); // Ölüm fonksiyonunu çağır
        }
    }

    // İyileşme fonksiyonu
    public void Heal(float amount)
    {
        currentHealth += amount;

        // Can maksimumu geçmesin
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateHealthUI();
    }

    // UI Güncelleme Fonksiyonu
    void UpdateHealthUI()
    {
        float healthPercentage = currentHealth / maxHealth;

        // Eğer Inspector'da atama yapılmadıysa hata vermemesi için kontrol ekliyoruz:
        if (healthText != null)
        {
            healthText.text = $"{currentHealth.ToString("F0")}/{maxHealth}";
        }

        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = healthPercentage;
            // Gradient rengini ayarla
            healthBarImage.color = healthGradient.Evaluate(healthPercentage);
        }
    }

    // Oyuncu öldüğünde ne olacağını buraya yazacağız
void Die()
    {
        Debug.Log("Oyuncu Öldü!");
        
        // Karakter kontrol scriptine ulaş ve ölümü tetikle
        GetComponent<Character_Control>().TriggerDeath();
    }
}