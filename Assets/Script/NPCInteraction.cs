using UnityEngine;
using UnityEngine.UI;
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
    public Image imagemRetratoUI;
    public Sprite spriteRetratoNPC;

    [TextArea(2, 5)]
    public string[] falasBloqueada;

    [TextArea(2, 5)]
    public string[] falas;

    [TextArea(2, 5)]
    public string[] falasEmAndamento;

    [TextArea(2, 5)]
    public string[] falasFaltaItem;

    [TextArea(2, 5)]
    public string[] falasConcluida;

    [Header("Missao")]
    public string idMissaoAnteriorNecessaria;
    public bool darMissaoAoFinalizar = false;
    public Missao missaoParaEntregar;
    public GameObject[] objetosParaAtivar;

    [Space(5)]
    public bool avancarMissaoAoFinalizar = false;
    public string idAlvoMissao;

    [Header("Entrega de Item na Missao (Opcional)")]
    public VerificadorInventario verificadorInventario;
    public string itemIdNecessario;
    public int quantidadeNecessaria = 1;
    public bool consumirItemAoEntregar = true;

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
    private bool bloqueadoPorFaltaDeItem;
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

        if (darMissaoAoFinalizar && missaoParaEntregar != null)
        {
            DefinirObjetosMissao(missaoParaEntregar.status != StatusMissao.Disponivel);
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

                foreach (Transform t in c.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.ToLower().Contains("dialogo") || t.name.ToLower().Contains("dialogue"))
                    {
                        painelDialogo = t.gameObject;
                        break;
                    }
                }
                if (painelDialogo != null) break;
            }
        }

        if (painelDialogo != null && textoDialogo == null)
            textoDialogo = painelDialogo.GetComponentInChildren<TMP_Text>(true);

        if (painelInteracao == null)
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
            {
                if (c.gameObject.scene != SceneManager.GetActiveScene())
                    continue;

                foreach (Transform t in c.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.ToLower().Contains("interacao") || t.name.ToLower().Contains("interact"))
                    {
                        painelInteracao = t.gameObject;
                        break;
                    }
                }
                if (painelInteracao != null) break;
            }
        }
    }

    private void AtivarHierarquia(GameObject alvo)
    {
        if (alvo == null) return;

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

    private bool DeveChecarItemAgora()
    {
        if (!avancarMissaoAoFinalizar || string.IsNullOrWhiteSpace(itemIdNecessario))
            return false;

        if (darMissaoAoFinalizar && missaoParaEntregar != null && missaoParaEntregar.status == StatusMissao.Disponivel)
            return false;

        return true;
    }

    private bool JogadorPossuiItemExigido()
    {
        if (!DeveChecarItemAgora())
            return true;

        if (verificadorInventario == null)
            verificadorInventario = FindFirstObjectByType<VerificadorInventario>();

        if (verificadorInventario == null)
            return false;

        return verificadorInventario.TemItem(itemIdNecessario, Mathf.Max(1, quantidadeNecessaria));
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
        if (!other.CompareTag("Player")) return;

        jogadorPerto = true;

        if (!dialogoAtivo && painelInteracao != null)
            painelInteracao.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

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
        bloqueadoPorFaltaDeItem = false;

        if (EstaBloqueado())
        {
            falasAtuais = falasBloqueada;
        }
        else if (darMissaoAoFinalizar && missaoParaEntregar != null && missaoParaEntregar.status == StatusMissao.Disponivel)
        {
            falasAtuais = falas;
        }
        else if (DeveChecarItemAgora() && !JogadorPossuiItemExigido())
        {
            bloqueadoPorFaltaDeItem = true;
            falasAtuais = (falasFaltaItem != null && falasFaltaItem.Length > 0) ? falasFaltaItem : falasEmAndamento;
        }
        else
        {
            falasAtuais = falas;

            if (darMissaoAoFinalizar && missaoParaEntregar != null)
            {
                if (missaoParaEntregar.status == StatusMissao.Concluida && falasConcluida != null && falasConcluida.Length > 0)
                    falasAtuais = falasConcluida;
                else if (missaoParaEntregar.status == StatusMissao.EmAndamento && falasEmAndamento != null && falasEmAndamento.Length > 0)
                    falasAtuais = falasEmAndamento;
            }
        }

        if (falasAtuais == null || falasAtuais.Length == 0)
        {
            Debug.LogWarning("Configure as falas do NPC: " + gameObject.name);
            return;
        }

        dialogoAtivo = true;
        indiceFala = 0;

        AtualizarRetratoUI();
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

        if (EstaBloqueado() || bloqueadoPorFaltaDeItem)
        {
            if (painelInteracao != null && jogadorPerto)
                painelInteracao.SetActive(true);
            return;
        }

        if (darMissaoAoFinalizar && missaoParaEntregar != null && missaoParaEntregar.status == StatusMissao.Disponivel)
        {
            if (missaoParaEntregar.progressoNecessario <= 0)
                missaoParaEntregar.progressoNecessario = 1;

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.AceitarMissao(missaoParaEntregar);
                DefinirObjetosMissao(true);
                AtualizarIconeMissao();
            }
            else
            {
                Debug.LogWarning("QuestManager nao encontrado na cena!");
            }

            if (painelInteracao != null && jogadorPerto)
                painelInteracao.SetActive(true);
            return;
        }

        if (DeveChecarItemAgora())
        {
            if (verificadorInventario == null)
                verificadorInventario = FindFirstObjectByType<VerificadorInventario>();

            if (consumirItemAoEntregar)
            {
                bool consumiu = verificadorInventario != null &&
                                verificadorInventario.ConsumirItem(itemIdNecessario, Mathf.Max(1, quantidadeNecessaria));

                if (!consumiu)
                {
                    if (painelInteracao != null && jogadorPerto)
                        painelInteracao.SetActive(true);
                    return;
                }
            }
            else if (!JogadorPossuiItemExigido())
            {
                if (painelInteracao != null && jogadorPerto)
                    painelInteracao.SetActive(true);
                return;
            }
        }

        if (itemRecompensa != null && !recompensaEntregue)
        {
            if (inventario == null)
                inventario = FindFirstObjectByType<InventoryController>();

            if (inventario != null)
            {
                bool recebeu = inventario.AddItemToInventory(itemRecompensa, quantidadeItem);
                if (!recebeu)
                {
                    dialogoAtivo = true;
                    if (painelDialogo != null)
                    {
                        AtivarHierarquia(painelDialogo);
                        painelDialogo.SetActive(true);
                    }
                    if (textoDialogo != null)
                        textoDialogo.text = "Inventario cheio! Libere espaco e pressione E.";

                    indiceFala = falasAtuais.Length;
                    return;
                }

                recompensaEntregue = true;
                AudioSFX.Instance?.TocarColetar();
            }
        }

        if (avancarMissaoAoFinalizar && !string.IsNullOrWhiteSpace(idAlvoMissao))
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.AtualizarProgresso(idAlvoMissao, 1);
        }

        if (trocarCenaAoFinalizar)
        {
            if (!string.IsNullOrWhiteSpace(nomeCenaDestino))
            {
                AudioSFX.Instance?.TocarTrocarCena();
                Time.timeScale = 1f;
                SceneManager.LoadScene(nomeCenaDestino);
                return;
            }
        }

        if (painelInteracao != null && jogadorPerto)
            painelInteracao.SetActive(true);
    }
}