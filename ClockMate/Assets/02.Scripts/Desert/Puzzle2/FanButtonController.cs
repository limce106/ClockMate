using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanButtonController : MonoBehaviour
{
    private AirFan _airFan;

    [SerializeField]
    private PressurePlate _hourPlate;

    private bool _previousPressedState = false;  // PressurePlate 상태 감지를 위함

    private float _lastSwitchTime = 0f;
    private float _debounceDelay = 1f;   // 버튼 쿨타임

    private void Awake()
    {
        _airFan = GetComponentInParent<AirFan>();
    }

    void Update()
    {
        bool currentPressedState = _hourPlate.isPressed;

        if (currentPressedState != _previousPressedState && currentPressedState == true)
        {
            if (Time.time - _lastSwitchTime > _debounceDelay)
            {
                TrySwitchFan();
                _lastSwitchTime = Time.time;
            }
        }

        _previousPressedState = currentPressedState;
    }

    private void TrySwitchFan()
    {
        if (NetworkManager.Instance.IsInRoomAndReady() && PhotonNetwork.IsMasterClient)
        {
            _airFan.photonView.RPC("RPC_SwitchFan", RpcTarget.All);
        }
        else
        {
            _airFan.SwitchFan();
        }
    }
}
