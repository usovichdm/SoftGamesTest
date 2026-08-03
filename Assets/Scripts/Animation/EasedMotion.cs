using UnityEngine;

namespace SoftGames.Animation
{
    /// <summary>
    /// Shared easing helpers for gameplay tweens.
    /// </summary>
    public static class EasedMotion
    {
        /// <summary>Ease-in-out cubic — readable arc for card flights.</summary>
        public static float EaseInOutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        /// <summary>Slight lift above a straight line for card travel.</summary>
        public static Vector3 Arc(Vector3 from, Vector3 to, float t, float height)
        {
            var flat = Vector3.LerpUnclamped(from, to, t);
            var arc = 4f * height * t * (1f - t);
            flat.y += arc;
            return flat;
        }
    }
}
