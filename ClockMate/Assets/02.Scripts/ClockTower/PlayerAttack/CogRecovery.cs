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
     private CogCenter[] _slots;       // 런타임 수집
     private readonly List<int> _activeIdx = new();     // 이번 라운드 활성 인덱스
     private readonly List<Vector2> _usedXZ = new();    // 스폰한 위치 기록(XZ)
     
     protected override void Init()
     {
         BuildRefs();
     }

     private void Start()
     {
         SpawnCogs();
     }
     
     private void BuildRefs()
     {
         if (!cogsRoot)
         {
             Debug.LogError("[CogRecovery] cogsRoot/slotsRoot 미할당");
             return;
         }

         _cogs = cogsRoot.GetComponentsInChildren<Cog>(true);

         // 시작 시 전원 비활성
         foreach (Cog cog in _cogs)
             if (cog && cog.gameObject.activeSelf)
                 cog.gameObject.SetActive(false);
     }

     /// <summary>
     /// 톱니바퀴들을 랜덤 위치에서 스폰한다.
     /// </summary>
     private void SpawnCogs()
     {
         if (!PhotonNetwork.IsMasterClient) return;
     
         _usedXZ.Clear();
         _activeIdx.Clear();
     
         for (int i = 0; i < _cogs.Length; i++)
         {
             Vector3 pos = GetRandomSpawnPos(_usedXZ);
             float yaw = Random.Range(0f, 360f);
     
             photonView.RPC(nameof(RPC_ActivatePlaceAssign), RpcTarget.All, i, pos, yaw, i);
     
             _usedXZ.Add(new Vector2(pos.x, pos.z));
             _activeIdx.Add(i);
         }
     }
     
     [PunRPC] 
     private void RPC_ActivatePlaceAssign(int idx, Vector3 pos, float yawDeg, int id)
     {
         if (_cogs == null || _cogs.Length == 0) BuildRefs();
         if (idx < 0 || idx >= _cogs.Length) return;

         var cog = _cogs[idx];
         if (!cog) return;

         cog.Id = id;
         cog.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yawDeg, 0f));
         cog.gameObject.SetActive(true);
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
         
             // 제한 시간이 다 되었는지 if문으로 확인 후 아래 코드 추가
             if (PhotonNetwork.IsMasterClient && BattleManager.Instance.IsTimeLimitEnd())
             {
                 EndRecovery(false);
                 yield break;
             }

             yield return null;
         }
     }

     public override void CancelAttack() { }

     /// <summary>
     /// 모든 톱니바퀴가 홈에 맞춰졌는지 여부를 반환한다.
     /// </summary>
     private bool AllCogsFitted()
     {
         if (_activeIdx.Count <= 0) return false;
         // TODO 모든 톱니바퀴가 홈에 끼워졌는지 확인
         foreach (int i in _activeIdx)
         {
             if (i >= 0 && i < _cogs.Length && _cogs[i])
                 if (!_cogs[i].Fitted) return false;
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
         _activeIdx.Clear();
         _usedXZ.Clear();
     }
     
     /// <summary>
     /// 톱니바퀴를 스폰할 랜덤 위치 가져오기
     /// </summary>
     private Vector3 GetRandomSpawnPos(List<Vector2> used)
     {
         float fieldRadius = BattleManager.Instance.battleFieldRadius; // 원형 전장의 반지름
         Vector3 fieldCenter = BattleManager.Instance.BattleFieldCenter; // 원형 전장의 중심

         while (true)
         {
             // 랜덤 위치 생성
             float r = fieldRadius * Mathf.Sqrt(Random.value);
             float angle = Random.value * 360f;

             float x = fieldCenter.x + r * Mathf.Cos(angle * Mathf.Deg2Rad);
             float z = fieldCenter.z + r * Mathf.Sin(angle * Mathf.Deg2Rad);

             Vector3 randomPos = new Vector3(x, spawnY, z);
             Vector2 randomPosXZ = new Vector2(randomPos.x, randomPos.z);

             bool isOverlapping = false;

             foreach (Cog cog in _cogs)
             {
                 Vector2 existingPosXZ = new Vector2(cog.transform.position.x, cog.transform.position.z);

                 if (Vector2.Distance(randomPosXZ, existingPosXZ) <= minDistanceBetweenCogs)
                 {
                     isOverlapping = true;
                     break;
                 }
             }

             if (!isOverlapping)
             {
                 return randomPos;
             }
         }
     }

     void EndRecovery(bool isSuccess)
     {
         if (!PhotonNetwork.IsMasterClient) return;
         ClearCogs();
         BattleManager.Instance.photonView.RPC("ReportAttackResult", RpcTarget.All, isSuccess);
     }
}
