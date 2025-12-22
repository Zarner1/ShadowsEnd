using UnityEngine;
using UnityEngine.SceneManagement; // Sahne bilgisini okumak için gerekli

public class CharacterScaler : MonoBehaviour
{
    void OnEnable()
    {
        // Sahne yüklendiğinde tetiklenecek olayı dinlemeye başla
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Dinlemeyi bırak (Hata almamak için önemli)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Bu fonksiyon her sahne açıldığında otomatik çalışır
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int sahneNo = scene.buildIndex;

        // Sahne Numaralarına göre boyut ayarlaması
        // Not: Build Profiles listesindeki numaraları kullanıyoruz.
        
        if (sahneNo == 1) // LWL1 (Başlangıç)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (sahneNo == 2) // LWL2 (İkinci Sahne)
        {
            transform.localScale = new Vector3(3, 3, 1);
        }
        else if (sahneNo == 3) // LWL3 (Üçüncü Sahne)
        {
            transform.localScale = new Vector3(6, 6, 1);
        }
    }
    
    // Eğer karakterin "DontDestroyOnLoad" değilse ve her sahnede yeniden oluşuyorsa,
    // Start fonksiyonu da işi garantiye almak için kullanılabilir:
    void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }
}