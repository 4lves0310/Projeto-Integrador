
using UnityEngine;
using UnityEngine.SceneManagement;

public class VoltarCena : MonoBehaviour
{
    private static string cenaAnterior = "";

    public void Voltar()
    {
        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(cenaAnterior))
        {
            SceneManager.LoadScene("Jogo");
            return;
        }

        string destino = cenaAnterior;
        cenaAnterior = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(destino);
    }

    public void TrocarCena(string nomeCena)
    {
        cenaAnterior = SceneManager.GetActiveScene().name;

        Time.timeScale = 1f;
        SceneManager.LoadScene(nomeCena);
    }
}
