using UnityEngine;
using UnityEngine.SceneManagement; 

public class MenuManager : MonoBehaviour
{
    // Oyna butonu tarafından çağrılacak metot
    public void StartGame()
    {
        if (GameManager.Instance != null)
    {
        GameManager.Instance.TamamenSifirla();
    }
    else
    {
        Debug.LogWarning("GameManager yok! Max can sıfırlanamadı.");
    }

        // İlk sahneyi yükle (Sahne 1 olarak varsayıyoruz)
        SceneManager.LoadScene("LWL1");
    }

    // Ayarlar butonu için (İsteğe bağlı)
    public void OpenSettings()
    {
        Debug.Log("Ayarlar Açılıyor...");
        // Ayarlar panelini aktif etme kodları buraya gelir.
    }

    // Çıkış butonu tarafından çağrılacak metot
    public void QuitGame()
    {
        Debug.Log("Oyundan Çıkış Yapıldı!");
        
        // Sadece derlenmiş oyunda çalışır. Unity Editor'de çalışmaz.
        Application.Quit();
        
        // Editor'de test etmek için:
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}