using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip leverPullClip;
    [SerializeField] private AudioClip reelScrollClip;
    [SerializeField] private AudioClip reelStopClip;
    [SerializeField] private AudioClip winClip;

    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 0.6f;

    private AudioSource source;
    private AudioSource loopSource;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            source = gameObject.AddComponent<AudioSource>();
            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.loop = true;

            ApplyVolume();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /* ---------- volume ---------- */
    private void ApplyVolume()
    {
        source.volume = masterVolume;
        loopSource.volume = masterVolume;
    }

    private void OnValidate()          // inspector tweak at runtime
    {
        if (source != null) source.volume = masterVolume;
        if (loopSource != null) loopSource.volume = masterVolume;
    }

    /* ---------- one-shot helpers ---------- */
    public void PlayLeverPull() => source.PlayOneShot(leverPullClip, masterVolume);

    public void StartReelScroll()
    {
        loopSource.clip = reelScrollClip;
        loopSource.Play();
    }

    public void StopReelScroll()
    {
        if (loopSource.isPlaying) loopSource.Stop();
        source.PlayOneShot(reelStopClip, masterVolume);
    }

    public void PlayWin() => source.PlayOneShot(winClip, masterVolume);
}