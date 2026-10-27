
using UnityEngine;
using System.Collections.Generic;

public class DontDestroy : MonoBehaviour
{
    private static readonly Dictionary<string, GameObject> instances =
        new Dictionary<string, GameObject>();

    [SerializeField] private string uniqueId;

    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueId))
        {
            uniqueId = gameObject.name;
        }

        if (instances.ContainsKey(uniqueId))
        {
            Destroy(gameObject);
            return;
        }

        instances.Add(uniqueId, gameObject);

        // Se estiver dentro de outro objeto, preserva a hierarquia inteira.
        transform.SetParent(null);

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instances.ContainsKey(uniqueId) &&
            instances[uniqueId] == gameObject)
        {
            instances.Remove(uniqueId);
        }
    }
}
