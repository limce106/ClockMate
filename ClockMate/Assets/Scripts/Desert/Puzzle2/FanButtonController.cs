using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanButtonController : MonoBehaviour
{
    private AirFan airFan;

    [SerializeField]
    private PressurePlate hourPlate;

    private bool previousPressedState = false;  // PressurePlate 상태 감지를 위함

    private float lastSwitchTime = 0f;
    private float debounceDelay = 1f;   // 버튼 쿨타임

    private void Awake()
    {
        airFan = GetComponentInParent<AirFan>();
    }

    void Update()
    {
        bool currentPressedState = hourPlate.IsPressed;

        if (currentPressedState != previousPressedState && currentPressedState == true)
        {
            if (Time.time - lastSwitchTime > debounceDelay)
            {
                TrySwitchFan();
                lastSwitchTime = Time.time;
            }
        }

        previousPressedState = currentPressedState;
    }

    private void TrySwitchFan()
    {
        if (NetworkManager.Instance.IsInRoomAndReady() && airFan.photonView.IsMine)
        {
            airFan.photonView.RPC("RPC_SwitchFan", RpcTarget.All);
        }
        else
        {
            airFan.SwitchFan();
        }
    }
}
