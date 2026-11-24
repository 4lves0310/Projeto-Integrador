using UnityEngine;
using UnityEngine.SceneManagement;

public class TrocarCenaPorta : MonoBehaviour
{
    [Header("Configurações de Cena")]
    public string nomeDaNovaCena;

    [Header("Interface")]
    public GameObject iconeInteracao;
    public KeyCode teclaInteracao = KeyCode.E;

    private bool jogadorPerto = false;

    void Start()
    {
        if (iconeInteracao != null)
            iconeInteracao.SetActive(false);
    }

    void Update()
    {
        if (jogadorPerto && Input.GetKeyDown(teclaInteracao))
        {
            CarregarNovaCena();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jogadorPerto = true;
            if (iconeInteracao != null)
                iconeInteracao.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jogadorPerto = false;
            if (iconeInteracao != null)
                iconeInteracao.SetActive(false);
        }
    }

    void CarregarNovaCena()
    {
        if (!string.IsNullOrEmpty(nomeDaNovaCena))
        {
            AudioSFX.Instance?.TocarTrocarCena();
            SceneManager.LoadScene(nomeDaNovaCena);
        }
        else
        {
            Debug.LogError("Esqueceu de digitar o nome da cena no Inspector da Porta!");
        }
    }
}