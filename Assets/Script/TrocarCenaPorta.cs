using UnityEngine;
using UnityEngine.SceneManagement;

public class TrocarCenaPorta : MonoBehaviour
{
    [Header("Configurações de Cena")]
    public string nomeDaNovaCena; // Digite o nome exato da nova cena no Inspector

    [Header("Interface")]
    public GameObject iconeInteracao; // O texto ou imagem de "Aperte E"
    public KeyCode teclaInteracao = KeyCode.E;

    private bool jogadorPerto = false;

    void Start()
    {
        // Garante que o ícone comece invisível ao iniciar o jogo
        if (iconeInteracao != null)
            iconeInteracao.SetActive(false);
    }

    void Update()
    {
        // Se o jogador estiver perto e apertar a tecla configurada (E)
        if (jogadorPerto && Input.GetKeyDown(teclaInteracao))
        {
            CarregarNovaCena();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Quando algo entra na área verde, verifica se tem a tag Player
        if (collision.CompareTag("Player"))
        {
            jogadorPerto = true;
            if (iconeInteracao != null)
                iconeInteracao.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Quando o jogador sai da área verde
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
            SceneManager.LoadScene(nomeDaNovaCena);
        }
        else
        {
            Debug.LogError("Esqueceu de digitar o nome da cena no Inspector da Porta!");
        }
    }
}