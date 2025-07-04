using Cinemachine;
using UnityEngine;

public class CameraModeToggle : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _followCamera;
    [SerializeField] private CinemachineVirtualCamera _overallCamera;
    
    private bool _isFollowMode = true;

    private void Awake()
    {
        _followCamera.gameObject.SetActive(_isFollowMode);
        _overallCamera.gameObject.SetActive(!_isFollowMode);
    }

    public void ToggleCameraMode()
    {
        _isFollowMode = !_isFollowMode;
        
        _followCamera.gameObject.SetActive(_isFollowMode);
        _overallCamera.gameObject.SetActive(!_isFollowMode);
        //_followCamera.Priority = _isFollowMode ? 10 : 0;
        //_overallCamera.Priority = _isFollowMode ? 0 : 10;
    }
}
