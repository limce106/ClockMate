using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAClimbObject : MonoBehaviourPun, IInteractable
{
    private UIManager _uiManager;
    private UINotice _uiNotice;

    private Sprite _climbSprite;
    private string _climbString;

    public Vector3 climbOffset = new Vector3(-0.3f, 0f, 0.3f);

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _uiManager = UIManager.Instance;
        _climbSprite = Resources.Load<Sprite>("UI/Sprites/interact_active");
        _climbString = "올라가기";
    }

    public bool CanInteract(CharacterBase character)
    {
        return character.CurrentState is not ClimbState;
    }

    public void OnInteractAvailable()
    {
        _uiNotice = _uiManager.Show<UINotice>("UINotice");
        _uiNotice.SetImage(_climbSprite);
        _uiNotice.SetText(_climbString);
    }

    public void OnInteractUnavailable()
    {
        _uiManager.Close(_uiNotice);
    }

    public bool Interact(CharacterBase character)
    {
        character.ChangeState<ClimbState>(gameObject.transform, climbOffset);
        return true;
    }
}
