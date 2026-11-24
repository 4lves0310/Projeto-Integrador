using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("Configuração da Cena")]
    [Tooltip("Digita aqui o nome exato da cena do teu jogo (ex: Nivel1, Jogo, Scene2)")]
    public string nomeDaCenaDoJogo = "Nivel1";

    public void IniciarJogo()
    {
        Debug.Log("A tentar carregar a cena: " + nomeDaCenaDoJogo);

        if (!string.IsNullOrEmpty(nomeDaCenaDoJogo))
        {
            AudioSFX.Instance?.TocarTrocarCena();
            SceneManager.LoadScene(nomeDaCenaDoJogo);
        }
        else
        {
            Debug.LogError("O nome da cena está vazio no Inspector!");
        }
    }
}