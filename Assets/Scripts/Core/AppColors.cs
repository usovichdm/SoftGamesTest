using UnityEngine;

namespace SoftGames.Core
{
    /// <summary>
    /// Shared casual-game palette. Keeps UI/gameplay visuals consistent without ScriptableObjects.
    /// </summary>
    public static class AppColors
    {
        public static readonly Color BackgroundDeep = new Color(0.09f, 0.16f, 0.18f, 1f);
        public static readonly Color BackgroundMid = new Color(0.13f, 0.24f, 0.27f, 1f);
        public static readonly Color Panel = new Color(0.16f, 0.30f, 0.33f, 0.94f);
        public static readonly Color PanelSoft = new Color(0.20f, 0.36f, 0.39f, 0.88f);

        public static readonly Color TextPrimary = new Color(0.95f, 0.94f, 0.90f, 1f);
        public static readonly Color TextMuted = new Color(0.78f, 0.82f, 0.80f, 1f);
        public static readonly Color Accent = new Color(0.93f, 0.45f, 0.38f, 1f);
        public static readonly Color AccentSoft = new Color(0.95f, 0.62f, 0.48f, 1f);
        public static readonly Color Success = new Color(0.45f, 0.78f, 0.55f, 1f);
        public static readonly Color Error = new Color(0.92f, 0.42f, 0.38f, 1f);

        public static readonly Color CardFace = new Color(0.97f, 0.96f, 0.93f, 1f);
        public static readonly Color CardBack = new Color(0.18f, 0.42f, 0.55f, 1f);
        public static readonly Color CardShadow = new Color(0f, 0f, 0f, 0.35f);
        public static readonly Color SuitRed = new Color(0.78f, 0.18f, 0.22f, 1f);
        public static readonly Color SuitBlack = new Color(0.16f, 0.18f, 0.20f, 1f);

        public static readonly Color FireOrange = new Color(1f, 0.45f, 0.12f, 1f);
        public static readonly Color FireGreen = new Color(0.25f, 0.85f, 0.35f, 1f);
        public static readonly Color FireBlue = new Color(0.25f, 0.55f, 1f, 1f);
    }
}
