using UnityEngine;
using UnityEngine.Playables;

public class UIChangeMixerBehaviour : PlayableBehaviour
{
    [HideInInspector] public UIChangeTrack track;  // 트랙 기본값 참조

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        var binding = playerData as TimelineUIBinder;
        if (binding == null) return;

        int inputCount = playable.GetInputCount();
        float totalWeight = 0f;
        UIChangeBehaviour last = null;

        for (int i = 0; i < inputCount; i++)
        {
            float w = playable.GetInputWeight(i);
            totalWeight += w;
            if (w > 0f)
                last = ((ScriptPlayable<UIChangeBehaviour>)playable.GetInput(i)).GetBehaviour();
        }

        if (binding.rootCanvasGroup)
            binding.rootCanvasGroup.alpha = totalWeight > 0f ? 1f : 0f;

        if (totalWeight <= 0f || last == null) return;

        bool twoMode = last.spriteB != null; // spriteB 있으면 이미지 2개

        // ---- A 슬롯 ----
        if (binding.imageA)
        {
            if (last.spriteA != null)
            {
                binding.imageA.sprite = last.spriteA;
                binding.imageA.enabled = true;

                Vector2 pA =
                    last.overridePosA ? last.posA :
                    (twoMode ? track.doublePosA : track.singlePosA);

                if (binding.RectA) binding.RectA.anchoredPosition = pA;
            }
            else binding.imageA.enabled = false;
        }

        // ---- B 슬롯 ----
        if (binding.imageB)
        {
            if (last.spriteB != null)
            {
                binding.imageB.sprite = last.spriteB;
                binding.imageB.enabled = true;

                Vector2 pB =
                    last.overridePosB ? last.posB :
                    track.doublePosB;

                if (binding.RectB) binding.RectB.anchoredPosition = pB;
            }
            else binding.imageB.enabled = false;
        }

        if (binding.targetText)
            binding.targetText.text = last.text ?? "";
    }
}
