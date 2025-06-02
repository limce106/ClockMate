using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayModeSelector : MonoBehaviour
{
    [SerializeField]
    private MatchManager matchManager;
    public Button continueButton;

    private void Start()
    {
        if (SaveManager.Instance == null)
            return;

        if(!SaveManager.Instance.SaveDataExist())
        {
            continueButton.interactable = false;
        }
    }

    public void OnClick_NewGame()
    {
        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSaveData();
        }

        matchManager.OnClick_CreateRoom();
    }
    public void OnClick_Continue()
    {
        if (SaveManager.Instance == null)
            return;

        matchManager.OnClick_CreateRoom();
    }
}
