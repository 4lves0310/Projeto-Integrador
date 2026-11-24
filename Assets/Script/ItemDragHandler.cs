using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();
        if (dropSlot == null && eventData.pointerEnter != null)
        {
            dropSlot = eventData.pointerEnter.GetComponentInParent<Slot>();
        }

        Slot originalSlot = originalParent.GetComponent<Slot>();

        if (dropSlot != null)
        {
            // Soltou em cima de um slot: troca ou move normalmente
            if (dropSlot.currentItem != null)
            {
                dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                originalSlot.currentItem = dropSlot.currentItem;
                dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            else
            {
                originalSlot.currentItem = null;
            }

            transform.SetParent(dropSlot.transform);
            dropSlot.currentItem = gameObject;
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        else
        {
            // Soltou fora de qualquer slot: tenta dropar no chão
            if (TentarDroparNoChao(originalSlot))
            {
                Destroy(gameObject);
                return;
            }

            // Se não tiver prefabNoChao configurado ou não achar o Player, devolve pro slot
            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }

    private bool TentarDroparNoChao(Slot originalSlot)
    {
        ItemData data = GetComponent<ItemData>();
        if (data == null || data.prefabNoChao == null)
            return false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return false;

        // Nasce um pouco abaixo do Player para não coletar no mesmo milissegundo se for coleta automática
        Vector3 posicaoDrop = player.transform.position + new Vector3(0f, -0.8f, 0f);
        GameObject dropObj = Instantiate(data.prefabNoChao, posicaoDrop, Quaternion.identity);

        ItemColetavel coletavel = dropObj.GetComponent<ItemColetavel>();
        if (coletavel != null)
        {
            coletavel.quantidade = data.quantity;
        }

        if (originalSlot != null)
        {
            originalSlot.currentItem = null;
        }

        AudioSFX.Instance?.TocarClique();
        return true;
    }
}