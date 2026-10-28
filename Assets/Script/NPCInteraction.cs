
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
    public string[] falas;

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

    void Start()
    {
        if (painelInteracao != null)
            painelInteracao.SetActive(false);

        if (painelDialogo != null)
            painelDialogo.SetActive(false);
    }

    void Update()
    {
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
        if (falas == null || falas.Length == 0)
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

        if (indiceFala < falas.Length)
            AtualizarTexto();
        else
            FinalizarDialogo();
    }

    private void AtualizarTexto()
    {
        if (textoDialogo != null)
            textoDialogo.text = falas[indiceFala];
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

                indiceFala = falas.Length;
                return;
            }

            recompensaEntregue = true;
            Debug.Log("Item recebido: " + itemRecompensa.name);
        }

        dialogoAtivo = false;

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

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
