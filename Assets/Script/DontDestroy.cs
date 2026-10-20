using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    private static readonly System.Collections.Generic.Dictionary<string, GameObject> instances
        = new();

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
        DontDestroyOnLoad(gameObject);
    }
}

