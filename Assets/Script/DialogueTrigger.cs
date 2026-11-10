using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [TextArea(3, 10)]
    public string[] lines;
    public DialogueManager dialogueManager;

    // Este método será chamado quando você clicar no NPC/Objeto
    public void StartInteraction()
    {
        // Só inicia se o diálogo não estiver aberto
        if (!dialogueManager.dialogueUI.activeSelf) 
        {
            dialogueManager.StartDialogue(lines);
        }
    }
}