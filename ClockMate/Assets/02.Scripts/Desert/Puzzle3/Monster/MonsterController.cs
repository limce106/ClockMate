using System;
using System.Collections.Generic;
using System.Reflection;
using Define;
using DefineExtension;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using static Define.Character;

public class MonsterController : MonoBehaviourPun
{
   [SerializeField] private Transform[] patrolPoints;
   public Transform[] PatrolPoints => patrolPoints; // 몬스터가 순회할 순찰 지점

   [SerializeField] private Transform returnPoint; // 플레이어 추격 중지 후 복귀할 지점

   [SerializeField] private float horizontalViewAngle = 90f;
   [SerializeField] private float verticalViewAngle = 60f;
   [SerializeField] private float viewDistance = 15f;

   [SerializeField] private CharacterBase hour;
   public NavMeshAgent Agent { get; private set; }

   private LayerMask _viewMask; 
   private IMonsterState _currentState;
   private Dictionary<Type, IMonsterState> _states;
   public event Action<MonsterController> OnMonsterDied;
   
   private void Awake()
   {
      Init();
   }

   private void Update()
   {
      _currentState?.Update();
      ChangeColorAccordingToState();
   }

   private void Init()
   {
      Agent = GetComponent<NavMeshAgent>();
      _states = new Dictionary<Type, IMonsterState>();
      ChangeStateTo<MStatePatrol>();
      _viewMask = LayerMask.GetMask("Player", "Default"); // 플레이어와 장애물 레이어
      
      //hourTransform = GameObject.FindGameObjectWithTag("Hour").transform;
      if (!GameManager.Instance.Characters.TryGetValue(CharacterName.Hour, out hour))
      {
         hour = GameObject.FindGameObjectWithTag("Hour").GetComponentInParent<Hour>();
      }

      if (hour == null)
      {
         Debug.LogError("아워 찾기 실패");
      }
   }

   /// <summary>
   /// 몬스터의 State를 전환한다.
   /// </summary>
   public void ChangeStateTo<T>() where T : IMonsterState
   {
      var type = typeof(T);

      if (!_states.TryGetValue(type, out var state))
      {
         // 매개변수로 MonsterController 받는 생성자 찾음
         ConstructorInfo constructor = type.GetConstructor(new[] { typeof(MonsterController) });
         if (constructor == null)
            throw new Exception($"{type} 클래스에 맞는 생성자가 없음");

         state = (IMonsterState)constructor.Invoke(new object[] { this });
         _states[type] = state;
      }

      _currentState?.Exit();
      _currentState = state;
      _currentState?.Enter();
   }

   /// <summary>
   /// 몬스터가 Hour(플레이어)를 시야 내에서 감지 가능한지 여부를 반환한다.
   /// </summary>
   public bool CanSeeHour()
   {
      if (hour.CurrentState is DeadState) return false;
      
      Transform hourTransform = hour.transform;
      Vector3 dirToHour = hourTransform.position - transform.position;

      // hour와의 수평/수직 시야각 계산
      float hAngleToHour = Vector3.Angle(transform.forward, new Vector3(dirToHour.x, 0f, dirToHour.z));
      float vAngleToHour = Vector3.Angle(transform.forward, dirToHour);
      
      // 시야각(수평/수직) 및 거리 조건 체크
      bool inHorizontal = hAngleToHour <= horizontalViewAngle / 2f;
      bool inVertical = vAngleToHour <= verticalViewAngle / 2f;
      bool inDistance = dirToHour.magnitude <= viewDistance;

      if (!inHorizontal || !inVertical || !inDistance) return false;
      
      // 시야 범위 내라면 장애물로 가려져있는지 확인
      Vector3 eyePosition = transform.position + Vector3.up * 0.5f; // 몬스터 눈 높이
      Vector3 targetCenter = hourTransform.position + Vector3.up * 1.0f; // 아워 몸 높이
      dirToHour = targetCenter - eyePosition;
      
      bool hitDetected = Physics.Raycast(eyePosition, dirToHour.normalized, out RaycastHit hit, viewDistance, _viewMask);
      Debug.DrawRay(eyePosition, dirToHour.normalized * viewDistance, Color.red);
      return hitDetected && hit.transform.root.CompareTag("Hour");
   }

   public void ChaseHour()
   {
      Agent.SetDestination(hour.transform.position);
   }

   public void StopChaseAndReturn()
   {
      Agent.SetDestination(returnPoint.position);
   }
   
   public bool IsReturnComplete()
   {
      return Vector3.Distance(transform.position, returnPoint.position) < 1.0f;
   }

   public void Die()
   {
      // 사망 처리
      OnMonsterDied?.Invoke(this);
      gameObject.SetActive(false);
   }

   private void OnCollisionEnter(Collision other)
   {
      if (!other.gameObject.CompareTag("Hour")) return;

//      Hour hour = other.gameObject.GetComponent<Hour>();
//      if (!hour.photonView.IsMine) return;
      
      hour.ChangeState<DeadState>();
      if (_currentState is MStateChase)
      {
         ChangeStateTo<MStateReturn>();
      }
   }

   #region Test
   
   [SerializeField] private SkinnedMeshRenderer meshRenderer;

   /// <summary>
   /// currentState에 따라 몬스터의 색을 변경한다.
   /// </summary>
   private void ChangeColorAccordingToState()
   {
      Material materialInstance = meshRenderer.material;

      Color stateColor = _currentState switch
      {
         MStateChase => CanSeeHour() ? Color.red : Color.yellow,
         MStateReturn => Color.gray,
         _ => Color.black
      };

      if (materialInstance.color == stateColor) return;
      
      materialInstance.color = stateColor;
   }

   /// <summary>
   /// 디버그용 몬스터 시야 범위 시각화
   /// </summary>
   private void OnDrawGizmosSelected()
   {
      if (!Application.isPlaying || hour ==null) return;

      Vector3 position = transform.position;
      Vector3 forward = transform.forward;

      // 수평 시야
      Gizmos.color = Color.magenta;
      Quaternion left = Quaternion.AngleAxis(-horizontalViewAngle / 2f, Vector3.up);
      Quaternion right = Quaternion.AngleAxis(horizontalViewAngle / 2f, Vector3.up);
      Gizmos.DrawRay(position, left * forward * viewDistance);
      Gizmos.DrawRay(position, right * forward * viewDistance);


      // 상하 시야
      Gizmos.color = Color.blue;
      Quaternion up = Quaternion.AngleAxis(-verticalViewAngle / 2f, transform.right);
      Quaternion down = Quaternion.AngleAxis(verticalViewAngle / 2f, transform.right);
      Gizmos.DrawRay(position, up * forward * viewDistance);
      Gizmos.DrawRay(position, down * forward * viewDistance);
   }

   #endregion
}
