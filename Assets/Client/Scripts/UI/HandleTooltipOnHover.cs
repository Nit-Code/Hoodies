using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandleTooltipOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea][SerializeField] private string _message;
    private TooltipScreenSpaceUI _tooltip;
    private bool _isHovered;

    void Awake()
    {
        _tooltip = FindObjectOfType<TooltipScreenSpaceUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;

        StartCoroutine(HoverTimer());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        _tooltip.MakeInvisible();
        StopCoroutine(HoverTimer());
    }

    private IEnumerator HoverTimer()
    {
        yield return new WaitForSeconds(2f);
        if(_isHovered)
        {
            _tooltip.SetText(_message);
            _tooltip.MakeVisible();
        }
    }
}
