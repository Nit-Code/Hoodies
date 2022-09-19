using TMPro;
using UnityEngine;

public class UIDeckDropdownHandler : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown myDecksDropdown;
    private SharedUser myUser;
    //private int mySelectedValue;
    public int GetSelectedDeck() {
        return myUser.GetDecks()[myDecksDropdown.value].GetId();
    }
    
    public void OnEnable()
    {
        myUser = FindObjectOfType<SharedUser>();
        if (myUser != null && myUser.GetDecks() != null)
        {
            foreach (SharedDeck deck in myUser.GetDecks())
            {
                TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(deck.GetName());
                myDecksDropdown.options.Add(option);
            }
        } 
    }
}
