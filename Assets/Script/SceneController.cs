using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para gerir o carregamento de cenas

public class SceneController : MonoBehaviour
{
    [Header("Configuração da Cena")]
    [Tooltip("Digita aqui o nome exato da cena do teu jogo (ex: Nivel1, Jogo, Scene2)")]
    public string nomeDaCenaDoJogo = "Nivel1";

    // Função pública para ser chamada pelo OnClick() do Botão
    public void IniciarJogo()
    {
        Debug.Log("A tentar carregar a cena: " + nomeDaCenaDoJogo);

        if (!string.IsNullOrEmpty(nomeDaCenaDoJogo))
        {
            SceneManager.LoadScene(nomeDaCenaDoJogo);
        }
        else
        {
            Debug.LogError("O nome da cena está vazio no Inspector!");
        }
    }
}