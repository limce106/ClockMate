using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class CogRecovery : AttackPattern
{
     [Header("Scene Roots")]
     [SerializeField] private Transform cogsRoot; // 톱니바퀴들 최상위 부모

     [Header("Spawn")]
     [SerializeField] private float spawnY = 0f;
     [SerializeField] private float minDistanceBetweenCogs = 2f;

     private Cog[] _cogs;          // 런타임 수집
     private List<IACogGrip> _grips;          // 런타임 수집
     private CogCenter[] _slots;       // 런타임 수집
     private readonly List<int> _activeIdx = new();     // 이번 라운드 활성 인덱스
     private readonly List<Vector2> _usedXZ = new();    // 스폰한 위치 기록(XZ)
     
     protected override void Init()
     {
         BuildRefs();
     }

     private void BuildRefs()
     {
         if (!cogsRoot)
         {
             Debug.LogError("[CogRecovery] cogsRoot/slotsRoot 미할당");
             return;
         }

         _cogs = cogsRoot.GetComponentsInChildren<Cog>(true);
         _grips = new List<IACogGrip>();

         // 시작 시 전원 활성 & grips 캐싱
         foreach (Cog cog in _cogs)
         {
             foreach (var grip in cog.GetComponentsInChildren<IACogGrip>(true))
             {
                 _grips.Add(grip);
                 grip.OnGripStateChanged -= EnableAllGripInteraction;
                 grip.OnGripStateChanged += EnableAllGripInteraction;
             }
             if (cog && !cog.gameObject.activeSelf)
                 cog.gameObject.SetActive(true);
         }     
     }
     
     public override IEnumerator Run()
     {
         while (true)
         {
            if (AllCogsFitted())
            {
                 EndRecovery(true);
                 yield break;
            }
         
            if (BattleManager.Instance.IsTimeLimitEnd())
            {
                EndRecovery(false);
                yield break;
            }

            yield return null;
         }
     }

     /// <summary>
     /// 모든 톱니바퀴가 홈에 맞춰졌는지 여부를 반환한다.
     /// </summary>
     private bool AllCogsFitted()
     {
         // if (_activeIdx.Count <= 0) return false;
         // foreach (int i in _activeIdx)
         // {
         //     if (i >= 0 && i < _cogs.Length && _cogs[i])
         //         if (!_cogs[i].Fitted) return false;
         // }
         // return true;
         foreach (Cog cog in _cogs)
         {
             if (!cog.Fitted) return false;
         }
         return true;
     }

     /// <summary>
     /// 스폰된 톱니바퀴 전부 제거
     /// </summary>
     void ClearCogs()
     {
         if (!PhotonNetwork.IsMasterClient) return;

         photonView.RPC(nameof(RPC_ClearAllCogs), RpcTarget.All);
     }

     [PunRPC] 
     private void RPC_ClearAllCogs()
     {
         if (_cogs == null || _cogs.Length == 0) BuildRefs();

         foreach (int i in _activeIdx)
         {
             if (i >= 0 && i < _cogs.Length && _cogs[i])
                 _cogs[i].gameObject.SetActive(false);
         }
     }

    private void ReleaseAllCogs()
    {
        if (_cogs == null || _cogs.Length == 0) return;

        foreach(Cog cog in _cogs)
        {
            if(cog == null) continue;

            foreach(IACogGrip grip in _grips)
            {
                if(grip.IsOccupied && grip.HolderViewId != -1)
                {
                    grip.Release();
                }
            }
        }
    }

    private void EnableAllGripInteraction(bool enable)
    {
        foreach (IACogGrip grip in _grips)
        {
            grip.EnableInteraction(enable);
        }
    }

     void EndRecovery(bool isSuccess)
     {
        ReleaseAllCogs();
        BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, isSuccess);
     }

    public override void CleanUpAttack()
    {
        cleanUpEnded = true;
    }
}
