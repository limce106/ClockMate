using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GiantFlowerManager : MonoBehaviourPun
{
    public GiantFlower[] giantFlowers;

    public const float dropOffsetY = 5f;  // 하강 거리

    private int currentIndex = 0;
    private bool flowerLeveled = false;

    void Update()
    {
        HandleFlowerLevelCheck();
    }

    private void HandleFlowerLevelCheck()
    {
        if (currentIndex >= giantFlowers.Length)
            return;

        GiantFlower curFlower = giantFlowers[currentIndex];

        if(PhotonNetwork.IsMasterClient && curFlower.IsLevel() && !flowerLeveled)
        {
            flowerLeveled = true;
            photonView.RPC(nameof(RPC_GrowSteam), RpcTarget.All, currentIndex);
            curFlower.Lock();

            currentIndex++;
            flowerLeveled = false;
        }
    }

    [PunRPC]
    private void RPC_GrowSteam(int index)
    {
        GiantFlower nextFlower = giantFlowers[index];

        nextFlower.sideSteam.SetActive(true);
        nextFlower.steamAnimator.Play("Grow", 0, 0f);

        SoundManager.Instance.PlaySfx(key: "steam_grow", pos: null, volume: 0.4f);
    }
}
