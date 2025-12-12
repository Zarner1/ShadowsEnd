using UnityEngine;

public class DusmanKodu : MonoBehaviour
{
    public Transform player;       // Oyuncu
    public GameObject mermiPrefab; // Fırlatılacak nesne
    public float menzil = 7f;      // Kaç metreden görsün (Genel 2D mesafe)
    public float atisHizi = 2f;  // Saniyede kaç atış
    
    // YENİ EKLENEN KISIM: Y Ekseninde Tolerans
    [Header("Y Ekseni Kontrolü")]
    [Tooltip("Oyuncu ile düşman arasındaki kabul edilebilir maksimum dikey fark.")]
    public float verticalTolerance = 1.0f; 

    private float zamanSayaci = 0;
    private Animator anim;

    void Start()
    {
        // Oyuncuyu otomatik bul (Oyuncunun Tag'i "Player" olmalı!)
        if (GameObject.FindGameObjectWithTag("Player") != null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
            
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        // Y ekseni farkını hesapla
        float verticalDifference = Mathf.Abs(player.position.y - transform.position.y);

        // Y EKSENİ KONTROLÜ
        if (verticalDifference > verticalTolerance)
        {
            // Eğer dikey fark toleransı aşarsa, görmezden gel ve çık.
            // Bu, düşmanın havaya ateş etmesini engeller.
            return; 
        }

        // Mesafe ölç (Zaten hem X hem Y'yi içerir)
        float mesafe = Vector2.Distance(transform.position, player.position);

        if (mesafe <= menzil) // Menzildeyse
        {
            // Yön çevirme mantığı (Aynı kalır, sadece fiziksel boyut kullanılır)
            float ilkBoyutX = Mathf.Abs(transform.localScale.x);
            float boyutY = transform.localScale.y; 
            float boyutZ = transform.localScale.z; 

            // Yüzünü oyuncuya dön
            if (player.position.x > transform.position.x)
            {
                // Oyuncu sağdaysa, düşman sağa baksın
                transform.localScale = new Vector3(ilkBoyutX, boyutY, boyutZ);
            }
            else
            {
                // Oyuncu soldaysa, düşman sola baksın
                transform.localScale = new Vector3(-ilkBoyutX, boyutY, boyutZ);
            }
            
            // Atış zamanı geldiyse
            if (Time.time > zamanSayaci)
            {
                Saldir();
                zamanSayaci = Time.time + atisHizi;
            }
        }
    }

    void Saldir()
    {
        // Animasyonu çalıştır
        anim.SetTrigger("Saldir");
        
        // Mermiyi oluştur
        GameObject yeniMermi = Instantiate(mermiPrefab, transform.position, Quaternion.identity);
        
        // SADECE YATAY YÖN (X EKSENİ) HAREKETİ
        
        float yonX = player.position.x - transform.position.x;
        
        // Yönün sadece yatay bileşenini al (Sign: 1 veya -1)
        Vector2 yon = new Vector2(Mathf.Sign(yonX), 0); 
        
        // Rigidbody'ye yatay hızı ver (10f hızı kullanıldı)
        // NOT: linearVelocity yerine rb.velocity kullanmak daha temizdir, ancak kodu koruduk.
        Rigidbody2D mermiRb = yeniMermi.GetComponent<Rigidbody2D>();
        if (mermiRb != null)
        {
            mermiRb.linearVelocity = yon * 10f; // velocity kullanıldı
        }
    }
}