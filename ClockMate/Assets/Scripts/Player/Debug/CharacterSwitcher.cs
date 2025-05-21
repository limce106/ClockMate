using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSwitcher : MonoBehaviour
{
    [SerializeField] private bool autoAssign = true; // 연결 자동/수동 여부
    
    [SerializeField] private CharacterBase _hour;
    [SerializeField] private CharacterBase _milli;
    
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _cameraFollowTarget;

    [SerializeField] private Text _currentCharacterText;

    [SerializeField] private DebugToolkitUI _debugUI;

    private void OnEnable()
    {
        if (!autoAssign) return;
        AutoAssign();
    }

    private void AutoAssign()
    {
        if (_hour == null)
        {
            _hour = GameObject.Find("Hour").GetComponent<Hour>();
        }

        if (_milli == null)
        {
            _milli = GameObject.Find("Milli").GetComponent<Milli>();
        }
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_cameraFollowTarget == null)
            _cameraFollowTarget = GameObject.Find("CameraTarget")?.transform;

        if (_currentCharacterText == null)
            _currentCharacterText = GameObject.Find("Text_CurrentCharacter")?.GetComponent<Text>();

        if (_debugUI == null)
            _debugUI = GameObject.Find("DebugToolkitPanel")?.GetComponent<DebugToolkitUI>();
    }

    private void Start()
    {
        ActivateCharacter(_hour);
    }

    public void SwitchToHour() => ActivateCharacter(_hour);
    
    public void SwitchToMilli() => ActivateCharacter(_milli);

    private void ActivateCharacter(CharacterBase target)
    {
        _hour.gameObject.GetComponent<PlayerInputHandler>().enabled = (target == _hour);
        _milli.gameObject.GetComponent<PlayerInputHandler>().enabled = (target == _milli);
        
        // 카메라 추적 대상 변경
        if (_cameraFollowTarget != null)
        {
            _cameraFollowTarget.position = target.transform.position;
        }
        _cameraFollowTarget.parent = target.transform;

        // 활성화된 캐릭터 텍스트 갱신
        if (_currentCharacterText != null)
        {
            _currentCharacterText.text = $"Current: {target.name}";
        }
        
        // 디버그 UI 타겟 갱신
        _debugUI?.Init(target);
    }

}
