using UnityEngine;

namespace SoftGames.Gameplay.PhoenixFlame
{
    /// <summary>
    /// Animated RGB channels sampled by the particle binder during Animator blends.
    /// </summary>
    public sealed class FireColorChannels : MonoBehaviour
    {
        public float colorR = 1f;
        public float colorG = 0.45f;
        public float colorB = 0.12f;

        public Color Color
        {
            get { return new Color(colorR, colorG, colorB, 1f); }
        }

        public void SetColor(Color color)
        {
            colorR = color.r;
            colorG = color.g;
            colorB = color.b;
        }
    }
}
