using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class player_health_system : MonoBehaviour
{
    [Header("UI Bağlantıları")]
    public TMP_Text healthText;     
    public Image healthBarImage;     
    public Gradient healthGradient;  

    [Header("Efektler")]
    public GameObject damagePopupPrefab;

    [Header("Game Over Ayarları")] // --- YENİ EKLENDİ ---
    public GameOverManager gameOverManager; // Unity'den Canvas üzerindeki scripti buraya sürükle

    [Header("Can Değerleri")]
    public float maxHealth = 100f; 
    public float currentHealth;

    private Character_Control charControl;

    void Start()
    {
        // GameManager'dan cezalı canı çek
        if (GameManager.Instance != null)
        {
            maxHealth = GameManager.Instance.GetKayitliMaxCan();
        }
        currentHealth = maxHealth;
        
        charControl = GetComponent<Character_Control>(); 
        
        UIBaglantilariniGuncelle();
        UpdateHealthUI();
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UIBaglantilariniGuncelle();
        UpdateHealthUI();
    }

    void UIBaglantilariniGuncelle()
    {
        GameObject barObj = GameObject.Find("HealthBar_Fill"); 
        if (barObj != null) healthBarImage = barObj.GetComponent<Image>();

        GameObject textObj = GameObject.Find("HealthText"); 
        if (textObj != null) healthText = textObj.GetComponent<TMP_Text>();

        // Game Over Manager'ı sahnede otomatik bul (Canvas üzerinde olmalı)
        if (gameOverManager == null)
        {
            gameOverManager = FindFirstObjectByType<GameOverManager>();
        }
    }

    public void TakeDamage(float amount, Transform attacker = null, float knockbackForce = 0f)
    {
        bool isBlockingSuccess = false;
        float damageToTake = amount;

        // Blok kontrolü
        if (charControl != null && charControl.isBlocking && attacker != null)
        {
            Vector2 directionToAttacker = (attacker.position - transform.position).normalized;
            float dotProduct = Vector2.Dot(transform.right, directionToAttacker);
            if (dotProduct > 0) isBlockingSuccess = true;
        }

        if (isBlockingSuccess)
        {
            damageToTake = amount / 2f; 
            Debug.Log("🛡️ Hasar Bloklandı!");
            if (knockbackForce > 0)
            {
                Vector2 knockbackDir = (transform.position - attacker.position).normalized;
                charControl.ApplyKnockback(knockbackDir, knockbackForce);
            }
        }
        else
        {
            damageToTake = amount;
            if (knockbackForce > 0 && attacker != null)
            {
                Vector2 knockbackDir = (transform.position - attacker.position).normalized;
                charControl.ApplyKnockback(knockbackDir, knockbackForce * 2f);
            }
        }

        currentHealth -= damageToTake;
        ShowDamagePopup((int)damageToTake);

        if (currentHealth < 0) currentHealth = 0;
        
        UpdateHealthUI();

        if (currentHealth == 0)
        {
            Die(); 
        }
    }

    void ShowDamagePopup(int damageAmount)
    {
        if (damagePopupPrefab != null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 1.5f, 0);
            GameObject popup = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null) popupScript.Setup(damageAmount);
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
        if (healthText != null) healthText.text = $"{currentHealth.ToString("F0")}/{maxHealth}";
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
        if (charControl != null) charControl.TriggerDeath();

        // --- ÖNEMLİ DEĞİŞİKLİK ---
        // Direkt yeniden başlatmak yerine Game Over ekranını açıyoruz.
        // 2 saniye bekle (ölüm animasyonu için), sonra ekranı aç.
        Invoke("OpenGameOverScreen", 2.0f);
    }

    void OpenGameOverScreen()
    {
        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOver();
        }
        else
        {
            // Eğer UI yoksa eski yöntemle yeniden başlat (Yedek Plan)
            Debug.LogWarning("Game Over UI bulunamadı, direkt resetleniyor.");
            if (GameManager.Instance != null) GameManager.Instance.OyuncuOldu();
        }
    }
}