using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // Dictionary için gerekli

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Ceza Ayarları")]
    public float baslangicMaxCan = 100f;
    public float canAzalmaMiktari = 20f;
    public float minCanLimiti = 20f;

    // --- YENİ: Düşmanların Can Veritabanı ---
    // String: Düşmanın ID'si, Float: Kalan Canı
    public Dictionary<string, float> dusmanHafizasi = new Dictionary<string, float>();

    private void Awake()
    {
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

    public float GetKayitliMaxCan()
    {
        return PlayerPrefs.GetFloat("MaxHP", baslangicMaxCan);
    }

    public void OyuncuOldu()
    {
        float suankiMaxCan = GetKayitliMaxCan();
        float yeniMaxCan = suankiMaxCan - canAzalmaMiktari;

        if (yeniMaxCan < minCanLimiti)
        {
            Debug.Log("OYUN TAMAMEN BİTTİ!");
            TamamenSifirla();
            SceneManager.LoadScene(0);
        }
        else
        {
            Debug.Log("Öldün! Max canın düştü: " + yeniMaxCan);
            
            PlayerPrefs.SetFloat("MaxHP", yeniMaxCan);
            PlayerPrefs.Save();

            // --- ÖNEMLİ: Düşman hafızasını SİLMİYORUZ, olduğu gibi tutuyoruz ---
            // Böylece sahne yeniden açıldığında düşmanlar eski verileri hatırlayacak.
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void TamamenSifirla()
    {
        PlayerPrefs.DeleteKey("MaxHP");
        
        // --- YENİ: Oyun tamamen bittiyse düşmanları da sıfırla ---
        dusmanHafizasi.Clear();
    }

    // --- YENİ: Düşmanların Veri Kaydetme Fonksiyonları ---
    public void DusmanVerisiniKaydet(string id, float can)
    {
        if (dusmanHafizasi.ContainsKey(id))
        {
            dusmanHafizasi[id] = can;
        }
        else
        {
            dusmanHafizasi.Add(id, can);
        }
    }

    public float DusmanVerisiniGetir(string id, float varsayilanCan)
    {
        if (dusmanHafizasi.ContainsKey(id))
        {
            return dusmanHafizasi[id];
        }
        return varsayilanCan;
    }
}