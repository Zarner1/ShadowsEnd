using UnityEngine;

public class Enemy_Arrow : MonoBehaviour
{
    [Header("Ok Ayarları")]
    public int damage = 10;        
    public float lifeTime = 5f;   // Saplandıktan sonra kaç saniye kalacak? (Sen 5 istedin)
    
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
        
        // Havada hiç bir şeye çarpmazsa 10 saniye sonra yok olsun (Performans için)
        Destroy(gameObject, 10f);
    }

    void Update()
    {
        if (isStuck) return;

        // Okun yönünü hareket ettiği açıya çevir (Gerçekçi uçuş)
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
            player_health_system playerHealth = hitInfo.GetComponent<player_health_system>();
            
            if (playerHealth != null)
            {
                // Hasarın geldiği yönü de gönderiyoruz
                playerHealth.TakeDamage(damage, transform);
            }
            
            // Oku ANINDA yok et
            Destroy(gameObject);
        }
        
        // --- 2. YERE VEYA BİNAYA ÇARPARSA ---
        // Buraya "Building" etiketini (Tag) ekledik.
        else if (hitInfo.CompareTag("Ground") || hitInfo.CompareTag("Building"))
        {
            // Okun saplandığı nesne hareket ediyorsa ok da onunla gitsin diye parent yapıyoruz
            transform.SetParent(hitInfo.transform);
            
            Stick(); // Saplanma fonksiyonunu çağır
        }
    }

    void Stick()
    {
        isStuck = true;
        
        // Fiziği durdur
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; // Artık fizik kuvvetlerinden etkilenmez
        rb.angularVelocity = 0f;
        
        // Okun kendi collider'ını kapat ki başka oklar buna çarpmasın veya oyuncu takılmasın
        if(col != null) col.enabled = false;
        
        // Belirlenen süre (5 sn) sonunda yok et
        Destroy(gameObject, lifeTime);
    }
}