using UnityEngine;
using TMPro;

public class PlayerInteracao : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI textoPrompt; // Arraste o componente de texto TMP aqui
    public GameObject painelPrompt;     // Opcional: objeto pai do texto para ocultar/exibir

    private IInteragivel interagivelAtual;

    void Update()
    {
        // Pressionar a tecla E para interagir
        if (interagivelAtual != null && Input.GetKeyDown(KeyCode.E))
        {
            interagivelAtual.Interagir();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se o objeto com o qual colidiu implementa IInteragivel
        IInteragivel interagivel = collision.GetComponent<IInteragivel>();
        if (interagivel != null)
        {
            interagivelAtual = interagivel;
            AtualizarUI(interagivelAtual.ObterMensagem());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteragivel interagivel = collision.GetComponent<IInteragivel>();
        if (interagivel != null && interagivel == interagivelAtual)
        {
            interagivelAtual = null;
            EsconderUI();
        }
    }

    void AtualizarUI(string mensagem)
    {
        if (textoPrompt != null) textoPrompt.text = mensagem;
        if (painelPrompt != null) painelPrompt.SetActive(true);
    }

    void EsconderUI()
    {
        if (painelPrompt != null) painelPrompt.SetActive(false);
    }
}