using System;
using System.Collections.Generic;

namespace SoftGames.Gameplay.MagicWords
{
    public enum AvatarSide
    {
        Left,
        Right
    }

    [Serializable]
    public sealed class MagicWordsResponse
    {
        public DialogueEntry[] dialogue;
        public AvatarEntry[] avatars;
    }

    [Serializable]
    public sealed class DialogueEntry
    {
        public string name;
        public string text;
    }

    [Serializable]
    public sealed class AvatarEntry
    {
        public string name;
        public string url;
        public string position;
    }

    public sealed class ResolvedAvatar
    {
        public string Name;
        public string Url;
        public AvatarSide Side;
    }

    public sealed class ResolvedDialogueLine
    {
        public string Speaker;
        public DialogueMessage Message;
        public IReadOnlyList<DialogueToken> Tokens;
        public ResolvedAvatar Avatar;
        public AvatarSide Side;
    }
}
