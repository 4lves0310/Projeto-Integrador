using UnityEngine;

public class GatilhoMissao : MonoBehaviour
{
    [Header("Configuracao do Alvo")]
    public string idAlvo; // Tem que ser EXATAMENTE igual ao idAlvo do NPC
    public int quantidade = 1;

    [Header("Interacao")]
    public bool precisaApertarE = true;
    public bool destruirAoAtivar = true;
    public GameObject painelInteracao; // Arraste aqui o texto/painel "Aperte E"

    private bool jogadorPerto = false;
    private bool jaAtivado = false;

    void Start()
    {
        if (painelInteracao != null)
            painelInteracao.SetActive(false);
    }

    void Update()
    {
        if (precisaApertarE && jogadorPerto && !jaAtivado && Input.GetKeyDown(KeyCode.E))
        {
            AtivarGatilho();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorPerto = true;

            if (precisaApertarE && !jaAtivado && painelInteracao != null)
                painelInteracao.SetActive(true);

            if (!precisaApertarE && !jaAtivado)
                AtivarGatilho();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorPerto = false;

            if (painelInteracao != null)
                painelInteracao.SetActive(false);
        }
    }

    private void AtivarGatilho()
    {
        jaAtivado = true;

        if (painelInteracao != null)
            painelInteracao.SetActive(false);

        AudioSFX.Instance?.TocarInteragir();

        if (QuestManager.Instance != null)
            QuestManager.Instance.AtualizarProgresso(idAlvo, quantidade);
        else
            Debug.LogWarning("QuestManager nao encontrado na cena!");

        if (destruirAoAtivar)
            Destroy(gameObject);
    }
}