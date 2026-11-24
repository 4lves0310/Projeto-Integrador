using UnityEngine;

public class ItemColetavel : MonoBehaviour
{
    [Header("Item de UI para o Inventario")]
    public GameObject itemUIPrefab; // Ex: o prefab item_Hamb
    [Min(1)] public int quantidade = 1;

    [Header("Coleta")]
    public bool precisaApertarE = false;
    public KeyCode teclaColetar = KeyCode.E;
    public GameObject avisoInteracao; // Opcional: texto/icone "Aperte E"

    private bool jogadorPerto = false;
    private bool coletado = false;

    void Start()
    {
        if (avisoInteracao != null)
            avisoInteracao.SetActive(false);
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
            AudioSFX.Instance?.TocarColetar();

            if (avisoInteracao != null)
                avisoInteracao.SetActive(false);

            Destroy(gameObject);
        }
    }
}