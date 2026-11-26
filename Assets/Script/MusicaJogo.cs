using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicaJogo : MonoBehaviour
{
    public static MusicaJogo Instance;

    [Serializable]
    public struct MusicaPorCena
    {
        public string nomeCena;
        public AudioClip musica;
    }

    [Header("Musica Padrao do Jogo")]
    public AudioClip musicaPadrao;
    public List<string> cenasSemMusica = new List<string> { "Menu" };

    [Header("Musicas Especificas por Cena")]
    public List<MusicaPorCena> musicasPorCena = new List<MusicaPorCena>();

    [Header("Configuracoes de Audio")]
    [Range(0f, 1f)] public float volumePadrao = 0.35f;
    public float fadeDuration = 0.8f;

    private AudioSource audioSource;
    private Coroutine fadeRoutine;
    private float multiplicadorVolume = 1f;

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
        AvaliarCena(SceneManager.GetActiveScene().name);
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
        AvaliarCena(scene.name);
    }

    public void AvaliarCena(string nomeCena)
    {
        if (string.IsNullOrWhiteSpace(nomeCena) || cenasSemMusica.Contains(nomeCena))
        {
            PararMusica();
            return;
        }

        AudioClip musicaEscolhida = ObterMusicaDaCena(nomeCena);

        if (musicaEscolhida == null)
        {
            PararMusica();
            return;
        }

        if (audioSource.clip == musicaEscolhida && audioSource.isPlaying)
        {
            AplicarFade(volumePadrao * multiplicadorVolume);
            return;
        }

        audioSource.clip = musicaEscolhida;
        audioSource.Play();
        AplicarFade(volumePadrao * multiplicadorVolume);
    }

    private AudioClip ObterMusicaDaCena(string nomeCena)
    {
        for (int i = 0; i < musicasPorCena.Count; i++)
        {
            if (musicasPorCena[i].nomeCena == nomeCena)
            {
                return musicasPorCena[i].musica;
            }
        }

        return musicaPadrao;
    }

    public void DefinirMultiplicadorVolume(float multiplicador)
    {
        multiplicadorVolume = Mathf.Clamp01(multiplicador);

        if (audioSource != null && audioSource.isPlaying)
        {
            AplicarFade(volumePadrao * multiplicadorVolume);
        }
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

        AplicarFade(0f, true);
    }

    private void AplicarFade(float destino, bool pararAoZerar = false)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeVolumeCoroutine(destino, pararAoZerar));
    }

    private IEnumerator FadeVolumeCoroutine(float destino, bool pararAoZerar)
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