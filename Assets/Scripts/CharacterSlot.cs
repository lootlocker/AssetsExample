using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterSlot : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI characterName;
    private int characterID;
    private bool hasCharacter;
    public void Generate(string characterNameString, int characterID)
    {
        hasCharacter = true;
        characterName.text = characterNameString;
        this.characterID = characterID;
    }

    public void Click()
    {
        if(hasCharacter == true)
        {
            StartCoroutine(PlayerManager.instance.LoadCharacterRoutine(characterID, true));
        }
        else
        {
            PlayerManager.instance.StartToCreateCharacterRoutine();
        }
    }
}
