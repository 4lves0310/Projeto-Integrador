using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMusica : MonoBehaviour
{
    public static MenuMusica Instance;

    [Header("Musica do Menu")]
    public AudioClip musicaMenu;
    [Range(0f, 1f)] public float volume = 0.5f;
    public float fadeDuration = 0.8f;
    public string nomeCenaMenu = "Menu";

    private AudioSource audioSource;
    private Coroutine fadeRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        VerificarCena(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        VerificarCena(scene.name);
    }

    private void VerificarCena(string cenaAtual)
    {
        if (cenaAtual == nomeCenaMenu)
        {
            TocarMusica();
        }
        else
        {
            PararMusica();
        }
    }

    public void TocarMusica()
    {
        if (musicaMenu == null || audioSource == null)
            return;

        if (audioSource.clip == musicaMenu && audioSource.isPlaying)
        {
            IniciarFade(volume);
            return;
        }

        audioSource.clip = musicaMenu;
        audioSource.Play();
        IniciarFade(volume);
    }

    public void PararMusica()
    {
        if (audioSource == null)
            return;

        if (!audioSource.isPlaying)
        {
            audioSource.volume = 0f;
            return;
        }

        IniciarFade(0f, true);
    }

    private void IniciarFade(float destinoVolume, bool pararAoZerar = false)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeVolume(destinoVolume, pararAoZerar));
    }

    private IEnumerator FadeVolume(float destino, bool pararAoZerar)
    {
        float inicial = audioSource.volume;
        float tempo = 0f;

        while (tempo < fadeDuration)
        {
            tempo += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(inicial, destino, tempo / fadeDuration);
            yield return null;
        }

        audioSource.volume = destino;

        if (pararAoZerar && Mathf.Approximately(destino, 0f))
        {
            audioSource.Stop();
            audioSource.clip = null;
        }

        fadeRoutine = null;
    }
}