using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Her yerden erişmek için Singleton yapısı

    [Header("Ceza Ayarları")]
    public float baslangicMaxCan = 100f; // Oyunun en başındaki saf can
    public float canAzalmaMiktari = 20f; // Her ölümde silinecek max can miktarı
    public float minCanLimiti = 20f;     // Can buna düşerse oyun tamamen biter (Game Over)

    private void Awake()
    {
        // Bu objenin sahneler arası yok olmasını engelle
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Oyuncunun o anki Max Can değerini getirir
    public float GetKayitliMaxCan()
    {
        // Eğer kayıt yoksa varsayılan değeri döndür
        return PlayerPrefs.GetFloat("MaxHP", baslangicMaxCan);
    }

    // Oyuncu öldüğünde çağrılacak fonksiyon
    public void OyuncuOldu()
    {
        float suankiMaxCan = GetKayitliMaxCan();
        float yeniMaxCan = suankiMaxCan - canAzalmaMiktari;

        if (yeniMaxCan < minCanLimiti)
        {
            Debug.Log("OYUN TAMAMEN BİTTİ! HER ŞEY SIFIRLANIYOR.");
            TamamenSifirla();
            SceneManager.LoadScene(0); // Ana menüye dön
        }
        else
        {
            Debug.Log("Öldün! Max canın düştü: " + yeniMaxCan);
            
            // Yeni cezalı canı kaydet
            PlayerPrefs.SetFloat("MaxHP", yeniMaxCan);
            PlayerPrefs.Save();

            // En baştan başlat (Sahne 1)
            // Not: İlerlemeyi (Unlocklar, itemler) korumak istiyorsan onları silme, sadece sahneyi yükle.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // Oyunu tamamen sıfırlamak için (Menüde 'New Game' butonu için de kullanabilirsin)
    public void TamamenSifirla()
    {
        PlayerPrefs.DeleteKey("MaxHP");
        // Varsa diğer kayıtları da (Itemler, Level kilidi) burada silebilirsin.
    }
}