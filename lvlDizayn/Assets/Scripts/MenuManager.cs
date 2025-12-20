using UnityEngine;
using UnityEngine.SceneManagement; // Sahne yönetimi için gerekli

public class MenuManager : MonoBehaviour
{
    // Oyna butonu tarafından çağrılacak metot
    public void StartGame()
    {
        // YÜKSEK İHTİMALLE OYUN SAHNENİZİN INDEX'İ 1'DİR.
        // EĞER OYUN SAHNENİZİN ADINI BİLİYORSANIZ: 
        // SceneManager.LoadScene("YourGameSceneName"); olarak değiştirin.
        SceneManager.LoadScene(1); 
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