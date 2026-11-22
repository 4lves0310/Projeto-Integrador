using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class NPCInteraction : MonoBehaviour
{
    [Header("Interacao")]
    public GameObject painelInteracao;
    public KeyCode teclaInteragir = KeyCode.E;

    [Header("Dialogo")]
    public GameObject painelDialogo;
    public TMP_Text textoDialogo;

    [TextArea(2, 5)]
    public string[] falas; // Falas iniciais (antes de aceitar a missao)

    [TextArea(2, 5)]
    public string[] falasEmAndamento; // Opcional: enquanto faz a missao

    [TextArea(2, 5)]
    public string[] falasConcluida; // Falas depois que concluiu a missao

    [Header("Missao")]
    public bool darMissaoAoFinalizar = false;
    public Missao missaoParaEntregar;

    [Space(5)]
    public bool avancarMissaoAoFinalizar = false;
    public string idAlvoMissao;

    [Header("Icone em cima do NPC")]
    public SpriteRenderer iconeMissao; // SpriteRenderer filho em cima da cabeca do NPC
    public Sprite spriteDisponivel;    // Ex: ponto de exclamacao (!)
    public Sprite spriteEmAndamento;   // Ex: interrogacao cinza (ou deixe vazio para esconder)
    public Sprite spriteConcluida;     // Ex: check verde ou estrela

    [Header("Recompensa")]
    public InventoryController inventario;
    public GameObject itemRecompensa;
    public int quantidadeItem = 1;

    [Header("Troca de Cena")]
    public bool trocarCenaAoFinalizar = false;
    public string nomeCenaDestino;

    private bool jogadorPerto;
    private bool dialogoAtivo;
    private bool recompensaEntregue;
    private int indiceFala;
    private string[] falasAtuais;

    void Start()
    {
        if (painelInteracao != null)
            painelInteracao.SetActive(false);

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        AtualizarIconeMissao();
    }

    void Update()
    {
        AtualizarIconeMissao();

        if (!jogadorPerto)
            return;

        if (Input.GetKeyDown(teclaInteragir))
        {
            if (!dialogoAtivo)
                IniciarDialogo();
            else
                ProximaFala();
        }
    }

    private void AtualizarIconeMissao()
    {
        if (iconeMissao == null || !darMissaoAoFinalizar || missaoParaEntregar == null)
            return;

        Sprite spriteEscolhido = null;

        switch (missaoParaEntregar.status)
        {
            case StatusMissao.Disponivel:
                spriteEscolhido = spriteDisponivel;
                break;
            case StatusMissao.EmAndamento:
                spriteEscolhido = spriteEmAndamento;
                break;
            case StatusMissao.Concluida:
                spriteEscolhido = spriteConcluida;
                break;
        }

        if (spriteEscolhido != null)
        {
            iconeMissao.sprite = spriteEscolhido;
            iconeMissao.enabled = true;
        }
        else
        {
            // Se voce deixar algum sprite vazio no Inspector, o icone fica invisivel naquele estado
            iconeMissao.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        jogadorPerto = true;

        if (!dialogoAtivo && painelInteracao != null)
            painelInteracao.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        jogadorPerto = false;

        if (painelInteracao != null)
            painelInteracao.SetActive(false);

        if (dialogoAtivo)
        {
            dialogoAtivo = false;

            if (painelDialogo != null)
                painelDialogo.SetActive(false);
        }
    }

    private void IniciarDialogo()
    {
        // Escolhe qual lista de falas usar conforme o status da missao
        falasAtuais = falas;

        if (darMissaoAoFinalizar && missaoParaEntregar != null)
        {
            if (missaoParaEntregar.status == StatusMissao.Concluida && falasConcluida != null && falasConcluida.Length > 0)
            {
                falasAtuais = falasConcluida;
            }
            else if (missaoParaEntregar.status == StatusMissao.EmAndamento && falasEmAndamento != null && falasEmAndamento.Length > 0)
            {
                falasAtuais = falasEmAndamento;
            }
        }

        if (falasAtuais == null || falasAtuais.Length == 0)
        {
            Debug.LogWarning("Configure as falas do NPC.");
            return;
        }

        dialogoAtivo = true;
        indiceFala = 0;

        if (painelInteracao != null)
            painelInteracao.SetActive(false);

        if (painelDialogo != null)
            painelDialogo.SetActive(true);

        AtualizarTexto();
    }

    private void ProximaFala()
    {
        indiceFala++;

        if (indiceFala < falasAtuais.Length)
            AtualizarTexto();
        else
            FinalizarDialogo();
    }

    private void AtualizarTexto()
    {
        if (textoDialogo != null)
            textoDialogo.text = falasAtuais[indiceFala];
    }

    private void FinalizarDialogo()
    {
        if (itemRecompensa != null && !recompensaEntregue)
        {
            if (inventario == null)
            {
                Debug.LogWarning("Configure o inventario do NPC.");
                return;
            }

            bool recebeu = inventario.AddItemToInventory(
                itemRecompensa, quantidadeItem);

            if (!recebeu)
            {
                if (textoDialogo != null)
                {
                    textoDialogo.text =
                        "Inventario cheio ou item invalido! " +
                        "Libere espaco e pressione E para tentar novamente.";
                }

                indiceFala = falasAtuais.Length;
                return;
            }

            recompensaEntregue = true;
            Debug.Log("Item recebido: " + itemRecompensa.name);
        }

        dialogoAtivo = false;

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        // --- SISTEMA DE MISSOES ---
        if (darMissaoAoFinalizar && missaoParaEntregar != null && missaoParaEntregar.status == StatusMissao.Disponivel)
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.AceitarMissao(missaoParaEntregar);
            else
                Debug.LogWarning("QuestManager nao encontrado na cena!");
        }

        if (avancarMissaoAoFinalizar && !string.IsNullOrWhiteSpace(idAlvoMissao))
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.AtualizarProgresso(idAlvoMissao, 1);
        }
        // --------------------------

        if (trocarCenaAoFinalizar)
        {
            if (string.IsNullOrWhiteSpace(nomeCenaDestino))
            {
                Debug.LogWarning("Configure o nome da cena de destino.");
                if (painelInteracao != null && jogadorPerto)
                    painelInteracao.SetActive(true);
                return;
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(nomeCenaDestino);
            return;
        }

        if (painelInteracao != null && jogadorPerto)
            painelInteracao.SetActive(true);

        Debug.Log("Dialogo concluido!");
    }
}