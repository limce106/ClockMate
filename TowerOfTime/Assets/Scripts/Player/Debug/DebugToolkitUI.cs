#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 디버그용 캐릭터 스탯 수정 UI
/// </summary>
public class DebugToolkitUI : MonoBehaviour
{
    [Header("슬라이더")]
    [SerializeField] private Slider _jumpPowerSlider;
    [SerializeField] private Slider _doubleJumpPowerSlider;
    [SerializeField] private Slider _walkSpeedSlider;
    [SerializeField] private Slider _climbSpeedSlider;

    [Header("슬라이더 수치 표시 텍스트")]
    [SerializeField] private Text _jumpText;
    [SerializeField] private Text _doubleJumpText;
    [SerializeField] private Text _walkText;
    [SerializeField] private Text _climbText;

    private PlayerBase _target;
    private CharacterStatsSO _runtimeStats;     // 수정용 복사본
    private CharacterStatsSO _originalSnapshot; // 마지막 저장값 복사본 (.asset과는 무관)

    /// <summary>
    /// 캐릭터가 바뀔 때 호출
    /// </summary>
    public void Init(PlayerBase character)
    {
        _target = character;

        // 저장된 값 스냅샷
        _originalSnapshot = Instantiate(_target.OriginalStats);

        // UI 조작용 복사본
        _runtimeStats = Instantiate(_target.Stats);
        _target.OverrideStats(_runtimeStats); // 캐릭터에 적용

        ApplyStatsToUI(_runtimeStats);
    }

    private void Update()
    {
        if (_target == null || _runtimeStats == null) return;

        ApplyStatsFromUI(_runtimeStats);
        _target.OverrideStats(_runtimeStats);
        UpdateTexts();
    }

    /// <summary>
    /// 슬라이더 → Stats
    /// </summary>
    private void ApplyStatsFromUI(CharacterStatsSO stats)
    {
        stats.jumpPower = Mathf.Round(_jumpPowerSlider.value * 10f) / 10f;
        stats.doubleJumpPower = Mathf.Round(_doubleJumpPowerSlider.value * 10f) / 10f;
        stats.walkSpeed = Mathf.Round(_walkSpeedSlider.value * 10f) / 10f;
        stats.climbSpeed = Mathf.Round(_climbSpeedSlider.value * 10f) / 10f;
    }

    /// <summary>
    /// Stats → 슬라이더
    /// </summary>
    private void ApplyStatsToUI(CharacterStatsSO stats)
    {
        _jumpPowerSlider.value = stats.jumpPower;
        _doubleJumpPowerSlider.value = stats.doubleJumpPower;
        _walkSpeedSlider.value = stats.walkSpeed;
        _climbSpeedSlider.value = stats.climbSpeed;
        UpdateTexts();
    }

    /// <summary>
    /// 현재 슬라이더 값을 텍스트에 표시
    /// </summary>
    private void UpdateTexts()
    {
        _jumpText.text = $"JumpPower: {_runtimeStats.jumpPower:0.0}";
        _doubleJumpText.text = $"DoubleJumpPower: {_runtimeStats.doubleJumpPower:0.0}";
        _walkText.text = $"WalkSpeed: {_runtimeStats.walkSpeed:0.0}";
        _climbText.text = $"ClimbSpeed: {_runtimeStats.climbSpeed:0.0}";
    }

    /// <summary>
    /// 저장 버튼: 실제 SO 파일에 반영
    /// </summary>
    public void SaveSO()
    {
#if UNITY_EDITOR
        if (_target == null || _runtimeStats == null) return;

        CopyStats(_runtimeStats, _target.OriginalStats);         // .asset에 반영
        CopyStats(_runtimeStats, _originalSnapshot);     // 복사본 갱신

        EditorUtility.SetDirty(_target.OriginalStats);
        AssetDatabase.SaveAssets();
        Debug.Log($"저장 완료: {_target.OriginalStats.name}");
#endif
    }

    /// <summary>
    /// 리셋 버튼: 마지막 저장 시점으로 복원
    /// </summary>
    public void ResetSO()
    {
        if (_originalSnapshot == null || _runtimeStats == null) return;

        CopyStats(_originalSnapshot, _runtimeStats);       // 복사본 복원
        ApplyStatsToUI(_runtimeStats);                     // UI에도 반영
        _target.OverrideStats(_runtimeStats);              // 캐릭터에도 적용

        Debug.Log("마지막 저장 상태로 리셋 완료");
    }

    private void CopyStats(CharacterStatsSO from, CharacterStatsSO to)
    {
        to.jumpPower = from.jumpPower;
        to.doubleJumpPower = from.doubleJumpPower;
        to.walkSpeed = from.walkSpeed;
        to.climbSpeed = from.climbSpeed;
    }
}
