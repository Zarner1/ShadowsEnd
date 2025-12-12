using UnityEngine;
using UnityEngine.UI; // Can çubuğu (Image) bileşenini kullanabilmek için zorunlu!

public class PlayerHealth : MonoBehaviour
{
    [Header("Can Ayarları")]
    public int maxHealth = 100; // Maksimum can değeri
    private int currentHealth;  // Mevcut can değeri

    // YENİ EKLENEN KISIM: UI Bağlantısı
    [Header("UI Bağlantısı")]
    [Tooltip("Inspector'dan HealthFill (yeşil doluluk) Image objesini buraya sürükleyin.")]
    public Image healthBarFill; // Can çubuğunun doluluk alanını temsil eder

    void Start()
    {
        // Oyuna başlarken canı maksimuma ayarla
        currentHealth = maxHealth;
        
        // UI'yı başlangıçta tam dolu göster
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
        }
        Debug.Log("Oyuncu Canı: " + currentHealth);
    }

    // Dışarıdan hasar almak için çağrılacak metod
    public void TakeDamage(int damageAmount)
    {
        // Canı azalt
        currentHealth -= damageAmount;
        
        // Can çubuğunu güncelle
        UpdateHealthBar(); 

        Debug.Log("Hasar Alındı! Kalan Can: " + currentHealth);

        // Can sıfıra veya altına düştüyse öl
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // YENİ METOT: Can çubuğunu matematiksel olarak günceller
    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            // Oran = Mevcut Can / Maksimum Can
            float fillRatio = (float)currentHealth / maxHealth;
            
            // Can çubuğunu bu orana göre doldur
            healthBarFill.fillAmount = fillRatio;
        }
    }

    void Die()
    {
        // Oyuncu öldüğünde yapılacaklar (Oyun bitti, animasyon oynat vb.)
        Debug.Log("OYUNCU ÖLDÜ!");
        
        // Örnek: Karakteri yok et
        Destroy(gameObject); 
        
        // Örnek: Oyunu durdur
        Time.timeScale = 0; 
    }
}