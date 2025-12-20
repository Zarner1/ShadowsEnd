using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class health_system : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("UI Bağlantıları")]
    public Image healthBarImage;
    public GameObject healthCanvas;

    // Her iki script türü için değişken tanımlıyoruz
    private enemy_knight_movement knightScript;
    private Enemy_archer_movement archerScript;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        // Üzerimdeki script hangisi? Onu bulup değişkene ata.
        knightScript = GetComponent<enemy_knight_movement>();
        archerScript = GetComponent<Enemy_archer_movement>();

        UpdateHealthUI();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log(gameObject.name + " hasar aldı. Kalan Can: " + currentHealth);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBarImage != null)
        {
            float fillValue = currentHealth / maxHealth;
            healthBarImage.fillAmount = fillValue;
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log(gameObject.name + " öldü.");

        // Can barını gizle
        if (healthCanvas != null)
        {
            healthCanvas.SetActive(false);
        }

        // Hangi script varsa onun ölüm fonksiyonunu çalıştır
        if (knightScript != null)
        {
            knightScript.TriggerDeath();
        }
        else if (archerScript != null)
        {
            archerScript.TriggerDeath();
        }

        // 5 Saniye sonra objeyi tamamen yok et (Ceset 5 saniye yerde kalır)
        Destroy(gameObject, 5f);
    }
}