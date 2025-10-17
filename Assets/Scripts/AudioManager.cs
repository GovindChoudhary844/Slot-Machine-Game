using UnityEngine;

/// <summary>
/// Singleton audio service for the slot-machine.
/// Plays one-shot SFX and looping reel scroll with master-volume control.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("--- Audio Clips ---")]
    [SerializeField] private AudioClip leverPullClip;   // lever pull SFX
    [SerializeField] private AudioClip reelScrollClip;  // looping while reels spin
    [SerializeField] private AudioClip reelStopClip;    // short "clunk" when reel locks
    [SerializeField] private AudioClip winClip;         // jackpot celebration

    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 0.6f; // inspector slider

    /* ---------- Audio Sources ---------- */
    private AudioSource source;      // general one-shot player
    private AudioSource loopSource;  // dedicated looping player

    /* ---------- Singleton ---------- */
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        // Classic persistent singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // survive scene loads

            // Create two sources on the same GameObject
            source = gameObject.AddComponent<AudioSource>();
            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.loop = true; // keep scroll sound looping

            ApplyVolume();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /* ---------- Volume Helpers ---------- */
    private void ApplyVolume()
    {
        source.volume = masterVolume;
        loopSource.volume = masterVolume;
    }

    // Called when you drag the slider in Play-mode
    private void OnValidate()
    {
        if (source != null) source.volume = masterVolume;
        if (loopSource != null) loopSource.volume = masterVolume;
    }

    /* ---------- Public API ---------- */
    public void PlayLeverPull() => source.PlayOneShot(leverPullClip, masterVolume);

    public void StartReelScroll()
    {
        loopSource.clip = reelScrollClip;
        loopSource.Play();
    }

    public void StopReelScroll()
    {
        if (loopSource.isPlaying) loopSource.Stop();
        source.PlayOneShot(reelStopClip, masterVolume); // single clunk
    }

    public void PlayWin() => source.PlayOneShot(winClip, masterVolume);
}