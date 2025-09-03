using UnityEngine;

/// <summary>
/// 캐릭터의 모든 사운드 효과(SFX) 재생을 전담
/// </summary>
public class CharacterSfx : MonoBehaviour
{
    [Header("Jump SFX")]
    [SerializeField] private string jumpSfxKey = "character_jump";
    [SerializeField] private float jumpVolume = 0.8f;

    [Header("Footstep SFX")]
    [SerializeField] private string footstepSfxKey = "character_walk";
    [SerializeField] private float footstepVolume = 0.8f;

    [Header("Landing SFX")]
    [SerializeField] private string landingSfxKey = "character_hit_ground";
    [SerializeField] private float minLandingImpactSpeed = 2.5f;
    [SerializeField] private float maxLandingImpactSpeed = 20f;
    [SerializeField] private float minLandingVolume = 0.4f;
    [SerializeField] private float maxLandingVolume = 1.0f;

    [Header("PickUp SFX")]
    [SerializeField] private string pickUpSfxKey = "item_get";
    [SerializeField] private float pickUpVolume = 1f;
    
    /// <summary>
    /// 점프 사운드 재생
    /// </summary>
    public void PlayJumpSound()
    {
        SoundManager.Instance.PlaySfx(jumpSfxKey, pos: transform.position, volume: jumpVolume);
    }

    /// <summary>
    /// 발소리 사운드 재생
    /// </summary>
    public void PlayFootstepSound()
    {
        SoundManager.Instance.PlaySfx(key: footstepSfxKey, pos: transform.position, volume: footstepVolume);
    }

    /// <summary>
    /// 착지 충격량에 따라 볼륨을 조절하여 착지 사운드 재생
    /// </summary>
    public void PlayLandingSound(float impactSpeed)
    {
        if (impactSpeed < minLandingImpactSpeed) return;

        float volumeScale = Mathf.InverseLerp(minLandingImpactSpeed, maxLandingImpactSpeed, impactSpeed);
        float finalVolume = Mathf.Lerp(minLandingVolume, maxLandingVolume, volumeScale);
        float finalPitch = Mathf.Lerp(1.0f, 1.1f, volumeScale);
//        Debug.Log($"impactSpeed: {impactSpeed}, finalVolume: {finalVolume}, finalPitch: {finalPitch}");

        SoundManager.Instance.PlaySfx(landingSfxKey, pos: transform.position, volume: finalVolume, pitch: finalPitch);
    }
    
    public void PlayPickUpSound()
    {
        SoundManager.Instance.PlaySfx(key:pickUpSfxKey, pos: transform.position, volume: pickUpVolume);
    }
}
