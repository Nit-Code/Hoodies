using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipScreenSpaceUI : MonoBehaviour
{
    private RectTransform myCanvasRectTransform;
    private RectTransform myBackgroundRectTransform;
    private TextMeshProUGUI myText;
    private RectTransform myRectTransform;
    private Canvas myTooltipParentCanvas;

    private void Awake()
    {
        // TODO: hardcoded named component finds ????????????????
        myBackgroundRectTransform = transform.Find("Background").GetComponent<RectTransform>();
        myText = transform.Find("Text").GetComponent<TextMeshProUGUI>();

        myRectTransform = transform.GetComponent<RectTransform>();
        myCanvasRectTransform = transform.parent.GetComponent<RectTransform>();
        myTooltipParentCanvas = myCanvasRectTransform.GetComponent<Canvas>();
        MakeInvisible();
    }

    private void Update()
    {
        myRectTransform.anchoredPosition = Input.mousePosition / myCanvasRectTransform.localScale.x; // follow the mouse
    }

    public void SetText(string aTooltipText)
    {        
        myText.SetText(aTooltipText);
        myText.ForceMeshUpdate();

        Vector2 textSize = myText.textBounds.size;
        Vector2 paddingSize = myText.margin * 2;

        myBackgroundRectTransform.sizeDelta = textSize + paddingSize;
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
