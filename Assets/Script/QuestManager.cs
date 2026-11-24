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

        AudioSFX.Instance?.TocarMissaoAtualizada();
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
            AudioSFX.Instance?.TocarMissaoAtualizada();
            AtualizarVisualDaBarra();
        }
    }

    private void AtualizarVisualDaBarra()
    {
        if (spritesDaBarra.Length == 0 || imagemDaBarra == null || missaoAtual == null) return;

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
        if (missaoAtual != null)
        {
            missaoAtual.status = StatusMissao.Concluida;

            if (!string.IsNullOrWhiteSpace(missaoAtual.idMissao) && !missoesConcluidas.Contains(missaoAtual.idMissao))
                missoesConcluidas.Add(missaoAtual.idMissao);

            Debug.Log("Missao concluida: " + missaoAtual.idMissao);
        }

        AudioSFX.Instance?.TocarMissaoConcluida();
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

    public ActiveQuestSaveData ExportarMissaoAtual()
    {
        if (missaoAtual == null || string.IsNullOrWhiteSpace(missaoAtual.idMissao))
            return null;

        return new ActiveQuestSaveData
        {
            idMissao = missaoAtual.idMissao,
            tipo = missaoAtual.tipo,
            idAlvo = missaoAtual.idAlvo,
            progressoAtual = missaoAtual.progressoAtual,
            progressoNecessario = missaoAtual.progressoNecessario,
            status = missaoAtual.status
        };
    }

    public void RestaurarEstado(int nivel, List<string> concluidas, ActiveQuestSaveData ativa)
    {
        CancelInvoke("EsconderUI");

        nivelAtual = nivel > 0 ? nivel : 1;

        missoesConcluidas.Clear();
        if (concluidas != null)
        {
            foreach (string id in concluidas)
            {
                if (!string.IsNullOrWhiteSpace(id) && !missoesConcluidas.Contains(id))
                    missoesConcluidas.Add(id);
            }
        }

        if (ativa != null && !string.IsNullOrWhiteSpace(ativa.idMissao))
        {
            missaoAtual = new Missao(ativa.idMissao, ativa.tipo, ativa.idAlvo, ativa.progressoNecessario)
            {
                progressoAtual = ativa.progressoAtual,
                status = ativa.status
            };

            if (missaoAtual.status == StatusMissao.EmAndamento)
            {
                if (textoMissaoConcluida != null) textoMissaoConcluida.SetActive(false);
                if (objetoDaBarra != null) objetoDaBarra.SetActive(true);
                AtualizarVisualDaBarra();
            }
            else
            {
                if (objetoDaBarra != null) objetoDaBarra.SetActive(false);
                if (textoMissaoConcluida != null) textoMissaoConcluida.SetActive(false);
            }
        }
        else
        {
            missaoAtual = null;
            if (objetoDaBarra != null) objetoDaBarra.SetActive(false);
            if (textoMissaoConcluida != null) textoMissaoConcluida.SetActive(false);
        }

        AtualizarTextoNivel();
    }
}