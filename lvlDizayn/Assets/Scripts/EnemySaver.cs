using UnityEngine;

public class EnemySaver : MonoBehaviour
{
    [Header("Kimlik Ayarı")]
    public string uniqueID; // Her düşman için EŞSİZ olmalı!

    private health_system myHealthScript;

    void Start()
    {
        // 1. Eğer ID boşsa otomatik bir ID oluştur (Pozisyona göre)
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = transform.position.ToString() + gameObject.name;
        }

        myHealthScript = GetComponent<health_system>();

        if (GameManager.Instance != null && myHealthScript != null)
        {
            // 2. GameManager'a sor: "Benim için kayıtlı bir can var mı?"
            // Eğer kayıt yoksa 'maxHealth' değerini, varsa kayıtlı değeri alır.
            float kayitliCan = GameManager.Instance.DusmanVerisiniGetir(uniqueID, myHealthScript.maxHealth);

            // 3. Eğer kayıtlı can 0 veya daha az ise bu düşman daha önce ölmüştür.
            if (kayitliCan <= 0)
            {
                gameObject.SetActive(false); // Veya Destroy(gameObject);
            }
            else
            {
                // 4. Düşman yaşıyorsa canını güncelle
                myHealthScript.currentHealth = kayitliCan;
                
                // Eğer UI varsa onu da güncellemek gerekebilir, health_system'deki UpdateHealthUI public değilse
                // ilk hasar yediğinde düzelir veya health_system'e küçük bir dokunuş yapabiliriz.
            }
        }
    }

    // Sahne değiştiğinde veya Obje yok olduğunda çalışır
    void OnDisable()
    {
        // Oyun kapanırken veya sahne değişirken son durumu GameManager'a bildir
        if (GameManager.Instance != null && myHealthScript != null)
        {
            GameManager.Instance.DusmanVerisiniKaydet(uniqueID, myHealthScript.currentHealth);
        }
    }
}