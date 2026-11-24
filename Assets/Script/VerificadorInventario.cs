using UnityEngine;

public class VerificadorInventario : MonoBehaviour
{
    [Tooltip("Arraste aqui o objeto que contem todos os Slots do inventario (ex: InventoryPanel)")]
    public Transform containerSlots;

    public int ContarItem(string itemIdAlvo)
    {
        if (string.IsNullOrWhiteSpace(itemIdAlvo)) return 0;

        Slot[] slots = ObterSlots();
        int total = 0;

        foreach (Slot slot in slots)
        {
            if (slot != null && slot.currentItem != null)
            {
                ItemData dados = slot.currentItem.GetComponent<ItemData>();
                if (dados != null && dados.itemId == itemIdAlvo)
                {
                    total += dados.quantity;
                }
            }
        }

        return total;
    }

    public bool TemItem(string itemIdAlvo, int quantidadeNecessaria = 1)
    {
        return ContarItem(itemIdAlvo) >= quantidadeNecessaria;
    }

    public bool ConsumirItem(string itemIdAlvo, int quantidadeNecessaria = 1)
    {
        if (!TemItem(itemIdAlvo, quantidadeNecessaria))
            return false;

        Slot[] slots = ObterSlots();
        int restante = quantidadeNecessaria;

        foreach (Slot slot in slots)
        {
            if (restante <= 0) break;

            if (slot != null && slot.currentItem != null)
            {
                ItemData dados = slot.currentItem.GetComponent<ItemData>();
                if (dados != null && dados.itemId == itemIdAlvo)
                {
                    if (dados.quantity > restante)
                    {
                        dados.quantity -= restante;
                        restante = 0;
                    }
                    else
                    {
                        restante -= dados.quantity;
                        Destroy(slot.currentItem);
                        slot.currentItem = null;
                    }
                }
            }
        }

        return true;
    }

    private Slot[] ObterSlots()
    {
        if (containerSlots != null)
            return containerSlots.GetComponentsInChildren<Slot>(true);

        return FindObjectsOfType<Slot>(true);
    }
}