using UnityEngine;

public class Snowball : MonoBehaviour
{
    private SnowballPoolModule _poolModule;
    private bool _isActive;
    private float _speed;
    private Transform _target;
    private SledHP _targetHP;

    public void Init(SnowballPoolModule pool, SledHP targetHP)
    {
        _poolModule = pool;
        _isActive = false;
        _targetHP = targetHP;
    }
    
    private void Update()
    {
        if (!_isActive) return;
        
        Vector3 direction = _target.position - transform.position;
        transform.Translate(direction.normalized * (_speed * Time.deltaTime), Space.World);
        
        if (Vector3.Distance(_target.position, transform.position) < 0.1f)
        {
            _poolModule.ReturnToPool(this);
            _targetHP.TakeDamage(1);
            _isActive = false;
        }

    }

    public void Launch(Transform spawnPos, Transform target, float speed)
    {
        _isActive = true;
        transform.position = spawnPos.position;
        _speed = speed;
        _target = target;
    }

    public void Destroy()
    {
        _poolModule.ReturnToPool(this);
        _isActive = false;
    }
}