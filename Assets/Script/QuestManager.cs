using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Nivel do Jogador")]
    public int nivelAtual = 1;
    public TMP_Text textoNivel;

    [Header("Missao Atual")]
    public Missao missaoAtual;
    public List<string> missoesConcluidas = new List<string>();

    [Header("Interface")]
    public GameObject objetoDaBarra;
    public Image imagemDaBarra;
    public Sprite[] spritesDaBarra;
    public GameObject textoMissaoConcluida;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (objetoDaBarra != null) objetoDaBarra.SetActive(false);
        if (textoMissaoConcluida != null) textoMissaoConcluida.SetActive(false);

        AtualizarTextoNivel();
    }

    public bool MissaoFoiConcluida(string idMissao)
    {
        if (string.IsNullOrWhiteSpace(idMissao)) return true;
        return missoesConcluidas.Contains(idMissao);
    }

    public void AceitarMissao(Missao novaMissao)
    {
        missaoAtual = novaMissao;
        missaoAtual.status = StatusMissao.EmAndamento;
        missaoAtual.progressoAtual = 0;

        if (textoMissaoConcluida != null) textoMissaoConcluida.SetActive(false);
        if (objetoDaBarra != null) objetoDaBarra.SetActive(true);

        AtualizarVisualDaBarra();
        Debug.Log("Missao aceita: " + missaoAtual.idMissao + " | Alvo esperado: '" + missaoAtual.idAlvo + "'");
    }

    public void AtualizarProgresso(string idAlvo, int quantidade = 1)
    {
        if (missaoAtual == null || missaoAtual.status != StatusMissao.EmAndamento)
        {
            Debug.LogWarning("Nenhuma missao em andamento para atualizar!");
            return;
        }

        if (missaoAtual.idAlvo != idAlvo)
        {
            Debug.LogWarning("ID diferente! Missao espera '" + missaoAtual.idAlvo + "', mas o objeto mandou '" + idAlvo + "'");
            return;
        }

        missaoAtual.progressoAtual += quantidade;

        if (missaoAtual.progressoAtual >= missaoAtual.progressoNecessario)
        {
            missaoAtual.progressoAtual = missaoAtual.progressoNecessario;
            AtualizarVisualDaBarra();
            CompletarMissao();
        }
        else
        {
            AtualizarVisualDaBarra();
        }
    }

    private void AtualizarVisualDaBarra()
    {
        if (spritesDaBarra.Length == 0 || imagemDaBarra == null) return;

        if (missaoAtual.progressoNecessario <= 0)
        {
            imagemDaBarra.sprite = spritesDaBarra[0];
            return;
        }

        float porcentagem = (float)missaoAtual.progressoAtual / missaoAtual.progressoNecessario;
        int index = Mathf.RoundToInt(porcentagem * (spritesDaBarra.Length - 1));
        index = Mathf.Clamp(index, 0, spritesDaBarra.Length - 1);

        imagemDaBarra.sprite = spritesDaBarra[index];
    }

    private void CompletarMissao()
    {
        missaoAtual.status = StatusMissao.Concluida;

        if (!missoesConcluidas.Contains(missaoAtual.idMissao))
            missoesConcluidas.Add(missaoAtual.idMissao);

        Debug.Log("Missao concluida: " + missaoAtual.idMissao);

        SubirDeNivel();

        if (textoMissaoConcluida != null)
            textoMissaoConcluida.SetActive(true);

        Invoke("EsconderUI", 2f);
    }

    public void SubirDeNivel(int quantidade = 1)
    {
        nivelAtual += quantidade;
        AtualizarTextoNivel();
    }

    private void AtualizarTextoNivel()
    {
        if (textoNivel != null)
            textoNivel.text = "LVL: " + nivelAtual;
    }

    private void EsconderUI()
    {
        if (objetoDaBarra != null) objetoDaBarra.SetActive(false);
        if (textoMissaoConcluida != null) textoMissaoConcluida.SetActive(false);
    }
}