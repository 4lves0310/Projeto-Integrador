# Salvamento do IFSC Adventures

Implementação preparada a partir da main, commit 8eede34. Não altera o GitHub.
Compatível com os nomes de campos dos passos anteriores e com Unity 2023.2 / Cinemachine 2.x.
O pacote contém apenas os scripts alterados/novos e os dois prefabs de itens.

## Instalar

1. Feche o Play Mode e faça uma cópia dos arquivos atuais.
2. Copie a pasta Assets do pacote para a raiz do projeto, substituindo os arquivos correspondentes. Mantenha os .meta existentes; copie também os .meta novos incluídos.
3. Se já criou ItemData.cs, mantenha o .meta desse script no seu projeto e confira os componentes ItemData nos prefabs depois da importação. Os prefabs do pacote referenciam o .meta novo fornecido. Se os GUIDs divergirem, remova o componente ausente e adicione ItemData novamente, com IDs hamburguer e coca e quantidade 1.
4. Não duplique as classes SaveData e ItemSaveData em outros arquivos.

## Configurar cada cena jogável

- Jogador ativo com tag Player.
- Um InventoryController, que pode ficar no GameController. Configure Inventory Panel apontando para InventoryPage, Slot Prefab apontando para Assets/prefabs/Slot.prefab, Slot Count = 15 e Item Prefabs com item_Hamb e item_Coca Variant. Se o painel não tiver layout, adicione Grid Layout Group e ajuste o tamanho das células ao seu inventário.
- Cada prefab de item precisa de ItemData, RectTransform e dos componentes de arrastar já existentes. Os dois prefabs fornecidos têm IDs hamburguer e coca. Novos tipos precisam de IDs únicos e devem entrar na lista Item Prefabs de todas as cenas em que possam ser carregados.
- A lista Item Prefabs também mantém a função original de fornecer os itens iniciais (um por slot). Se quiser começar com inventário vazio, será preciso separar catálogo e itens iniciais.
- Na câmera virtual, adicione a extensão CinemachineConfiner (o componente antigo da versão 2.x, não CinemachineConfiner2D). Selecione modo Confine 2D e atribua o PolygonCollider2D ou CompositeCollider2D do MapBounds em Bounding Shape 2D. Use nome único para cada objeto de limite na cena.
- Um SaveController ativo, por exemplo no GameController. Ele não deve usar DontDestroyOnLoad. O objeto temporário SaveLoadOperation cuida de atravessar a troca de cena; não adicione esse script manualmente.
- Remova outras chamadas automáticas de carregamento no Start. Não mantenha uma versão antiga do SaveController junto da nova.

Observações da main analisada: SampleScene não tinha InventoryController configurado; Scene 2 tinha inventário, mas o botão SaveButton estava com alvo nulo e não tinha SaveController associado no GameController. Não encontrei Bounding Shape 2D serializado nessas cenas. Essas ligações precisam ser feitas no Inspector.

## Menu e botões

Adicione SaveController a um objeto ativo no menu. Não é necessário inventário nem câmera no menu.

No campo First Scene Name, escreva o nome EXATO da primeira cena jogável. O padrão do script é SampleScene; altere se seu jogo começa em outra cena.

| Botão | Método em Button > On Click() |
| --- | --- |
| Novo jogo | SaveController.NewGame() |
| Continuar | SaveController.ContinueGame() |
| Salvar | SaveController.SaveGame() |
| Excluir save | SaveController.DeleteSave() |

Arraste o GameObject com SaveController para cada evento e selecione o método. Corrija também o alvo do botão Salvar já existente. Em cada cena, os botões devem apontar para o SaveController local.

No SaveController do menu, arraste o botão Continuar para Continue Button. Ele será desabilitado quando não houver arquivo legível ou durante o carregamento. Se houver mais de um botão Continuar, configure um controlador por conjunto de interface.

Crie um texto TextMeshPro e arraste para Status Text para mostrar sucesso e erros na tela. Sem essa referência, as mensagens aparecem apenas no Console. Use texto ativo na cena de destino também se quiser ver o resultado do carregamento.

Novo jogo recarrega a primeira cena e substitui o save quando consegue capturar o estado inicial. Se quiser confirmação antes de substituir, abra um painel e só chame NewGame() no botão Confirmar.

Excluir save apaga o arquivo, sem reiniciar o jogo em execução. Se clicar em Salvar novamente, será criado outro arquivo. A abertura do menu não cria saves.

## Cenas no build

Em File > Build Settings, adicione o menu e todas as cenas jogáveis em Scenes In Build. Coloque o menu primeiro para ele ser a tela inicial do executável. Use nomes de cenas distintos.

## O que acontece ao continuar

1. O arquivo é lido e seus campos básicos são verificados.
2. A cena salva é carregada, mesmo quando já é a cena atual, reconstruindo seu estado inicial.
3. Os slots são inicializados explicitamente, inclusive com painel inativo.
4. Todos os IDs, quantidades e índices salvos são conferidos antes de limpar os itens atuais.
5. O inventário é restaurado, e a posição do jogador e o limite da câmera são aplicados.

Os saves antigos sem nome de cena usam First Scene Name como destino; sem inventário salvo, conservam os itens iniciais. Um inventário salvo vazio permanece vazio.

O arquivo fica em Application.persistentDataPath/save.json. A gravação usa um temporário e substituição do arquivo existente. Saves ilegíveis ou incompatíveis geram mensagens e não são recriados silenciosamente.

## Teste no Unity

1. Novo jogo: confira posição e itens iniciais e a mensagem de sucesso.
2. Mova o personagem e troque Coca e hambúrguer de lugar. Salve após soltar os itens.
3. Feche e abra o jogo. Continuar deve recuperar a cena, a posição e os slots.
4. Teste Continuar a partir de outra cena, com o inventário fechado.
5. Faça um save com inventário vazio e confira que ele volta vazio.
6. Exclua o save no menu: Continuar deve ficar indisponível; reabrir o menu não deve recriar o arquivo.
7. Com uma cópia de segurança, altere o JSON para um ID desconhecido ou slot fora do limite e confira a mensagem de erro.
8. Teste também no executável, não apenas em Play Mode.

## Limites desta entrega

Foi verificada a sintaxe dos scripts e a consistência dos IDs/metadados dos prefabs. O Unity não está instalado no ambiente de preparação: compilação contra os pacotes, Inspector, layout e fluxo em Play Mode ainda precisam ser verificados no seu projeto.

O save cobre cena, posição, limite de câmera e inventário. Missões, NPCs, portas, itens coletados no mapa e outros estados do mundo precisam de seus próprios campos e lógica de restauração. Quantidade é persistida, mas isso não implementa empilhamento nem interface de quantidade. Este sistema usa JSON local, sem banco de dados.
