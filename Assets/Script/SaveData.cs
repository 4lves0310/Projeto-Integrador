using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public string mapBoundary;
    public string sceneName;
    public List<ItemSaveData> items = new List<ItemSaveData>();
}

[System.Serializable]
public class ItemSaveData
{
    public string itemId;
    public int quantity;
    public int slotIndex;
}
