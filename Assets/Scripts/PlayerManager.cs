using UnityEngine;
using LootLocker.Requests;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public Image loadingCanvasMaskImage;
    public Image characterClassMaskImage;
    public Image characterCreationMaskImage;
    public Image characterNameMaskImage;


    public static PlayerManager instance;


    public List<CharacterSlot> characterSlots = new List<CharacterSlot>();
    public int newCharacterClassID;
    public string newCharacterName;
    public TMP_InputField characterNameInputField;


    private List<string> equippableContexts = new List<string>();


    private Color classColor;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Start login in the background immediately when the player starts
        StartCoroutine(LoginPlayerRoutine());
    }

    // Log in player, return error if error occured
    IEnumerator LoginPlayerRoutine()
    {
        // Start Guest session
        bool gotError = false;
        bool gotResponse = false;

        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                gotResponse = true;
            }
            else
            {
                // Something was wrong, diplay the error message
                gotError = true;
                gotResponse = true;
            }
        });
        yield return new WaitWhile(() => gotResponse == false);
        if (gotError)
        {
            yield break;
        }

        yield return GenerateCharacterSlotsRoutine();

        // Simple function to animate the transition
        yield return AnimateCanvasMask(loadingCanvasMaskImage);

    }

    IEnumerator GenerateCharacterSlotsRoutine()
    {
        // Get all characters that the current player has
        bool gotError = false;
        bool gotResponse = false;
        LootLockerSDKManager.GetCharacterLoadout((response) =>
        {
            if (response.success)
            {
                gotResponse = true;
                // Populate the buttons with the characters that the player has
                for (int i = 0; i < response.loadouts.Length; i++)
                {
                    characterSlots[i].Generate(response.loadouts[i].character.name, response.loadouts[i].character.id);
                }
            }
            else
            {
                // Something was wrong, diplay the error message
                gotError = true;
                gotResponse = true;
            }
        });
        yield return new WaitWhile(() => gotResponse == false);
        if (gotError)
        {
            yield break;
        }
    }

    public IEnumerator LoadCharacterRoutine(int characterID, bool loadedCharacter)
    {
        yield return SetDefaultCharacterRoutine(characterID);
        yield return GetCharacterEquippableContextsRoutine();
        yield return LoadMainSceneRoutine();

        InventoryManager.instance.characterID = characterID;

        // Change color of the character
        VertexColorer.instance.ChangeColor(classColor);
        yield return InventoryManager.instance.HandleEquipmentRoutine();
        StartCoroutine(AnimateCanvasMask(characterCreationMaskImage));
        StartCoroutine(AnimateCanvasMask(characterClassMaskImage));
        StartCoroutine(AnimateCanvasMask(characterNameMaskImage));
    }

    IEnumerator GetCharacterEquippableContextsRoutine()
    {
        bool done = false;
        bool gotError = false;
        equippableContexts.Clear();
        LootLockerSDKManager.GetEquipableContextToDefaultCharacter((response) =>
        {
            if (response.success)
            {
                for (int i = 0; i < response.contexts.Length; i++)
                {
                    equippableContexts.Add(response.contexts[i].name);
                }
                done = true;
            }
            else
            {
                done = true;
                Debug.Log("Error:" + response.Error);
                gotError = true;
            }
        });
        yield return new WaitWhile(() => done == false);
        if (gotError)
        {
            yield break;
        }
    }

    public bool CanBeEquippedByPlayer(string contextID)
    {
        for (int i = 0; i < equippableContexts.Count; i++)
        {
            if (contextID == equippableContexts[i])
            {
                return true;
            }
        }
        return false;
    }

    public IEnumerator SetDefaultCharacterRoutine(int characterID)
    {
        bool done = false;
        bool gotError = false;
        LootLockerSDKManager.SetDefaultCharacter(characterID.ToString(), (response) =>
        {
            if (response.success)
            {
                string characterType = "";
                // Get color for this character type
                for (int i = 0; i < response.GetCharacters().Length; i++)
                {
                    if(response.GetCharacters()[i].is_default)
                    {
                        characterID = response.GetCharacters()[i].id;
                        characterType = response.GetCharacters()[i].type;
                    }
                }
                LootLockerSDKManager.ListCharacterTypes((response) =>
                {
                    if (response.success)
                    {
                        for (int i = 0; i < response.character_types.Length; i++)
                        {
                            if(response.character_types[i].name == characterType)
                            {
                                string colorValue = response.character_types[i].storage[0].value;
                                ColorUtility.TryParseHtmlString("#" + colorValue, out Color newClassColor);
                                classColor = newClassColor;
                                break;
                            }
                        }
                        done = true;
                    }
                    else
                    {
                        done = true;
                        Debug.Log("Error:" + response.Error);
                        gotError = true;
                    }
                });

            }
            else
            {
                done = true;
                Debug.Log("Error:" + response.Error);
                gotError = true;
            }
        });
        yield return new WaitWhile(() => done == false);
        if (gotError)
        {
            yield break;
        }
    }

    public void StartToCreateCharacterRoutine()
    {
        StartCoroutine(AnimateCanvasMask(characterCreationMaskImage));
    }

    public void SelectCharacterClass(int characterClassID)
    {
        newCharacterClassID = characterClassID;
        StartCoroutine(AnimateCanvasMask(characterClassMaskImage));
    }

    public void CreateNewCharacter()
    {
        StartCoroutine(CreateNewCharacterRoutine(characterNameInputField.text));
    }

    IEnumerator CreateNewCharacterRoutine(string newCharacterName)
    {
        bool done = false;
        bool gotError = false;
        int characterID = 0;
        LootLockerSDKManager.CreateCharacter(newCharacterClassID.ToString(), newCharacterName, true, (response) =>
        {
            if (response.success)
            {
                Debug.Log(response.GetCharacters()[0].id);
                characterID = response.GetCharacters()[0].id;
                done = true;
            }
            else
            {
                done = true;
                Debug.Log("Error:" + response.Error);
                gotError = true;
            }
        });
        yield return new WaitWhile(() => done == false);
        if (gotError)
        {
            yield break;
        }
        yield return LoadCharacterRoutine(characterID, false);
    }

    IEnumerator AnimateCanvasMask(Image canvasMaskImage, bool show = false)
    {
        canvasMaskImage.gameObject.SetActive(true);
        float duration = 1f;
        float timer = 0f;
        float startValue = show ? 0f : 1f;
        float endValue = show ? 1f : 0f;
        while (timer <= duration)
        {
            canvasMaskImage.fillAmount = Mathf.SmoothStep(startValue, endValue, timer / duration);

            // Increase timer
            timer += Time.deltaTime;
            yield return null;
        }

        canvasMaskImage.fillAmount = endValue;
        canvasMaskImage.gameObject.SetActive(show);
        yield return null;
    }

    private IEnumerator LoadMainSceneRoutine()
    {
        // Start loading the main scene async
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);

        // Wait until the asynchronous scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public IEnumerator ExitToStartScene()
    {
        yield return GenerateCharacterSlotsRoutine();
        StartCoroutine(AnimateCanvasMask(characterClassMaskImage, true));
        StartCoroutine(AnimateCanvasMask(characterNameMaskImage, true));
        yield return AnimateCanvasMask(characterCreationMaskImage, true);

        // Start unloading the main scene async
        AsyncOperation asyncUnLoad = SceneManager.UnloadSceneAsync("MainScene");

        // Wait until the asynchronous scene is fully loaded
        while (!asyncUnLoad.isDone)
        {
            yield return null;
        }
    }

    public void GetCharacterList()
    {
        LootLockerSDKManager.ListCharacterTypes((response) => {
            if(response.success)
            {
                for (int i = 0; i < response.character_types.Length; i++)
                {
                    Debug.Log(response.character_types[i]);
                }
            }
            });
    }

    public void GetCharacterEquippableItems()
    {
        LootLockerSDKManager.GetEquipableContextToDefaultCharacter((response) => { });
    }

    [ContextMenu("Clear PlayerPrefs")]
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("<color=red>Deleted PlayerPrefs</color>");
    }
}
