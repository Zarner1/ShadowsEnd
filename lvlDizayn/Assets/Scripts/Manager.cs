using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Manager : MonoBehaviour
{
    public static Manager Instance;
    public GameObject hudCanvas;
    [Header("UI")]
    public GameObject breakPanel;
    public TMPro.TextMeshProUGUI breakText;

    [Header("Scene Control")]
    public string nextSceneName;     // normal bölüm sonrası
    public bool isFinalLevel = false; // SON BÖLÜM MÜ?

    public float waitTime = 3f;

    private int enemyCount;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    public void EnemyDied()
    {
        enemyCount--;

        if (enemyCount <= 0)
        {
            StartCoroutine(LevelFinished());
        }
    }

    IEnumerator LevelFinished()
{
    // Oyunu durdur
    Time.timeScale = 0f;

    // HUD'u kapat (health bar vs.)
    if (hudCanvas != null)
        hudCanvas.SetActive(false);

    // Ara sahne panelini aç
    if (breakPanel != null)
        breakPanel.SetActive(true);

    // Oyun durmuşken bekle
    yield return new WaitForSecondsRealtime(waitTime);

    // Oyunu tekrar başlat
    Time.timeScale = 1f;

    // Sahne geçişi
    if (isFinalLevel)
    {
        SceneManager.LoadScene("main_page");
    }
    else
    {
        SceneManager.LoadScene(nextSceneName);
    }
}

}
