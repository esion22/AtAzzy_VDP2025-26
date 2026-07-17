using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Handles the settings menu functionality
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    /// <summary>
    /// Initializes the settings menu by setting up slider listeners
    /// </summary>
    void Start()
    {
        musicSlider.onValueChanged.AddListener(ifMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(ifSFXVolumeChanged);
    }

    /// <summary>
    /// Called when the music volume slider value changes
    /// </summary>
    /// <param name="volume">New volume value</param>
    public void ifMusicVolumeChanged(float volume)
    {
        AudioManager.Instance.SetMusicVolume(volume);
    }

    /// <summary>
    /// Called when the SFX volume slider value changes
    /// </summary>
    /// <param name="volume">New volume value</param>
    public void ifSFXVolumeChanged(float volume)
    {
        AudioManager.Instance.SetSFXVolume(volume);
    }
}