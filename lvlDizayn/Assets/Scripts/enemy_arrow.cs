using UnityEngine;

public class Enemy_Arrow : MonoBehaviour
{
    [Header("Ok Ayarları")]
    public int damage = 10;        
    public float lifeTime = 5f;   // Yerle temas ederse ne kadar kalacağı
    
    [Header("Görsel Ayarlar")]
    [Tooltip("Eğer ok yan gidiyorsa burayı 90, -90 veya 180 yap")]
    public float rotationOffset = 0f; 

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isStuck = false; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        
        // Havada hiç bir şeye çarpmazsa 10 saniye sonra yok olsun
        Destroy(gameObject, 10f);
    }

    void Update()
    {
        if (isStuck) return;

        // Okun yönünü hareket ettiği açıya çevir
        Vector2 velocity = rb.linearVelocity;
        if (velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle + rotationOffset, Vector3.forward);
        }
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (isStuck) return;

        // --- 1. OYUNCUYA ÇARPARSA ---
        if (hitInfo.CompareTag("Player"))
        {
            // Oyuncunun can scriptine ulaş
            player_health_system playerHealth = hitInfo.GetComponent<player_health_system>();
            
            // Eğer script varsa hasar ver
            if (playerHealth != null)
            {
                // DEĞİŞİKLİK BURADA: Hasar verirken 'transform' (okun kendisi) gönderiliyor.
                // Böylece oyuncu hasarın okun geldiği yönden olduğunu anlıyor.
                playerHealth.TakeDamage(damage, transform);
            }
            
            // Oku ANINDA yok et (Saplanmasın)
            Destroy(gameObject);
        }
        
        // 2. YERE ÇARPARSA
        else if (hitInfo.CompareTag("Ground"))
        {
            Stick(); // Yerde saplanıp kalsın
        }
    }

    void Stick()
    {
        isStuck = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.angularVelocity = 0f;
        if(col != null) col.enabled = false;
        Destroy(gameObject, lifeTime);
    }
}