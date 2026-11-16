using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    private readonly List<Slot> slots = new List<Slot>();
    private bool initialized;

    void Start()
    {
        try { InitializeInventory(); }
        catch (Exception error) { Debug.LogError(error.Message, this); }
    }

    public static InventoryController FindActiveInventory()
    {
        InventoryController[] inventories = FindObjectsByType<InventoryController>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        InventoryController fallback = null;
        foreach (InventoryController inventory in inventories)
        {
            if (inventory == null || !inventory.isActiveAndEnabled)
                continue;
            if (!IsInLoadedScene(inventory.gameObject))
                continue;

            if (inventory.gameObject.scene == SceneManager.GetActiveScene())
                return inventory;

            if (fallback == null)
                fallback = inventory;
        }

        return fallback;
    }

    // Funciona também quando o painel está inativo e seu Start ainda não rodou.
    public void InitializeInventory()
    {
        ResolveInventoryPanel();
        if (initialized) return;
        if (inventoryPanel == null || slotPrefab == null || slotCount <= 0 ||
            slotPrefab.GetComponent<Slot>() == null)
            throw new InvalidOperationException("Configure painel, prefab e quantidade de slots.");
        if (!IsInLoadedScene(inventoryPanel))
            throw new InvalidOperationException(
                "O Inventory Panel aponta para um prefab, não para o InventoryPage da cena.");
        ValidateCatalog();
        for (int i = 0; i < slotCount; i++)
            slots.Add(Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<Slot>());
        initialized = true;
        // Mantém os itens iniciais do projeto original.
        for (int i = 0; i < slots.Count && i < itemPrefabs.Length; i++)
            if (itemPrefabs[i] != null)
                CreateItem(itemPrefabs[i], slots[i], itemPrefabs[i].GetComponent<ItemData>().quantity);
    }

    private void ResolveInventoryPanel()
    {
        if (IsInLoadedScene(inventoryPanel))
            return;

        if (initialized)
        {
            slots.Clear();
            initialized = false;
        }

        inventoryPanel = FindSceneInventoryPage();
    }

    private static GameObject FindSceneInventoryPage()
    {
        GridLayoutGroup[] groups = FindObjectsByType<GridLayoutGroup>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        GameObject fallback = null;
        foreach (GridLayoutGroup group in groups)
        {
            if (group == null || group.gameObject.name != "InventoryPage")
                continue;
            if (!IsInLoadedScene(group.gameObject))
                continue;

            if (group.gameObject.scene == SceneManager.GetActiveScene())
                return group.gameObject;

            if (fallback == null)
                fallback = group.gameObject;
        }

        return fallback;
    }

    private static bool IsInLoadedScene(GameObject obj)
    {
        return obj != null && obj.scene.IsValid() && obj.scene.isLoaded;
    }

    private void ValidateCatalog()
    {
        if (itemPrefabs == null)
            throw new InvalidOperationException("Configure a lista Item Prefabs.");
        HashSet<string> ids = new HashSet<string>();
        foreach (GameObject prefab in itemPrefabs)
        {
            if (prefab == null) continue;
            ItemData data = prefab.GetComponent<ItemData>();
            if (data == null || string.IsNullOrWhiteSpace(data.itemId) || data.quantity <= 0 ||
                prefab.GetComponent<RectTransform>() == null)
                throw new InvalidOperationException("Configure ItemData e RectTransform em " + prefab.name);
            if (!ids.Add(data.itemId))
                throw new InvalidOperationException("ID de item repetido: " + data.itemId);
        }
    }

    public GameObject FindItemPrefab(string itemId)
    {
        foreach (GameObject prefab in itemPrefabs)
        {
            if (prefab == null) continue;
            ItemData data = prefab.GetComponent<ItemData>();
            if (data != null && data.itemId == itemId) return prefab;
        }
        return null;
    }

    public List<ItemSaveData> GetInventoryData()
    {
        InitializeInventory();
        List<ItemSaveData> items = new List<ItemSaveData>();
        for (int i = 0; i < slots.Count; i++)
        {
            GameObject item = slots[i].currentItem;
            if (item == null) continue;
            if (item.transform.parent != slots[i].transform)
                throw new InvalidOperationException("Solte o item antes de salvar.");
            ItemData data = item.GetComponent<ItemData>();
            if (data == null || data.quantity <= 0 || FindItemPrefab(data.itemId) == null)
                throw new InvalidOperationException("Item sem cadastro válido: " + item.name);
            items.Add(new ItemSaveData
            {
                itemId = data.itemId,
                quantity = data.quantity,
                slotIndex = i
            });
        }
        return items;
    }

    public void ValidateInventoryData(List<ItemSaveData> items)
    {
        InitializeInventory();
        if (items == null) throw new InvalidOperationException("Lista de itens ausente no save.");
        HashSet<int> occupied = new HashSet<int>();
        foreach (ItemSaveData item in items)
        {
            if (item == null || item.slotIndex < 0 || item.slotIndex >= slots.Count ||
                item.quantity <= 0 || !occupied.Add(item.slotIndex) || FindItemPrefab(item.itemId) == null)
                throw new InvalidOperationException("O save contém um item ou slot inválido.");
        }
    }

    public void LoadInventoryData(List<ItemSaveData> items)
    {
        // Valida tudo antes de remover os itens atuais.
        ValidateInventoryData(items);
        foreach (Slot slot in slots)
        {
            if (slot.currentItem == null) continue;
            slot.currentItem.SetActive(false);
            Destroy(slot.currentItem);
            slot.currentItem = null;
        }
        foreach (ItemSaveData item in items)
            CreateItem(FindItemPrefab(item.itemId), slots[item.slotIndex], item.quantity);
    }

    private void CreateItem(GameObject prefab, Slot slot, int quantity)
    {
        GameObject item = Instantiate(prefab, slot.transform);
        item.GetComponent<ItemData>().quantity = quantity;
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        Vector3 position = rect.localPosition;
        rect.localPosition = new Vector3(position.x, position.y, 0f);
        rect.localScale = Vector3.one;
        slot.currentItem = item;
    }

    public bool AddItemToInventory(GameObject prefab, int quantity)
    {
        InitializeInventory();

        if (prefab == null || quantity <= 0)
            return false;

        ItemData data = prefab.GetComponent<ItemData>();
        GameObject uiPrefab = data != null ? FindItemPrefab(data.itemId) : null;

        if (uiPrefab == null)
            return false;

        foreach (Slot slot in slots)
        {
            if (slot.currentItem == null)
            {
                CreateItem(uiPrefab, slot, quantity);
                return true;
            }
        }

        Debug.LogWarning("Inventario cheio! Nao foi possivel receber o item.");
        return false;
    }
}
