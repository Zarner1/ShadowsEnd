using UnityEngine;
using System.Collections;

public class IntroStoryManager : MonoBehaviour
{
    public GameObject introPanel;
    public float autoStartTime = 5f; // kaç saniye sonra geçsin

    void Start()
    {
        Time.timeScale = 0f;      // oyunu durdur
        introPanel.SetActive(true);
        Debug.Log("INTRO START");
        if (autoStartTime > 0)
        {
            StartCoroutine(AutoStart());
        }
    }

    IEnumerator AutoStart()
    {
        yield return new WaitForSecondsRealtime(autoStartTime);
        StartGame();
    }

    public void StartGame()
    {
        introPanel.SetActive(false);
        Time.timeScale = 1f;      // oyunu başlat
    }
}
