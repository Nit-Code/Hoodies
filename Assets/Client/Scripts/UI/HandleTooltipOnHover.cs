using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandleTooltipOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea][SerializeField] private string myMessage;
    private TooltipScreenSpaceUI myTooltip;
    private bool myIsHovered;

    void Awake()
    {
        myTooltip = FindObjectOfType<TooltipScreenSpaceUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        myIsHovered = true;

        StartCoroutine(HoverTimer());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        myIsHovered = false;
        myTooltip.MakeInvisible();
        StopCoroutine(HoverTimer());
    }

    private IEnumerator HoverTimer()
    {
        yield return new WaitForSeconds(1.5f);
        if(myIsHovered)
        {
            myTooltip.SetText(myMessage);
            myTooltip.MakeVisible();
        }
        yield return null;
    }
}
