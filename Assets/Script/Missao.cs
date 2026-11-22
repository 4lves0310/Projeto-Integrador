using System;

// 1. Criamos listas de opções (Enums) para facilitar a configuração
public enum TipoMissao 
{ 
    ColetarItem, 
    IrLocal, 
    FalarNPC 
}

public enum StatusMissao 
{ 
    Disponivel, 
    EmAndamento, 
    Concluida 
}

// 2. Criamos a classe que representa a Missão em si
[Serializable] // Isso permite que a missão apareça no painel (Inspector) da engine
public class Missao
{
    public string idMissao;             // Ex: "missao_01_chave"
    public TipoMissao tipo;             // Ex: TipoMissao.ColetarItem
    public string idAlvo;               // Ex: "chave_ferrugem" ou "npc_diretor"
    
    public int progressoAtual;          // Começa sempre em 0
    public int progressoNecessario;     // Quantos itens precisa (ex: 5). Para NPC/Local, será 1.
    
    public StatusMissao status;         // Começa como Disponivel

    // 3. Método construtor para criar uma missão facilmente via código (opcional)
    public Missao(string id, TipoMissao tipo, string alvo, int necessidade)
    {
        this.idMissao = id;
        this.tipo = tipo;
        this.idAlvo = alvo;
        this.progressoAtual = 0;
        this.progressoNecessario = necessidade;
        this.status = StatusMissao.Disponivel;
    }
}