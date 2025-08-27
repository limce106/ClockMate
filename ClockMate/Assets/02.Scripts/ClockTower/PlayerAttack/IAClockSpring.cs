using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAClockSpring : MonoBehaviourPun, IInteractable
{
    public Transform hourAttachPos;
    public Transform milliAttachPos;

    private UINotice _uiNotice;
    private Sprite _exitSprite;
    private string _exitString;

    private Rigidbody _rb;

    private Dictionary<int, CharacterBase> _attachedPlayers = new Dictionary<int, CharacterBase>();
    private Dictionary<int, bool> _pushInput = new Dictionary<int, bool>();
    private Vector3 _followLocalOffset;

    private const float RotationSpeed = 50f;
    private const float RecoveryFillAmount = 0.0005f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        _exitSprite = Resources.Load<Sprite>("UI/Sprites/keyboard_q_outline");
        _exitString = "나가기";
    }

    private void Update()
    {
        if (IsLocalPlayerAttached(out int localViewID))
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ExitControl(localViewID);
            }
        }
    }

    private void FixedUpdate()
    {
        if (IsLocalPlayerAttached(out int localViewID))
        {
            // W 입력 동기화
            bool isPushing = Input.GetKey(KeyCode.W);
            photonView.RPC(nameof(RPC_SetPushInput), RpcTarget.All, localViewID, isPushing);
        }

        // 둘 다 W 누르고 있으면 태엽 회전
        // TODO 테스트가 끝나면 2로 바꿀 것!!
        if (_attachedPlayers.Count == 2 && PhotonNetwork.IsMasterClient)
        {
            bool allPushing = true;
            foreach (var kvp in _pushInput)
            {
                if (!kvp.Value) { allPushing = false; break; }
            }

            if (allPushing)
            {
                RotateSpring();
            }
        }

        FollowClockSpring();
    }

    private void FollowClockSpring()
    {
        if(IsLocalPlayerAttached(out int localViewID))
        {
            CharacterBase character = _attachedPlayers[localViewID];

            Vector3 newPosition = transform.TransformPoint(_followLocalOffset);
            newPosition.y = character.transform.position.y;

            character.transform.position = newPosition;

            if (character.transform.root.tag == "Hour")
            {
                character.transform.rotation = transform.rotation;
            }
            else if (character.transform.root.tag == "Milli")
            {
                Quaternion oppositeRotation = Quaternion.Euler(
                    transform.rotation.eulerAngles.x,
                    transform.rotation.eulerAngles.y + 180f,
                    transform.rotation.eulerAngles.z);

                character.transform.rotation = oppositeRotation;
            }
        }
    }

    private void RotateSpring()
    {
        _rb.MoveRotation(_rb.rotation * Quaternion.Euler(0, RotationSpeed * Time.fixedDeltaTime, 0));
        BattleManager.Instance.photonView.RPC(nameof(BattleManager.Instance.RPC_UpdateRecovery), RpcTarget.All, RecoveryFillAmount);
    }

    bool IsLocalPlayerAttached(out int localViewID)
    {
        foreach(var kvp in _attachedPlayers)
        {
            if(kvp.Value.photonView.IsMine)
            {
                localViewID = kvp.Key;
                return true;
            }
        }

        localViewID = -1;
        return false;
    }
    public void ExitControl(int viewID)
    {
        if (!_attachedPlayers.ContainsKey(viewID)) return;

        CharacterBase character = _attachedPlayers[viewID];
        character.ChangeState<IdleState>();
        character.InputHandler.enabled = true;

        photonView.RPC(nameof(RPC_RemovePlayer), RpcTarget.All, character.photonView.ViewID);
        _followLocalOffset = Vector3.zero;

        if(_attachedPlayers.Count == 0)
            _rb.isKinematic = true;

        UIManager.Instance.Close(_uiNotice);
        _uiNotice = null;

        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach (var col in cols)
        {
            col.enabled = true;
        }
    }

    [PunRPC]
    public void RPC_SetPushInput(int viewID, bool isPushing)
    {
        if(_pushInput.ContainsKey(viewID))
        {
            _pushInput[viewID] = isPushing;
        }
    }

    [PunRPC]
    public void RPC_AddPlayer(int viewID)
    {
        CharacterBase character = PhotonView.Find(viewID).GetComponent<CharacterBase>();

        _attachedPlayers[viewID] = character;
        _pushInput[viewID] = false;
    }

    [PunRPC]
    public void RPC_RemovePlayer(int viewID)
    {
        if (!_attachedPlayers.ContainsKey(viewID)) return;

        _attachedPlayers.Remove(viewID);
        _pushInput.Remove(viewID);
    }

    public bool CanInteract(CharacterBase character)
    {
        return !_attachedPlayers.ContainsKey(character.photonView.ViewID);
    }

    public bool Interact(CharacterBase character)
    {
        character.ChangeState<PushState>(transform);
        character.InputHandler.enabled = false;

        _rb.isKinematic = false;

        if(character.transform.root.tag == "Hour")
        {
            character.transform.position = hourAttachPos.position;
        }
        else if (character.transform.root.tag == "Milli")
        {
            character.transform.position = milliAttachPos.position;
        }

        photonView.RPC(nameof(RPC_AddPlayer), RpcTarget.All, character.photonView.ViewID);

        Vector3 localOffset = transform.InverseTransformPoint(character.transform.position);
        localOffset.y = 0f;
        _followLocalOffset = localOffset;

        _uiNotice = UIManager.Instance.Show<UINotice>("UINotice");
        _uiNotice.SetImage(_exitSprite);
        _uiNotice.SetText(_exitString);

        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach(var col in cols)
        {
            col.enabled = false;
        }

        return true;
    }

    public void OnInteractAvailable() { }

    public void OnInteractUnavailable(){ }

    [PunRPC]
    public void RPC_ExitControlAll()
    {
        foreach (var kvp in _attachedPlayers)
        {
            CharacterBase character = kvp.Value;

            if(character.photonView.IsMine)
            {
                character.ChangeState<IdleState>();
                character.InputHandler.enabled = true;
            }
        }
    }
}
