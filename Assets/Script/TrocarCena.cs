using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para gerenciar cenas

public class TrocaCenaAoEncostar : MonoBehaviour
{
    [Header("Configurações da Cena")]
    [Tooltip("Digite o nome exato da cena que vai carregar")]
    public string nomeDaCena;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Garante que apenas o Player ativa a troca de cena
        if (collision.CompareTag("Player"))
        {
            CarregarCena();
        }
    }

    void CarregarCena()
    {
        if (!string.IsNullOrEmpty(nomeDaCena))
        {
            SceneManager.LoadScene(nomeDaCena);
        }
        else
        {
            Debug.LogWarning("Nome da cena não foi preenchido no Inspector!");
        }
    }
}