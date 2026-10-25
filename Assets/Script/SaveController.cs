using System;
using System.IO;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveController : MonoBehaviour
{
    [Header("Novo jogo")]
    [SerializeField] private string firstSceneName = "SampleScene";
    [Header("Interface (opcional)")]
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text statusText;

    public static string SaveLocation => Path.Combine(Application.persistentDataPath, "save.json");
    public static bool IsLoading { get; internal set; }
    public static event Action<string> StatusChanged;
    private static string lastStatus = "";

    void OnEnable()
    {
        StatusChanged += UpdateInterface;
        UpdateInterface(lastStatus);
    }

    void OnDisable() { StatusChanged -= UpdateInterface; }

    private void UpdateInterface(string message)
    {
        if (statusText != null) statusText.text = message;
        if (continueButton != null) continueButton.interactable = HasSave() && !IsLoading;
    }

    public static bool HasSave()
    {
        try { ReadSave(); return true; }
        catch { return false; }
    }

    public static void Report(string message, bool error = false)
    {
        lastStatus = message;
        if (error) Debug.LogError(message);
        else Debug.Log(message);
        StatusChanged?.Invoke(message);
    }

    public void SaveGame()
    {
        if (IsLoading) return;
        try
        {
            CaptureAndWrite();
            Report("Jogo salvo!");
        }
        catch (Exception error) { Report("Não foi possível salvar: " + error.Message, true); }
    }

    public void NewGame()
    {
        if (IsLoading) return;
        if (!Application.CanStreamedLevelBeLoaded(firstSceneName))
        {
            Report("Adicione a cena inicial nas Build Settings: " + firstSceneName, true);
            return;
        }
        // O save anterior só é substituído quando o novo estado estiver pronto.
        SaveLoadOperation.Begin(firstSceneName, null);
    }

    public void ContinueGame() { LoadGame(); }

    public void LoadGame()
    {
        if (IsLoading) return;
        try
        {
            SaveData data = ReadSave();
            string scene = string.IsNullOrEmpty(data.sceneName)
                ? firstSceneName : data.sceneName;
            if (!Application.CanStreamedLevelBeLoaded(scene))
                throw new InvalidOperationException("Adicione a cena nas Build Settings: " + scene);
            SaveLoadOperation.Begin(scene, data);
        }
        catch (Exception error) { Report("Não foi possível continuar: " + error.Message, true); }
    }

    public void DeleteSave()
    {
        if (IsLoading) return;
        try
        {
            if (File.Exists(SaveLocation)) File.Delete(SaveLocation);
            if (File.Exists(SaveLocation + ".tmp")) File.Delete(SaveLocation + ".tmp");
            Report("Save excluído. O progresso em execução não foi reiniciado.");
        }
        catch (Exception error) { Report("Não foi possível excluir: " + error.Message, true); }
    }

    private static SaveData ReadSave()
    {
        if (!File.Exists(SaveLocation))
            throw new FileNotFoundException("Nenhum jogo salvo foi encontrado.");
        string json = File.ReadAllText(SaveLocation);
        // Exige os campos que também existiam nos saves antigos.
        if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{") ||
            !json.Contains("\"playerPosition\"") || !json.Contains("\"mapBoundary\""))
            throw new InvalidDataException("Arquivo de salvamento inválido.");
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        if (data == null || string.IsNullOrWhiteSpace(data.mapBoundary) ||
            !Finite(data.playerPosition.x) || !Finite(data.playerPosition.y) || !Finite(data.playerPosition.z))
            throw new InvalidDataException("Dados de salvamento inválidos.");
        // Saves antigos não possuem inventário: preserve os itens iniciais.
        if (!json.Contains("\"items\"")) data.items = null;
        else if (data.items == null) throw new InvalidDataException("Inventário inválido no save.");
        return data;
    }

    private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }

    internal static InventoryController FindInventory()
    {
        Scene scene = SceneManager.GetActiveScene();
        foreach (InventoryController inventory in FindObjectsByType<InventoryController>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (inventory.gameObject.scene == scene) return inventory;
        throw new InvalidOperationException("A cena precisa de um InventoryController configurado.");
    }

    private static GameObject FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) throw new InvalidOperationException("Jogador com tag Player não encontrado.");
        return player;
    }

    private static CinemachineConfiner FindCamera()
    {
        foreach (CinemachineConfiner camera in FindObjectsByType<CinemachineConfiner>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (camera.gameObject.scene == SceneManager.GetActiveScene()) return camera;
        throw new InvalidOperationException("CinemachineConfiner não encontrado na cena.");
    }

    internal static void CaptureAndWrite()
    {
        GameObject player = FindPlayer();
        CinemachineConfiner camera = FindCamera();
        if (camera.m_BoundingShape2D == null)
            throw new InvalidOperationException("Configure o limite 2D da câmera.");
        SaveData data = new SaveData {
            playerPosition = player.transform.position,
            mapBoundary = camera.m_BoundingShape2D.gameObject.name,
            sceneName = SceneManager.GetActiveScene().name,
            items = FindInventory().GetInventoryData()
        };
        // Escreve primeiro um temporário para reduzir risco de truncar o save.
        string temporary = SaveLocation + ".tmp";
        File.WriteAllText(temporary, JsonUtility.ToJson(data, true));
        if (File.Exists(SaveLocation)) File.Replace(temporary, SaveLocation, null);
        else File.Move(temporary, SaveLocation);
    }

    internal static void Restore(SaveData data)
    {
        GameObject player = FindPlayer();
        CinemachineConfiner camera = FindCamera();
        Collider2D boundary = null;
        foreach (Collider2D candidate in FindObjectsByType<Collider2D>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (candidate.gameObject.scene == SceneManager.GetActiveScene() &&
                candidate.gameObject.name == data.mapBoundary &&
                (candidate is PolygonCollider2D || candidate is CompositeCollider2D))
            {
                if (boundary != null)
                    throw new InvalidOperationException("Há limites de mapa com nomes repetidos.");
                boundary = candidate;
            }
        }
        if (boundary == null) throw new InvalidOperationException("Limite do mapa não encontrado: " + data.mapBoundary);
        InventoryController inventory = FindInventory();
        inventory.InitializeInventory();
        if (data.items != null) inventory.ValidateInventoryData(data.items);
        if (data.items != null) inventory.LoadInventoryData(data.items);
        player.transform.position = data.playerPosition;
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.position = data.playerPosition;
            body.velocity = Vector2.zero;
        }
        camera.m_BoundingShape2D = boundary;
        camera.InvalidatePathCache();
    }
}
