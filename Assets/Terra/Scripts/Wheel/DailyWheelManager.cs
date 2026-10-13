using TerraTactix.Core;
using TerraTactix.Data;
using UnityEngine;

namespace TerraTactix.Wheel
{
    public sealed class WheelSegment
    {
        public string label;
        public RewardType type;
        public int amount;
        public float floatAmount;
        public Color color;
    }

    public sealed class DailyWheelManager : MonoBehaviour
    {
        public const int SegmentCount = 6;

        DailyWheelProgressData wheel;

        public static readonly WheelSegment[] Segments =
        {
            new WheelSegment { label = "50 Altın", type = RewardType.Gold, amount = 50, color = new Color(0.95f, 0.78f, 0.25f) },
            new WheelSegment { label = "1 Dondurucu", type = RewardType.TimeFreeze, amount = 1, color = new Color(0.36f, 0.82f, 0.96f) },
            new WheelSegment { label = "1 Can", type = RewardType.Hearts, amount = 1, color = new Color(0.90f, 0.32f, 0.38f) },
            new WheelSegment { label = "5 Kristal", type = RewardType.Gems, amount = 5, color = new Color(0.58f, 0.40f, 0.95f) },
            new WheelSegment { label = "250 Altın", type = RewardType.Gold, amount = 250, color = new Color(0.35f, 0.75f, 0.45f) },
            new WheelSegment { label = "2 Can", type = RewardType.Hearts, amount = 2, color = new Color(0.30f, 0.62f, 0.95f) }
        };

        public bool HasPending => wheel != null && wheel.hasPending;
        public int PendingSegment => wheel.pendingSegment;

        public bool CanSpin => wheel != null && !wheel.hasPending && wheel.lastSpinDay != TimeService.TodayKey;

        public void Bind(DailyWheelProgressData progress)
        {
            wheel = progress;
        }

        public void BeginSpin()
        {
            if (!CanSpin)
                return;

            var index = RngService.Range(0, SegmentCount);
            var segment = Segments[index];
            wheel.hasPending = true;
            wheel.pendingSegment = index;
            wheel.pendingType = segment.type;
            wheel.pendingAmount = segment.amount;
            wheel.pendingFloatAmount = segment.floatAmount;
            wheel.pendingTransactionId = "wheel:" + TimeService.TodayKey;
            wheel.lastSpinDay = TimeService.TodayKey;
            wheel.totalSpins++;
            GameBootstrap.Instance.Save.Save();
        }

        public bool CompletePending(out string label)
        {
            label = string.Empty;
            if (!wheel.hasPending)
                return false;

            var segment = Segments[Mathf.Clamp(wheel.pendingSegment, 0, SegmentCount - 1)];
            label = segment.label;
            var applied = GameBootstrap.Instance.Rewards.TryApply(
                wheel.pendingTransactionId,
                wheel.pendingType,
                wheel.pendingAmount,
                wheel.pendingFloatAmount);

            wheel.hasPending = false;
            GameBootstrap.Instance.Save.Save();
            if (applied)
                GameEvents.RaiseDailySpinCompleted();
            return applied;
        }
    }
}
