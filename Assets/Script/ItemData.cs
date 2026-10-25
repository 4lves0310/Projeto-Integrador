using UnityEngine;

public class ItemData : MonoBehaviour
{
    public string itemId;
    [Min(1)] public int quantity = 1;
}
