using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Objeto temporário que sobrevive apenas durante o carregamento.
// Os SaveControllers e botões de cada cena continuam sendo objetos locais.
public class SaveLoadOperation : MonoBehaviour
{
    public static void Begin(string scene, SaveData data)
    {
        if (SaveController.IsLoading) return;
        SaveController.IsLoading = true;
        GameObject runner = new GameObject("Carregamento do save");
        DontDestroyOnLoad(runner);
        runner.AddComponent<SaveLoadOperation>().StartCoroutine(
            runner.GetComponent<SaveLoadOperation>().Load(scene, data));
    }

    private IEnumerator Load(string scene, SaveData data)
    {
        SaveController.Report("Carregando...");
        AsyncOperation operation = null;
        try { operation = SceneManager.LoadSceneAsync(scene); }
        catch (Exception error) { SaveController.Report("Falha ao abrir cena: " + error.Message, true); }
        if (operation == null)
        {
            Finish();
            yield break;
        }
        yield return operation;
        // Dá aos componentes ativos a oportunidade de executar Start.
        yield return null;
        string failureMessage = null;
        try
        {
            Time.timeScale = 1f;
            if (data == null)
            {
                SaveController.FindInventory().InitializeInventory();
                SaveController.CaptureAndWrite();
            }
            else SaveController.Restore(data);
        }
        catch (Exception error)
        {
            failureMessage = "Não foi possível iniciar o jogo: " + error.Message;
        }
        SaveController.IsLoading = false;
        if (failureMessage == null)
            SaveController.Report(data == null ? "Novo jogo iniciado e salvo!" : "Progresso carregado!");
        else SaveController.Report(failureMessage, true);
        Destroy(gameObject);
    }

    private void Finish()
    {
        SaveController.IsLoading = false;
        SaveController.Report("Carregamento interrompido.", true);
        Destroy(gameObject);
    }

    void OnDestroy() { SaveController.IsLoading = false; }
}
