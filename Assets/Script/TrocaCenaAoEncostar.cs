using UnityEngine;
using UnityEngine.SceneManagement;

public class TrocarCena : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Nome exato da cena para onde quer ir (ex: Fases, Menu, Sala2)")]
    public string nomeDaCena;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CarregarNovaCena();
        }
    }

    void CarregarNovaCena()
    {
        if (!string.IsNullOrEmpty(nomeDaCena))
        {
            AudioSFX.Instance?.TocarTrocarCena();
            SceneManager.LoadScene(nomeDaCena);
        }
        else
        {
            Debug.LogWarning("O nome da cena não foi definido no Inspector!");
        }
    }
}