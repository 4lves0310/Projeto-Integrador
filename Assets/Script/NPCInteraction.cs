using UnityEngine;
using UnityEngine.UI; // Necess�rio para controlar componentes Image da UI
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
    public Image imagemRetratoUI;      // Arraste aqui o objeto Image da UI onde aparece a foto
    public Sprite spriteRetratoNPC;    // Arraste aqui o Sprite (rosto/arte) deste NPC

    [TextArea(2, 5)]
    public string[] falasBloqueada;

    [TextArea(2, 5)]
    public string[] falas; // Falas iniciais (missao disponivel)

    [TextArea(2, 5)]
    public string[] falasEmAndamento; // Falas enquanto faz a missao

    [TextArea(2, 5)]
    public string[] falasConcluida; // Falas apos concluir a missao

    [Header("Missao")]
    public string idMissaoAnteriorNecessaria;
    public bool darMissaoAoFinalizar = false;
    public Missao missaoParaEntregar;
    public GameObject[] objetosParaAtivar; // Objetos escondidos no Start e ativados ao aceitar a missao

    [Space(5)]
    public bool avancarMissaoAoFinalizar = false;
    public string idAlvoMissao;

    [Header("Icone em cima do NPC")]
    public SpriteRenderer iconeMissao;
    public Sprite spriteDisponivel;
    public Sprite spriteEmAndamento;
    public Sprite spriteConcluida;

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
        GarantirReferenciasUI();

        if (painelInteracao != null)
            painelInteracao.SetActive(false);

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        SincronizarEstadoMissao();

        if (darMissaoAoFinalizar && missaoParaEntregar != null && missaoParaEntregar.status == StatusMissao.Disponivel)
        {
            DefinirObjetosMissao(false);
        }
        else if (darMissaoAoFinalizar && missaoParaEntregar != null && missaoParaEntregar.status != StatusMissao.Disponivel)
        {
            DefinirObjetosMissao(true);
        }

        AtualizarIconeMissao();
    }

    void Update()
    {
        SincronizarEstadoMissao();
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

    private void GarantirReferenciasUI()
    {
        if (painelDialogo == null)
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
            {
                if (c.gameObject.scene != SceneManager.GetActiveScene())
                    continue;

                Transform[] filhos = c.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in filhos)
                {
                    if (t.name.ToLower().Contains("dialogo") || t.name.ToLower().Contains("dialogue"))
                    {
                        painelDialogo = t.gameObject;
                        break;
                    }
                }

                if (painelDialogo != null)
                    break;
            }
        }

        if (painelDialogo != null && textoDialogo == null)
        {
            textoDialogo = painelDialogo.GetComponentInChildren<TMP_Text>(true);
        }

        if (painelInteracao == null)
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
            {
                if (c.gameObject.scene != SceneManager.GetActiveScene())
                    continue;

                Transform[] filhos = c.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in filhos)
                {
                    if (t.name.ToLower().Contains("interacao") || t.name.ToLower().Contains("interact"))
                    {
                        painelInteracao = t.gameObject;
                        break;
                    }
                }

                if (painelInteracao != null)
                    break;
            }
        }
    }

    private void AtivarHierarquia(GameObject alvo)
    {
        if (alvo == null)
            return;

        Transform atual = alvo.transform;
        while (atual != null)
        {
            if (!atual.gameObject.activeSelf)
                atual.gameObject.SetActive(true);
            atual = atual.parent;
        }
    }

    private void SincronizarEstadoMissao()
    {
        if (!darMissaoAoFinalizar || missaoParaEntregar == null || string.IsNullOrWhiteSpace(missaoParaEntregar.idMissao))
            return;

        if (QuestManager.Instance == null)
            return;

        if (QuestManager.Instance.MissaoFoiConcluida(missaoParaEntregar.idMissao))
        {
            missaoParaEntregar.status = StatusMissao.Concluida;
            if (QuestManager.Instance.missaoAtual != null && QuestManager.Instance.missaoAtual.idMissao == missaoParaEntregar.idMissao)
                missaoParaEntregar.progressoAtual = QuestManager.Instance.missaoAtual.progressoAtual;
        }
        else if (QuestManager.Instance.missaoAtual != null && QuestManager.Instance.missaoAtual.idMissao == missaoParaEntregar.idMissao)
        {
            missaoParaEntregar.status = QuestManager.Instance.missaoAtual.status;
            missaoParaEntregar.progressoAtual = QuestManager.Instance.missaoAtual.progressoAtual;
        }
    }

    private bool EstaBloqueado()
    {
        if (string.IsNullOrWhiteSpace(idMissaoAnteriorNecessaria))
            return false;

        if (QuestManager.Instance == null)
            return true;

        return !QuestManager.Instance.MissaoFoiConcluida(idMissaoAnteriorNecessaria);
    }

    private void DefinirObjetosMissao(bool ativo)
    {
        if (objetosParaAtivar == null) return;

        foreach (GameObject obj in objetosParaAtivar)
        {
            if (obj != null)
                obj.SetActive(ativo);
        }
    }

    private void AtualizarIconeMissao()
    {
        if (iconeMissao == null || !darMissaoAoFinalizar || missaoParaEntregar == null)
            return;

        if (EstaBloqueado())
        {
            iconeMissao.enabled = false;
            return;
        }

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
        GarantirReferenciasUI();
        SincronizarEstadoMissao();

        if (EstaBloqueado())
        {
            falasAtuais = falasBloqueada;
        }
        else
        {
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
        }

        if (falasAtuais == null || falasAtuais.Length == 0)
        {
            Debug.LogWarning("Configure as falas do NPC.");
            return;
        }

        dialogoAtivo = true;
        indiceFala = 0;

        AudioSFX.Instance?.TocarInteragir();

        if (painelInteracao != null)
            painelInteracao.SetActive(false);

        if (painelDialogo != null)
        {
            AtivarHierarquia(painelDialogo);
            painelDialogo.SetActive(true);
        }

        AtualizarTexto();
    }

    // M�todo p�blico caso voc� queira trocar o sprite por outro script ou evento
    public void AtualizarRetratoUI()
    {
        if (imagemRetratoUI == null) return;

        if (spriteRetratoNPC != null)
        {
            imagemRetratoUI.sprite = spriteRetratoNPC;
            imagemRetratoUI.enabled = true;
        }
        else
        {
            // Se voc� n�o colocar nenhum sprite nesse NPC, a imagem fica oculta
            imagemRetratoUI.enabled = false;
        }
    }

    private void ProximaFala()
    {
        indiceFala++;

        if (indiceFala < falasAtuais.Length)
        {
            AudioSFX.Instance?.TocarInteragir();
            AtualizarTexto();
        }
        else
        {
            FinalizarDialogo();
        }
    }

    private void AtualizarTexto()
    {
        if (textoDialogo != null)
        {
            AtivarHierarquia(textoDialogo.gameObject);
            textoDialogo.gameObject.SetActive(true);
            textoDialogo.enabled = true;
            textoDialogo.text = falasAtuais[indiceFala];
        }
    }

    private void FinalizarDialogo()
    {
        dialogoAtivo = false;

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        if (EstaBloqueado())
        {
            if (painelInteracao != null && jogadorPerto)
                painelInteracao.SetActive(true);
            return;
        }

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
                dialogoAtivo = true;
                if (painelDialogo != null)
                {
                    AtivarHierarquia(painelDialogo);
                    painelDialogo.SetActive(true);
                }

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
            AudioSFX.Instance?.TocarColetar();
            Debug.Log("Item recebido: " + itemRecompensa.name);
        }

        if (darMissaoAoFinalizar && missaoParaEntregar != null && missaoParaEntregar.status == StatusMissao.Disponivel)
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.AceitarMissao(missaoParaEntregar);
                DefinirObjetosMissao(true);
            }
            else
                Debug.LogWarning("QuestManager nao encontrado na cena!");
        }

        if (avancarMissaoAoFinalizar && !string.IsNullOrWhiteSpace(idAlvoMissao))
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.AtualizarProgresso(idAlvoMissao, 1);
        }

        if (trocarCenaAoFinalizar)
        {
            if (string.IsNullOrWhiteSpace(nomeCenaDestino))
            {
                Debug.LogWarning("Configure o nome da cena de destino.");
                if (painelInteracao != null && jogadorPerto)
                    painelInteracao.SetActive(true);
                return;
            }

            AudioSFX.Instance?.TocarTrocarCena();
            Time.timeScale = 1f;
            SceneManager.LoadScene(nomeCenaDestino);
            return;
        }

        if (painelInteracao != null && jogadorPerto)
            painelInteracao.SetActive(true);

        Debug.Log("Dialogo concluido!");
    }
}