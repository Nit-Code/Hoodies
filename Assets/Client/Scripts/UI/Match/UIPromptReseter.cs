using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPromptReseter : MonoBehaviour, IPointerClickHandler
{
    MatchSceneUIManager myMatchSceneUIManager;
    ClientGameManager myGameManager;

    private void Start()
    {
        myMatchSceneUIManager = FindObjectOfType<MatchSceneUIManager>();
        myGameManager = FindObjectOfType<ClientGameManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!myMatchSceneUIManager.IsMouseOverActionPrompt(eventData))
        {
            myGameManager.ResetPlayerSelections();
            
        }
        // TODO: commented code?
        //if (myGameManagerReference.myIsCastingUnitAbility)
        //{
        //    myGameManagerReference.myIsCastingUnitAbility = false;
        //    myMatchSceneUIManagerReference.UpdateAbilityButtonStatus(null);
        //}
    }
}
