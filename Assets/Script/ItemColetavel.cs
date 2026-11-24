using System.Collections.Generic;
using UnityEngine;

public class ItemColetavel : MonoBehaviour
{
    [Header("Identificacao Unica no Save")]
    public string coletavelId;

    [Header("Item de UI para o Inventario")]
    public GameObject itemUIPrefab;
    [Min(1)] public int quantidade = 1;

    [Header("Coleta")]
    public bool precisaApertarE = false;
    public KeyCode teclaColetar = KeyCode.E;
    public GameObject avisoInteracao;

    public static readonly HashSet<string> ColetadosNaSessao = new HashSet<string>();

    private bool jogadorPerto = false;
    private bool coletado = false;

    private string ObterIdEfetivo()
    {
        if (!string.IsNullOrWhiteSpace(coletavelId))
            return coletavelId.Trim();

        Vector3 pos = transform.position;
        return $"{gameObject.scene.name}_{gameObject.name}_{pos.x:F2}_{pos.y:F2}";
    }

    void Awake()
    {
        string id = ObterIdEfetivo();
        if (ColetadosNaSessao.Contains(id))
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (avisoInteracao != null)
            avisoInteracao.SetActive(false);

        string id = ObterIdEfetivo();
        if (ColetadosNaSessao.Contains(id))
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (coletado || !precisaApertarE || !jogadorPerto)
            return;

        if (Input.GetKeyDown(teclaColetar))
        {
            TentarColetar();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (coletado || !other.CompareTag("Player"))
            return;

        jogadorPerto = true;

        if (precisaApertarE)
        {
            if (avisoInteracao != null)
                avisoInteracao.SetActive(true);
        }
        else
        {
            TentarColetar();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        jogadorPerto = false;

        if (avisoInteracao != null)
            avisoInteracao.SetActive(false);
    }

    private void TentarColetar()
    {
        if (coletado || itemUIPrefab == null)
            return;

        InventoryController inventario = SaveController.FindInventory();

        if (inventario == null)
        {
            Debug.LogWarning("InventoryController nao encontrado.");
            return;
        }

        bool adicionou = inventario.AddItemToInventory(itemUIPrefab, quantidade);

        if (adicionou)
        {
            coletado = true;
            ColetadosNaSessao.Add(ObterIdEfetivo());

            AudioSFX.Instance?.TocarColetar();

            if (avisoInteracao != null)
                avisoInteracao.SetActive(false);

            Destroy(gameObject);
        }
    }

    public void MarcarComoColetado()
    {
        ColetadosNaSessao.Add(ObterIdEfetivo());
    }

    public bool JaFoiColetado()
    {
        return ColetadosNaSessao.Contains(ObterIdEfetivo());
    }
}