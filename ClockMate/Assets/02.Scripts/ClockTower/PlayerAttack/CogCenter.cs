using UnityEngine;

public class CogCenter : MonoBehaviour
{
    [SerializeField] private Cog cog;
    [SerializeField] private Material glowMat;
    private MeshRenderer _cogRenderer;
    
    private void Start()
    {
        _cogRenderer = cog.GetComponent<MeshRenderer>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (cog.Fitted || !cog.Carried) return; // 이미 끼워진 상태 또는 운반중이 아니라면 무시
        if (!other.CompareTag("CogSlot") || cog.Slot.gameObject != other.gameObject) return;
        // 중앙이 슬롯 중앙과 가깝고 올바른 슬롯이라면 상호작용 가능 표시
        cog.Slot.ActivateTrigger(true);
        //_cogRenderer.materials[1] = glowMat;
        cog.Slot.ApplyCogToTrigger(cog);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject != cog.gameObject) return;
        if (cog.Slot.TriggerActivated != cog.Carried)
        {
            // 톱니바퀴와 슬롯 중앙이 충분히 가까운데 트리거 활성화 상태와 운반 상태가 불일치한다면
            // 트리거 활성화 상태를 올바르게 변경한다.
            cog.Slot.ActivateTrigger(cog.Carried);
            //_cogRenderer.materials[1] = cog.Carried ? glowMat : null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (cog.Fitted || cog.Slot.gameObject != other.gameObject) return;
        
        cog.Slot.ActivateTrigger(false);
        //_cogRenderer.materials[1] = null;
    }
    
}
