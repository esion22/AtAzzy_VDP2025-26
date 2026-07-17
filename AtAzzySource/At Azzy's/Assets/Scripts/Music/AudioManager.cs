using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource walkingSource;
    [SerializeField] private AudioSource boilingSource;
    [SerializeField] private AudioSource cuttingSource;
    [SerializeField] private AudioSource stoveSource;
    [SerializeField] private AudioSource ovenSource;
    [SerializeField] private AudioSource kegSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip backgroundMusicFarm;
    [SerializeField] private AudioClip backgroundMusicTavern;
    [SerializeField] private AudioClip backgroundFinaleMusic;
    [SerializeField] private AudioClip finaleTransformationMusic;
    [SerializeField] private AudioClip finaleHappyEndingMusic;
    
    [SerializeField] private List<AudioClip> walkingInside;
    [SerializeField] private List<AudioClip> walkingOutside;
    
    [SerializeField] private AudioClip cookingReadySFX;
    [SerializeField] private AudioClip cookingFailedSFX;
    [SerializeField] private AudioClip pickupItemSFX;
    
    private Scene _currentScene;
    private bool _isFirst;
    private Coroutine fadeOutCoroutine;

    /*
     * Awake is called when the script instance is being loaded.
     * It initializes the singleton instance and sets up scene change handling.
     */
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;      
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /*
     * Start is called before the first frame update
     */
    public void Start()
    {
        _isFirst = true;
    }
    
    
    /*
     * Called when a new scene is loaded, to update background music accordingly
     * @param scene The scene that was loaded
     * @param mode The load scene mode
     */
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioClip clipToPlay = null;
        _currentScene = scene;
        switch (_currentScene.name)
        {
            case "BootScene":
                return;
            case "FarmScene":
                clipToPlay = backgroundMusicFarm;
                break;
            case "TavernScene":
                clipToPlay = backgroundMusicTavern;
                break;
        }
        
        if (clipToPlay != null)
            PlayBackgroundMusic(clipToPlay);
    }
    
    /*
     * Plays the specified background music clip
     * @param musicClip The AudioClip to play as background music
     */
    public void PlayBackgroundMusic(AudioClip musicClip)
    {
        if (backgroundMusicSource.clip == musicClip && backgroundMusicSource.isPlaying)
            return;

        backgroundMusicSource.clip = musicClip;
        backgroundMusicSource.loop = true;
        backgroundMusicSource.Play();
    }
    
    /*
     * Plays the farm background music
     */
    public void PlayBackgroundFarmMusic()
    {
        PlayBackgroundMusic(backgroundMusicFarm);
    }
    
    /*
     * Plays the tavern background music
     */
    public void PlayBackgroundTavernMusic()
    {
        PlayBackgroundMusic(backgroundMusicTavern);
    }
    
    /*
     * Plays the specified sound effect clip
     * @param sfxClip The AudioClip to play as a sound effect
     */
    public void PlaySFX(AudioClip sfxClip)
    {
        sfxSource.PlayOneShot(sfxClip);
    }
    
    /*
     * Plays the specified sound effect clip at a reduced volume
     * @param sfxClip The AudioClip to play as a sound effect
     * @param volumeScale The scale of the volume (0.0 to 1.0)
     */
    public void PlaySFXVolumeReduced(AudioClip sfxClip, float volumeScale)
    {
        sfxSource.PlayOneShot(sfxClip, volumeScale);
    }
    
    /*
     * Plays a random walking sound based on the current scene
     */
    public void PlayWalkingSound()
    {
        List<AudioClip> walkingClips = _currentScene.name == "TavernScene" ? walkingInside : walkingOutside;
        if (walkingClips.Count == 0) return;

        AudioClip clipToPlay = _isFirst ? walkingClips[0] : walkingClips[1];
        // alternate the two based on the last played
        if (_isFirst)
        {
            _isFirst = false;
            
        }
        else
        {
            _isFirst = true;
        }
        walkingSource.PlayOneShot(clipToPlay);
    }
    
    /*
     * Checks if the walking sound is currently playing
     * @return True if the walking sound is playing, false otherwise
     */
    public bool IsWalkingSoundPlaying()
    {
        return walkingSource.isPlaying;
    }
    
    /*
     * Stops the walking sound effect
     */
    public void StopWalkingSound()
    {
        walkingSource.Stop();
    }
    
    /*
     * Plays the boiling sound effect
     */
    public void PlayBoilingSound(bool stop = false)
    {
        if (stop)
        {
            boilingSource.Stop();
            return;
        }
        
        if (!boilingSource.isPlaying)
        {
            boilingSource.loop = true;
            boilingSource.Play();
        }
    }
    
    /*
     * Plays the cutting sound effect once
     */
    public void PlayCuttingSound()
    {
        cuttingSource.PlayOneShot(cuttingSource.clip);
    }
    
    /*
     * Plays the stove sound effect
     */
    public void PlayStoveSound(bool stop = false)
    {
        if (stop)
        {
            stoveSource.Stop();
            return;
        }
        if (!stoveSource.isPlaying)
        {
            stoveSource.loop = true;
            stoveSource.Play();
        }
    }
    
    /*
     * Plays the oven sound effect
     */
    public void PlayOvenSound(bool stop = false)
    {
        if (stop)
        {
            ovenSource.Stop();
            return;
        }
        if (!ovenSource.isPlaying)
        {
            ovenSource.loop = true;
            ovenSource.Play();
        }
    }
    
    /*
     * Plays the keg sound effect
     */
    public void PlayKegSound(bool stop = false)
    {
        if (stop)
        {
            kegSource.Stop();
            return;
        }

        if (!kegSource.isPlaying)
        {
            kegSource.loop = true;
            kegSource.Play();
        }
    }
    
    /*
     * Plays the cooking ready sound effect
     */
    public void PlayCookingReadySound()
    {
        PlaySFX(cookingReadySFX);
    }
    
    /*
     * Plays the cooking failed sound effect
     */
    public void PlayCookingFailedSound()
    {
        PlaySFX(cookingFailedSFX);
    }
    
    /*
     * Plays the item pickup sound effect
     */
    public void PlayPickupItemSound()
    {
        PlaySFX(pickupItemSFX);
    }
    
    public void SetMusicVolume(float volume)
    {
        backgroundMusicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        walkingSource.volume = volume;
        boilingSource.volume = volume;
        cuttingSource.volume = volume;
        stoveSource.volume = volume;
        ovenSource.volume = volume;
        kegSource.volume = volume;

    }
    
    /*
     * Plays the finale background music
     */
    public void PlayBackgroundFinaleMusic()
    {
        backgroundMusicSource.Stop();
        backgroundMusicSource.clip = backgroundFinaleMusic;
        backgroundMusicSource.loop = true;
        backgroundMusicSource.Play();
    }

    /*
     * Plays the happy ending background music
     */
    public void PlayFinaleHappyEndingMusic()
    {
        backgroundMusicSource.Stop();
        backgroundMusicSource.clip = finaleHappyEndingMusic;
        backgroundMusicSource.loop = true;
        backgroundMusicSource.Play();
    }

    /*
     * Stops the background music
     */
    public void StopBackgroundMusic(float fadeDuration = 0.5f)
    {
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
        }
        fadeOutCoroutine = StartCoroutine(FadeOutMusic(backgroundMusicSource, fadeDuration));
    }

    /*
     * Fades out the specified audio source over the given duration
     * @param audioSource The AudioSource to fade out
     * @param duration The duration of the fade out in seconds
     */
    private IEnumerator FadeOutMusic(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // Reset volume for future use
    }
}
