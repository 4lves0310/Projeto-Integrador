using UnityEngine;

public class ItemColetavel : MonoBehaviour
{
    [Header("Configuração do Item")]
    public GameObject prefabItemUI; // O Prefab do item de UI (ex: item_Hamb)
    public int quantidade = 1;
    public KeyCode teclaColetar = KeyCode.E;

    private bool jogadorPerto = false;

    void Update()
    {
        if (jogadorPerto && Input.GetKeyDown(teclaColetar))
        {
            Coletar();
        }
    }

    private void Coletar()
    {
        // Procura o InventoryController da cena, ou o que persiste com DontDestroy.
        InventoryController inventario = InventoryController.FindActiveInventory();

        if (inventario != null && prefabItemUI != null)
        {
            // Usa a função oficial do seu InventoryController.cs
            bool adicionou = inventario.AddItemToInventory(prefabItemUI, quantidade);

            if (adicionou)
            {
                // Se coube no inventário, destrói o item do chão
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventário cheio ou item inválido!");
            }
        }
        else
        {
            Debug.LogWarning("Falta configurar o Prefab Item UI ou o InventoryController não foi encontrado.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            jogadorPerto = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            jogadorPerto = false;
    }
}