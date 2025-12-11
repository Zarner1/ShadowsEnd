using UnityEngine;

public class DusmanKodu : MonoBehaviour
{
    public Transform player;       // Oyuncu (Sürükle bırak yapmana gerek yok, kod bulacak)
    public GameObject mermiPrefab; // Fırlatılacak nesne (Prefab'i buraya sürükle)
    public float menzil = 7f;      // Kaç metreden görsün
    public float atisHizi = 2f;  // Saniyede kaç atış

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

        // Mesafe ölç
        float mesafe = Vector2.Distance(transform.position, player.position);

        if (mesafe <= menzil) // Menzildeyse
        {
            // --- DÜZELTİLEN KISIM BAŞLANGICI ---
            
            // Mevcut boyutun pozitif halini al (Örn: -3 ise 3 yap, 3 ise 3 kalsın)
            float ilkBoyutX = Mathf.Abs(transform.localScale.x);
            float boyutY = transform.localScale.y; // Yüksekliği bozma
            float boyutZ = transform.localScale.z; // Derinliği bozma

            // Yüzünü oyuncuya dön
            if (player.position.x > transform.position.x)
            {
                // Oyuncu sağdaysa, X pozitif olsun (Sağa bak)
                transform.localScale = new Vector3(-ilkBoyutX, boyutY, boyutZ);
            }
            else
            {
                // Oyuncu soldaysa, X negatif olsun (Sola bak)
                transform.localScale = new Vector3(ilkBoyutX, boyutY, boyutZ);
            }
            
            // --- DÜZELTİLEN KISIM BİTİŞİ ---

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
        
        // Mermiyi oluştur (Tam düşmanın olduğu yerde)
        GameObject yeniMermi = Instantiate(mermiPrefab, transform.position, Quaternion.identity);
        
        // --- DÜZELTME BAŞLANGICI: SADECE YATAY YÖN (X EKSENİ) HAREKETİ ---
        
        // Oyuncunun X pozisyonu ile düşmanın X pozisyonu arasındaki farkı bul
        float yonX = player.position.x - transform.position.x;
        
        // Yönün sadece yatay bileşenini (sağ veya sol) normalleştir.
        // Eğer yonX pozitifse (oyuncu sağdaysa), yon = 1
        // Eğer yonX negatifse (oyuncu soldaysa), yon = -1
        Vector2 yon = new Vector2(Mathf.Sign(yonX), 0).normalized; 
        
        // Rigidbody'ye yatay hızı ver (5f hızı kullanıldı)
        // Burada linearVelocity yerine velocity (veya AddForce) kullanmak daha doğru olacaktır, 
        // ancak mevcut yapıyı korumak için bunu kullanalım.
        yeniMermi.GetComponent<Rigidbody2D>().linearVelocity = yon * 10f;
        
        // NOT: Mermiyi fırlattıktan sonra merminin kendi kodunda (Bullet.cs)
        // herhangi bir transform.Translate() kodu olmadığından emin olun.
    }
}