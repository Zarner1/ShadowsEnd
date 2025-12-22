using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro kullanıyorsan bu kütüphane gerekli (Yoksa UnityEngine.UI yeterli)
using UnityEngine.UI; // Normal Text kullanıyorsan bu gerekli

public class DoorTrigger : MonoBehaviour
{
    [Header("Ayarlar")]
    public string gidilecekSahneAdi = "LWL2";
    public string dusmanTagi = "Enemy"; // Düşmanlara verdiğin Tag ismi
    
    [Header("UI Ayarları")]
    public GameObject bilgilendirmeObjesi; // Text objesinin kendisi (Açıp kapatmak için)
    public Text bilgilendirmeYazisi;       // Yazıyı değiştirmek için (Legacy Text ise)
    // public TMP_Text bilgilendirmeYazisi; // Eğer TextMeshPro kullanıyorsan üsttekini sil bunu aç

    private bool oyuncuKapidaMi = false;

    void Start()
    {
        if (bilgilendirmeObjesi != null)
            bilgilendirmeObjesi.SetActive(false);
    }

    void Update()
    {
        // Kapıdaysa ve E'ye bastıysa
        if (oyuncuKapidaMi && Input.GetKeyDown(KeyCode.E))
        {
            DusmanKontroluVeGecis();
        }
    }

    void DusmanKontroluVeGecis()
    {
        // Sahnede bu Tag'e sahip kaç obje kaldığını sayar
        int kalanDusman = GameObject.FindGameObjectsWithTag(dusmanTagi).Length;

        if (kalanDusman <= 0)
        {
            // Hiç düşman kalmadı, geçiş serbest!
            SceneManager.LoadScene(gidilecekSahneAdi);
        }
        else
        {
            // Düşmanlar hala hayatta!
            Debug.Log("Hala " + kalanDusman + " düşman var!");
            
            // Oyuncuya uyarı verelim
            if (bilgilendirmeYazisi != null)
            {
                bilgilendirmeYazisi.text = "Önce tüm düşmanları temizle! (" + kalanDusman + " kaldı)";
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            oyuncuKapidaMi = true;
            
            if (bilgilendirmeObjesi != null)
            {
                bilgilendirmeObjesi.SetActive(true);
                // Kapıya ilk geldiğinde standart yazıyı gösterelim
                bilgilendirmeYazisi.text = "Sonraki seviye için E tuşuna bas"; 
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            oyuncuKapidaMi = false;
            
            if (bilgilendirmeObjesi != null)
                bilgilendirmeObjesi.SetActive(false);
        }
    }
}