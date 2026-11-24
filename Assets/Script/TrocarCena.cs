using UnityEngine;
using UnityEngine.SceneManagement;

public class TrocaCenaAoEncostar : MonoBehaviour
{
    [Header("Configurações da Cena")]
    [Tooltip("Digite o nome exato da cena que vai carregar")]
    public string nomeDaCena;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CarregarCena();
        }
    }

    void CarregarCena()
    {
        if (!string.IsNullOrEmpty(nomeDaCena))
        {
            AudioSFX.Instance?.TocarTrocarCena();
            SceneManager.LoadScene(nomeDaCena);
        }
        else
        {
            Debug.LogWarning("Nome da cena não foi preenchido no Inspector!");
        }
    }
}