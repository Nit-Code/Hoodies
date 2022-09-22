using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTooltipScreenSpaceUI : MonoBehaviour
{
    private RectTransform myCanvasRectTransform;
    [SerializeField] private RectTransform myCardRectTransform;
    private RectTransform myRectTransform;
    private Canvas myTooltipParentCanvas;

    private void Awake()
    {
        myRectTransform = transform.GetComponent<RectTransform>();
        myCanvasRectTransform = transform.parent.GetComponent<RectTransform>();
        myTooltipParentCanvas = myCanvasRectTransform.GetComponent<Canvas>();
        MakeInvisible();
    }

    private void Update()
    {
        myRectTransform.anchoredPosition = Input.mousePosition / myCanvasRectTransform.localScale.x; // follow the mouse
    }

    public void MakeVisible()
    {
        myTooltipParentCanvas.enabled = true;
    }
    public void MakeInvisible()
    {
        myTooltipParentCanvas.enabled = false;
    }
}
