using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const string BackgroundMusicPath = "Music/andriih-cozy-cozy-coffee-music-568234";
    private const string SwapClipPath = "SFX/lesiakower-error-mistake-sound-effect-incorrect-answer-437420";
    private const string MatchClipPath = "SFX/kave_msri-glass-break-316720";
    private const string BombClipPath = "SFX/kave_msri-glass-break-316720";

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip matchClip;
    [SerializeField] private AudioClip bombClip;
    [SerializeField] private AudioClip swapClip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateIfMissing()
    {
        if (Instance != null || FindAnyObjectByType<AudioManager>() != null)
            return;

        new GameObject(nameof(AudioManager)).AddComponent<AudioManager>();
    }

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource ??= gameObject.AddComponent<AudioSource>();
        sfxSource ??= gameObject.AddComponent<AudioSource>();
        musicSource.spatialBlend = 0f;
        sfxSource.spatialBlend = 0f;
        musicSource.loop = true;

        backgroundMusic ??= Resources.Load<AudioClip>(BackgroundMusicPath);
        swapClip ??= Resources.Load<AudioClip>(SwapClipPath);
        matchClip ??= Resources.Load<AudioClip>(MatchClipPath);
        bombClip ??= Resources.Load<AudioClip>(BombClipPath);
    }
    void Start()
    {
        if(backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioManager could not load Background Music.");
        }
    }
    public void PlaySwapSfx() => PlayOneShot(swapClip);
    public void PlayMatchSfx() => PlayOneShot(matchClip);
    public void PlayBombSfx() => PlayOneShot(bombClip);
    private void PlayOneShot(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
        else
            Debug.LogWarning("AudioManager tried to play an unassigned SFX clip.");
    }
}
