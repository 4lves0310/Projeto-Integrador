using UnityEngine;

public class AudioSFX : MonoBehaviour
{
    public static AudioSFX Instance { get; private set; }

    [Header("Componentes")]
    public AudioSource sfxSource;

    [Header("UI e Menus")]
    public AudioClip somMenuEscape;
    public AudioClip somCliqueBotao;

    [Header("Gameplay, Diálogo e Missões")]
    public AudioClip somLetraDialogo;
    public AudioClip somInteragir;
    public AudioClip somColetarItem;
    public AudioClip somMissaoAtualizada;
    public AudioClip somMissaoConcluida;
    public AudioClip somTrocarCena;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();
    }

    public void TocarSom(AudioClip clip, float volume = 1f, float pitchMin = 0.96f, float pitchMax = 1.04f)
    {
        if (sfxSource == null || clip == null) return;

        sfxSource.pitch = Random.Range(pitchMin, pitchMax);
        sfxSource.PlayOneShot(clip, volume);
        sfxSource.pitch = 1f;
    }

    public void TocarMenu() => TocarSom(somMenuEscape, 1f, 1f, 1f);
    public void TocarClique() => TocarSom(somCliqueBotao, 1f, 0.98f, 1.02f);
    public void TocarLetra() => TocarSom(somLetraDialogo, 0.25f, 0.92f, 1.08f);
    public void TocarInteragir() => TocarSom(somInteragir, 1f, 0.95f, 1.05f);
    public void TocarColetar() => TocarSom(somColetarItem, 1f, 0.95f, 1.05f);
    public void TocarMissaoAtualizada() => TocarSom(somMissaoAtualizada, 1f, 1f, 1f);
    public void TocarMissaoConcluida() => TocarSom(somMissaoConcluida, 1f, 1f, 1f);
    public void TocarTrocarCena() => TocarSom(somTrocarCena, 1f, 1f, 1f);
}