using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para gerenciar cenas

public class TrocarCena : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Nome exato da cena para onde quer ir (ex: Fases, Menu, Sala2)")]
    public string nomeDaCena;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se quem encostou no collider foi o Jogador
        if (collision.CompareTag("Player"))
        {
            CarregarNovaCena();
        }
    }

    void CarregarNovaCena()
    {
        if (!string.IsNullOrEmpty(nomeDaCena))
        {
            SceneManager.LoadScene(nomeDaCena);
        }
        else
        {
            Debug.LogWarning("O nome da cena não foi definido no Inspector!");
        }
    }
}