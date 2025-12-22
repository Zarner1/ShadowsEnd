using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Paneli")]
    public GameObject gameOverPanel; // Unity'de oluşturacağın siyah panel

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // Paneli aç
            Cursor.visible = true; // Mouse'u görünür yap
            Cursor.lockState = CursorLockMode.None; // Mouse kilidini kaldır
            
            // Oyunu durdurmak istersen (Arkadaki düşmanlar dursun):
            Time.timeScale = 0f; 
        }
    }

    // "Yeniden Dene" Butonu İçin
    public void RetryGame()
    {
        Time.timeScale = 1f; // Zamanı tekrar başlat

        // GameManager varsa cezalı sistemi çalıştır
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OyuncuOldu();
        }
        else
        {
            // Yoksa sahneyi düz yeniden başlat
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // "Ana Menü" Butonu İçin
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Build Settings'deki ilk sahneye (Menü) dön
    }
}