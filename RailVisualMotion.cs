using System;

namespace EMS_TEST_SIMULATOR
{
    /// <summary>SR50/SR150 레일 차체 표시: EMS current/target 섹션 기반 부드러운 X 보간 (물리 정밀도 불필요).</summary>
    public static class RailVisualMotion
    {
        public const double StartPos = 50;
        public const double EndPos = 700;
        public const int SensorCount = 13;

        public static double DogInterval => (EndPos - StartPos) / (SensorCount - 1);

        public static bool TryParseDogSection(string section, out int dog)
        {
            dog = -1;
            if (string.IsNullOrWhiteSpace(section)) return false;
            if (!int.TryParse(section.Trim(), out dog)) return false;
            return dog >= 101 && dog <= 113;
        }

        public static double DogToPixel(int dog) => StartPos + (dog - 101) * DogInterval;

        /// <summary>
        /// anchorX: EMS 현재 도그 픽셀을 부드럽게 추종.
        /// targetX: 주행(목적동작0) 중이면 목적 방향으로 완만히 끌어당김, 정지 시 anchor에 맞춤.
        /// currentX: 화면 표시 위치.
        /// </summary>
        public static void Tick(ref double currentX, ref double targetX, ref double anchorX,
            double lerpFactor, double anchorLerp = 0.22, double moveBlend = 0.42)
        {
            bool hasCurrent = TryParseDogSection(RailStatus.CurrentSectionCount, out int currentDog);
            bool hasTarget = TryParseDogSection(RailStatus.TargetSectionCount, out int targetDog);
            bool isRailMove = string.Equals(RailStatus.TargetActionMode?.Trim(), "0", StringComparison.Ordinal);

            if (hasCurrent)
            {
                double currentPx = DogToPixel(currentDog);
                anchorX += (currentPx - anchorX) * anchorLerp;
            }

            if (isRailMove && hasCurrent && hasTarget && currentDog != targetDog)
            {
                double destPx = DogToPixel(targetDog);
                double glideGoal = anchorX + (destPx - anchorX) * moveBlend;
                targetX += (glideGoal - targetX) * 0.18;
            }
            else if (hasCurrent)
            {
                targetX += (anchorX - targetX) * 0.25;
            }

            double diff = targetX - currentX;
            if (Math.Abs(diff) < 0.35)
                currentX = targetX;
            else
                currentX += diff * lerpFactor;
        }
    }
}
