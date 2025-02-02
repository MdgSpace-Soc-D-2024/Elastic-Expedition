using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public Sprite MmuteSprite,SmuteSprite;         // Sprite for mute
    public Sprite MunmuteSprite,SunmuteSprite;       // Sprite for unmute
    public Image musicButtonImage;    // Button image for music
    public Image sfxButtonImage;      // Button image for SFX

    public AudioSource musicSource;   // AudioSource for background music
    public AudioSource sfxSource;     // AudioSource for SFX
    public AudioClip gameStartSFX;    // SFX to play when entering the game scene

    private bool isMusicMuted = false;
    private bool isSfxMuted = false;

    private void Start()
    {
        // Initialize audio states from PlayerPrefs
        isMusicMuted = PlayerPrefs.GetInt("IsMusicMuted", 0) == 1;
        isSfxMuted = PlayerPrefs.GetInt("IsSfxMuted", 0) == 1;

        // Apply initial states
        musicSource.mute = isMusicMuted;
        sfxSource.mute = isSfxMuted;

        UpdateMusicButtonSprite(musicButtonImage, isMusicMuted);
        UpdateSFXButtonSprite(sfxButtonImage, isSfxMuted);

        // Subscribe to scene load event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the scene load event to avoid errors
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Play SFX only when the game scene is loaded
        if ((scene.name == "Level_1"|| scene.name == "Level_2"|| scene.name == "Level_3"|| scene.name == "Custom_Scene") && gameStartSFX != null && !isSfxMuted)
        {
            sfxSource.PlayOneShot(gameStartSFX);
        }
    }

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;

        UpdateMusicButtonSprite(musicButtonImage, isMusicMuted);
        // Mute/unmute music and save state
        musicSource.mute = isMusicMuted;

        // Update button sprite
        
    }

    public void ToggleSFX()
    {
        isSfxMuted = !isSfxMuted;

        // Mute/unmute SFX and save state
        sfxSource.mute = isSfxMuted;

        // Update button sprite
        UpdateSFXButtonSprite(sfxButtonImage, isSfxMuted);
    }

    private void UpdateMusicButtonSprite(Image buttonImage, bool isMuted)
    {
        // Change the button's sprite based on the mute state
        buttonImage.sprite = isMuted ? MmuteSprite : MunmuteSprite;
    }
    private void UpdateSFXButtonSprite(Image buttonImage, bool isMuted)
    {
        // Change the button's sprite based on the mute state
        buttonImage.sprite = isMuted ? SmuteSprite : SunmuteSprite;
    }
}