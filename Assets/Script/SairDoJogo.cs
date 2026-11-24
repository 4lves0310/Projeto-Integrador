using System.Collections;
using UnityEngine;

public class SairDoJogo : MonoBehaviour
{
    [SerializeField] private float atrasoParaSair = 0.15f;

    private bool saindo = false;

    public void Sair()
    {
        if (saindo) return;
        saindo = true;

        Time.timeScale = 1f;
        AudioSFX.Instance?.TocarClique();

        StartCoroutine(ExecutarSaida());
    }

    private IEnumerator ExecutarSaida()
    {
        if (atrasoParaSair > 0f)
        {
            yield return new WaitForSecondsRealtime(atrasoParaSair);
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}