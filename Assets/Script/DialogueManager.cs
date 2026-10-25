using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    private string[] currentLines;
    private int currentLineIndex;

    public void StartDialogue(string[] lines)
    {
        currentLines = lines;
        currentLineIndex = 0;
        dialogueUI.SetActive(true);
        ShowLine();
    }

    void Update()
    {
        // Avança o diálogo com o clique esquerdo do mouse ou toque na tela
        if (dialogueUI.activeSelf && Input.GetMouseButtonDown(0))
        {
            AdvanceDialogue();
        }
    }

    private void AdvanceDialogue()
    {
        currentLineIndex++;
        if (currentLineIndex < currentLines.Length)
            ShowLine();
        else
            EndDialogue();
    }

    private void ShowLine()
    {
        StopAllCoroutines();

        // Insere o texto completo para o painel estabilizar o seu tamanho final
        dialogueText.text = currentLines[currentLineIndex];
        Canvas.ForceUpdateCanvases();

        StartCoroutine(TypeSentence());
    }

    private IEnumerator TypeSentence()
    {
        // Define a visibilidade inicial para zero letras (mas a caixa mantém o tamanho)
        dialogueText.maxVisibleCharacters = 0;

        int totalCharacters = dialogueText.text.Length;

        for (int i = 0; i <= totalCharacters; i++)
        {
            // Revela uma letra de cada vez
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(0.02f);
        }
    }

    private void EndDialogue()
    {
        dialogueUI.SetActive(false);
    }
}