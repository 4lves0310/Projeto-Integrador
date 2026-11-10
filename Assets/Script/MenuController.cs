using UnityEngine;
using System.Collections;

public class PressAnyKey : MonoBehaviour
{
    [Header("Telas do Menu")]
    public GameObject telaAtual;
    public GameObject proximaTela;

    [Header("Canvas Group da Tela Toda")]
    public CanvasGroup canvasGroupTela1; // Arraste o CanvasGroup da Tela 1 aqui

    [Header("Configurações")]
    public float tempoDoFade = 0.8f; // Duração do Fade Out em segundos

    private bool jaApertou = false;

    void Start()
    {
        // Garante que a Tela 1 comece totalmente visível (Alpha 1)
        if (canvasGroupTela1 != null)
        {
            canvasGroupTela1.alpha = 1f;
        }

        if (telaAtual != null) telaAtual.SetActive(true);
        if (proximaTela != null) proximaTela.SetActive(false);
    }

    void Update()
    {
        if (Input.anyKeyDown && !jaApertou)
        {
            jaApertou = true;
            StartCoroutine(FadeOutEProximaTela());
        }
    }

    IEnumerator FadeOutEProximaTela()
    {
        float contador = 0f;

        if (canvasGroupTela1 != null)
        {
            // Diminui o Alpha do Canvas Group de 1 (100%) para 0 (0%) frame a frame
            while (contador < tempoDoFade)
            {
                contador += Time.deltaTime;
                canvasGroupTela1.alpha = Mathf.Lerp(1f, 0f, contador / tempoDoFade);
                yield return null; // Espera o próximo frame da animação
            }

            canvasGroupTela1.alpha = 0f; // Garante que zera no final
        }

        // Troca as telas após o fade out terminar
        if (telaAtual != null) telaAtual.SetActive(false);
        if (proximaTela != null) proximaTela.SetActive(true);
    }
}