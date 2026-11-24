using UnityEngine;

public class ItemData : MonoBehaviour
{
    public string itemId;
    [Min(1)] public int quantity = 1;

    [Header("Drop no Mundo")]
    public GameObject prefabNoChao; // Arraste aqui o prefab coletável do chão (ex: Coletavel_Hamburguer)
}