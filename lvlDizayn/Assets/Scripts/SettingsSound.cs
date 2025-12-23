using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsSound : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider volumeSlider;

    void Start()
    {
        float volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        volumeSlider.value = volume;
        SetVolume(volume);
    }

    public void SetVolume(float volume)
    {
        if (volume <= 0f) volume = 0.0001f;

        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }
}
