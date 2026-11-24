using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public string mapBoundary;
    public string sceneName;
    public List<ItemSaveData> items = new List<ItemSaveData>();
    public List<string> collectedItems = new List<string>();
    public int questLevel = 1;
    public List<string> completedQuests = new List<string>();
    public ActiveQuestSaveData activeQuest;
}

[System.Serializable]
public class ActiveQuestSaveData
{
    public string idMissao;
    public TipoMissao tipo;
    public string idAlvo;
    public int progressoAtual;
    public int progressoNecessario;
    public StatusMissao status;
}

[System.Serializable]
public class ItemSaveData
{
    public string itemId;
    public int quantity;
    public int slotIndex;
}