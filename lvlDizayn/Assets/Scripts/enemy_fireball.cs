using UnityEngine;

public class enemy_fireball : MonoBehaviour
{
    [Header("Alev Topu Ayarları")]
    public float speed = 10f;    // Hız
    public int damage = 20;      // Hasar
    public float lifeTime = 5f;  // Yok olma süresi

    void Start()
    {
        // Ömrü dolunca yok olsun
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Oluşturulduğu yöne (kendi sağına) doğru düz gider
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // 1. OYUNCUYA ÇARPARSA
        if (hitInfo.CompareTag("Player"))
        {
            player_health_system playerHealth = hitInfo.GetComponent<player_health_system>();
            if (playerHealth != null)
            {
                // Hasar ver (transform gönderiyoruz ki bloklanabilsin)
                playerHealth.TakeDamage(damage, transform);
            }
            
            // Çarpınca yok olsun
            Destroy(gameObject);
        }
        // 2. YERE VEYA DUVARA ÇARPARSA
        else if (hitInfo.CompareTag("Ground"))
        {
             // İstersen burada patlama efekti (Instantiate) oluşturabilirsin
             Destroy(gameObject);
        }
    }
}