using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveController : MonoBehaviour
{
    [Header("Novo jogo")]
    [SerializeField] private string firstSceneName = "Jogo";

    [Header("Interface opcional")]
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text statusText;

    public static string SaveLocation =>
        Path.Combine(Application.persistentDataPath, "save.json");

    public static bool IsLoading { get; internal set; }

    public static event Action<string> StatusChanged;

    private static string lastStatus = "";

    private void OnEnable()
    {
        StatusChanged += UpdateInterface;
        UpdateInterface(lastStatus);
    }

    private void OnDisable()
    {
        StatusChanged -= UpdateInterface;
    }

    private void UpdateInterface(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        if (continueButton != null)
        {
            continueButton.interactable = HasSave() && !IsLoading;
        }
    }

    public static void Report(string message, bool error = false)
    {
        lastStatus = message;

        if (error)
        {
            Debug.LogError(message);
        }
        else
        {
            Debug.Log(message);
        }

        StatusChanged?.Invoke(message);
    }

    public static bool HasSave()
    {
        try
        {
            ReadSave();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void SaveGame()
    {
        if (IsLoading)
        {
            Report("Espere o carregamento terminar.");
            return;
        }

        try
        {
            CaptureAndWrite();
            Report("Jogo salvo!");
        }
        catch (Exception error)
        {
            Report("Não foi possível salvar: " + error.Message, true);
        }
    }

    public void NewGame()
    {
        if (IsLoading)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(firstSceneName))
        {
            Report(
                "Adicione a cena nas Build Settings: " + firstSceneName,
                true
            );
            return;
        }

        ItemColetavel.ColetadosNaSessao.Clear();
        SaveLoadOperation.Begin(firstSceneName, null);
    }

    public void ContinueGame()
    {
        LoadGame();
    }

    public void LoadGame()
    {
        if (IsLoading)
        {
            return;
        }

        try
        {
            SaveData data = ReadSave();

            string sceneName = string.IsNullOrEmpty(data.sceneName)
                ? firstSceneName
                : data.sceneName;

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                throw new InvalidOperationException(
                    "Adicione a cena nas Build Settings: " + sceneName
                );
            }

            AplicarColetadosAntesDaCena(data);
            SaveLoadOperation.Begin(sceneName, data);
        }
        catch (Exception error)
        {
            Report("Não foi possível carregar: " + error.Message, true);
        }
    }

    public void DeleteSave()
    {
        if (IsLoading)
        {
            return;
        }

        try
        {
            if (File.Exists(SaveLocation))
            {
                File.Delete(SaveLocation);
            }

            string temporary = SaveLocation + ".tmp";

            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }

            ItemColetavel.ColetadosNaSessao.Clear();
            Report("Save excluído!");
            UpdateInterface(lastStatus);
        }
        catch (Exception error)
        {
            Report("Não foi possível excluir: " + error.Message, true);
        }
    }

    public void ReturnToMenu()
    {
        if (IsLoading)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded("Menu"))
        {
            Report("Adicione a cena Menu nas Build Settings.", true);
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void SaveAndQuit()
    {
        if (IsLoading)
        {
            return;
        }

        try
        {
            CaptureAndWrite();
            Report("Jogo salvo!");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        catch (Exception error)
        {
            Report("Não foi possível salvar: " + error.Message, true);
        }
    }

    private static SaveData ReadSave()
    {
        if (!File.Exists(SaveLocation))
        {
            throw new FileNotFoundException(
                "Nenhum jogo salvo foi encontrado."
            );
        }

        string json = File.ReadAllText(SaveLocation);

        if (string.IsNullOrWhiteSpace(json) ||
            !json.TrimStart().StartsWith("{") ||
            !json.Contains("\"playerPosition\""))
        {
            throw new InvalidDataException("Arquivo de save inválido.");
        }

        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null ||
            !IsFinite(data.playerPosition.x) ||
            !IsFinite(data.playerPosition.y) ||
            !IsFinite(data.playerPosition.z))
        {
            throw new InvalidDataException("Posição inválida no save.");
        }

        if (!json.Contains("\"items\""))
        {
            data.items = null;
        }
        else if (data.items == null)
        {
            throw new InvalidDataException("Inventário inválido no save.");
        }

        if (data.collectedItems == null)
        {
            data.collectedItems = new List<string>();
        }

        if (data.completedQuests == null)
        {
            data.completedQuests = new List<string>();
        }

        if (data.questLevel <= 0)
        {
            data.questLevel = 1;
        }

        return data;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private static GameObject FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            throw new InvalidOperationException(
                "Jogador com a tag Player não encontrado."
            );
        }

        return player;
    }

    internal static InventoryController FindInventory()
    {
        InventoryController[] inventories =
            UnityEngine.Object.FindObjectsByType<InventoryController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (InventoryController inventory in inventories)
        {
            if (inventory != null &&
                inventory.gameObject.scene == SceneManager.GetActiveScene() &&
                inventory.isActiveAndEnabled)
            {
                return inventory;
            }
        }

        foreach (InventoryController inventory in inventories)
        {
            if (inventory != null && inventory.isActiveAndEnabled)
            {
                return inventory;
            }
        }

        throw new InvalidOperationException(
            "InventoryController não encontrado. " +
            "Verifique se o inventário existe e está habilitado."
        );
    }

    private static QuestManager FindQuestManager()
    {
        if (QuestManager.Instance != null)
            return QuestManager.Instance;

        QuestManager[] managers = UnityEngine.Object.FindObjectsByType<QuestManager>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (QuestManager manager in managers)
        {
            if (manager != null)
                return manager;
        }

        return null;
    }

    private static void AplicarColetadosAntesDaCena(SaveData data)
    {
        ItemColetavel.ColetadosNaSessao.Clear();
        if (data == null || data.collectedItems == null)
            return;

        foreach (string id in data.collectedItems)
        {
            if (!string.IsNullOrWhiteSpace(id))
                ItemColetavel.ColetadosNaSessao.Add(id.Trim());
        }
    }

    internal static void CaptureAndWrite()
    {
        GameObject player = FindPlayer();
        InventoryController inventory = FindInventory();
        QuestManager questManager = FindQuestManager();

        SaveData data = new SaveData
        {
            playerPosition = player.transform.position,
            sceneName = SceneManager.GetActiveScene().name,
            mapBoundary = "",
            items = inventory.GetInventoryData(),
            collectedItems = new List<string>(ItemColetavel.ColetadosNaSessao),
            questLevel = questManager != null ? questManager.nivelAtual : 1,
            completedQuests = questManager != null ? new List<string>(questManager.missoesConcluidas) : new List<string>(),
            activeQuest = questManager != null ? questManager.ExportarMissaoAtual() : null
        };

        string temporary = SaveLocation + ".tmp";

        File.WriteAllText(
            temporary,
            JsonUtility.ToJson(data, true)
        );

        if (File.Exists(SaveLocation))
        {
            File.Replace(temporary, SaveLocation, null);
        }
        else
        {
            File.Move(temporary, SaveLocation);
        }
    }

    internal static void Restore(SaveData data)
    {
        AplicarColetadosAntesDaCena(data);

        GameObject player = FindPlayer();
        InventoryController inventory = FindInventory();
        QuestManager questManager = FindQuestManager();

        inventory.InitializeInventory();

        if (data.items != null)
        {
            inventory.LoadInventoryData(data.items);
        }

        if (questManager != null)
        {
            questManager.RestaurarEstado(data.questLevel, data.completedQuests, data.activeQuest);
        }

        ItemColetavel[] coletaveisNaCena = UnityEngine.Object.FindObjectsByType<ItemColetavel>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (ItemColetavel coletavel in coletaveisNaCena)
        {
            if (coletavel != null && coletavel.JaFoiColetado())
            {
                coletavel.gameObject.SetActive(false);
                Destroy(coletavel.gameObject);
            }
        }

        player.transform.position = data.playerPosition;

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();

        if (body != null)
        {
            body.position = new Vector2(
                data.playerPosition.x,
                data.playerPosition.y
            );

            body.velocity = Vector2.zero;
        }
    }
}