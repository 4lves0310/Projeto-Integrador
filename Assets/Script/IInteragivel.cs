public interface IInteragivel
{
    // Retorna a mensagem que vai aparecer na tela (ex: "Aperte E para falar", "Aperte E para abrir")
    string ObterMensagem();

    // O que acontece quando o jogador aperta a tecla de interação (ex: 'E')
    void Interagir();
}