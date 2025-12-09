using UnityEngine;
using UnityEngine.UI;

public class health_system : MonoBehaviour
{
    [Header("UI Ayarları")]
    public Image healthBarFill; // Yeşil/Kırmızı dolan kısım
    public GameObject healthCanvas; // Barın olduğu canvas (Ölünce kapatmak için)

    [Header("Can Değerleri")]
    public float maxHealth = 50f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateBar();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        UpdateBar();
    }

    void UpdateBar()
    {
        // Barın doluluk oranını ayarla
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        // 1. Hareket scriptine ulaş ve ölümü tetikle
        if(GetComponent<enemy_knight_movement>() != null)
        {
            GetComponent<enemy_knight_movement>().TriggerDeath();
        }

        // 2. Can barını gizle
        if(healthCanvas != null)
        {
            healthCanvas.SetActive(false);
        }

        Debug.Log(gameObject.name + " öldü!");
        
        // 3. 5 saniye sonra cesedi tamamen yok et
        Destroy(gameObject, 5.0f); 
    }
}