using UnityEngine;

public class Porta : MonoBehaviour, IInteragivel
{
    public string ObterMensagem()
    {
        return "[E] INTERAGIR";
    }

    public void Interagir()
    {
        Debug.Log("Teleportando jogador ou mudando de cena...");
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }
}