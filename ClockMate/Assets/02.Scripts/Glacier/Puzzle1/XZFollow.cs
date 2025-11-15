using UnityEngine;

public class XZFollow : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private float fixedY;
    
    private void Update()
    {
        transform.position = new Vector3(followTarget.position.x, fixedY, followTarget.position.z);
    }
}
