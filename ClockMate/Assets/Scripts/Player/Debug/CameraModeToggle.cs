using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraModeToggle : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _followCamera;
    [SerializeField] private CinemachineVirtualCamera _fixedCamera;
    
    private bool _isFollowMode = false;

    public void ToggleCameraMode()
    {
        _isFollowMode = !_isFollowMode;
        
        _followCamera.Priority = _isFollowMode ? 10 : 0;
        _fixedCamera.Priority = _isFollowMode ? 0 : 10;
    }
}
