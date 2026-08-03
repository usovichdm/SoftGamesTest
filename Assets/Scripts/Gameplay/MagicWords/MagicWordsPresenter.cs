using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoftGames.Core;
using SoftGames.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoftGames.Gameplay.MagicWords
{
    /// <summary>
    /// Loads Magic Words data, parses emoji tokens, and binds the scroll list.
    /// </summary>
    public sealed class MagicWordsPresenter : MonoBehaviour
    {
        [SerializeField]
        private DialogueLineView _linePrefab;

        [SerializeField]
        private RectTransform _contentRoot;

        [SerializeField]
        private ScrollRect _scrollRect;

        [SerializeField]
        private TMP_Text _statusLabel;

        [SerializeField]
        private TMP_Text _title;

        [SerializeField]
        private Button _retryButton;

        [SerializeField]
        private Image _background;

        [SerializeField]
        private string _endpoint = MagicWordsApiClient.DefaultEndpoint;

        private readonly List<DialogueLineView> _spawned = new List<DialogueLineView>(32);

        private HttpJsonClient _http;
        private MagicWordsApiClient _api;
        private TextureDownloader _textures;
        private EmojiParser _parser;
        private CancellationTokenSource _loadCts;

        private void Awake()
        {
            _background.color = AppColors.BackgroundDeep;

            _title.text = "Magic Words";
            _title.color = AppColors.TextPrimary;

            _retryButton.onClick.RemoveAllListeners();
            _retryButton.onClick.AddListener(Load);
            _retryButton.gameObject.SetActive(false);

            _http = new HttpJsonClient();
            _api = new MagicWordsApiClient(_http, _endpoint);
            _textures = new TextureDownloader();
            _parser = new EmojiParser();
        }

        private void Start()
        {
            Load();
        }

        private void OnDestroy()
        {
            CancelLoad();
        }

        public void Load()
        {
            CancelLoad();
            ClearLines();
            SetStatus("Loading dialogue…");
            _retryButton.gameObject.SetActive(false);

            _loadCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            LoadAsync(_loadCts.Token).Forget();
        }

        private async UniTaskVoid LoadAsync(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _api.FetchAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (!result.Success)
                {
                    SetStatus($"Failed to load dialogue.\n{result.Error}");
                    _retryButton.gameObject.SetActive(true);
                    return;
                }

                var lines = BuildLines(result.Value);
                if (lines.Count == 0)
                {
                    SetStatus("No dialogue lines available.");
                    _retryButton.gameObject.SetActive(true);
                    return;
                }

                SetStatus(string.Empty);
                await BindLinesAsync(lines, cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private List<ResolvedDialogueLine> BuildLines(MagicWordsResponse response)
        {
            var avatars = BuildAvatarMap(response.avatars);
            var source = response.dialogue ?? Array.Empty<DialogueEntry>();
            var lines = new List<ResolvedDialogueLine>(source.Length);

            for (var i = 0; i < source.Length; i++)
            {
                var entry = source[i];
                if (entry == null)
                {
                    continue;
                }

                var speaker = string.IsNullOrWhiteSpace(entry.name) ? "Unknown" : entry.name.Trim();
                avatars.TryGetValue(speaker, out var avatar);

                var message = new DialogueMessage(entry.text);
                var tokens = _parser.Parse(message);

                lines.Add(new ResolvedDialogueLine
                {
                    Speaker = speaker,
                    Message = message,
                    Tokens = tokens,
                    Avatar = avatar,
                    Side = avatar != null ? avatar.Side : AvatarSide.Left
                });
            }

            return lines;
        }

        private static Dictionary<string, ResolvedAvatar> BuildAvatarMap(AvatarEntry[] entries)
        {
            var map = new Dictionary<string, ResolvedAvatar>(8);
            if (entries == null)
            {
                return map;
            }

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.name))
                {
                    continue;
                }

                var name = entry.name.Trim();
                var url = entry.url != null ? entry.url.Trim() : string.Empty;
                var side = ParseSide(entry.position);

                if (map.TryGetValue(name, out var existing))
                {
                    if (string.IsNullOrEmpty(existing.Url) && !string.IsNullOrEmpty(url))
                    {
                        existing.Url = url;
                        existing.Side = side;
                    }

                    continue;
                }

                map[name] = new ResolvedAvatar
                {
                    Name = name,
                    Url = url,
                    Side = side
                };
            }

            return map;
        }

        private static AvatarSide ParseSide(string position)
        {
            if (string.IsNullOrWhiteSpace(position))
            {
                return AvatarSide.Left;
            }

            return position.Trim().Equals("right", StringComparison.OrdinalIgnoreCase)
                ? AvatarSide.Right
                : AvatarSide.Left;
        }

        private async UniTask BindLinesAsync(
            List<ResolvedDialogueLine> lines,
            CancellationToken cancellationToken)
        {
            var avatarTasks = new List<UniTask>(lines.Count);

            for (var i = 0; i < lines.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = lines[i];
                var view = Instantiate(_linePrefab, _contentRoot);
                view.Bind(line);
                _spawned.Add(view);

                var speaker = line.Speaker;
                var url = line.Avatar != null ? line.Avatar.Url : null;
                var avatarView = view.Avatar;

                if (string.IsNullOrEmpty(url))
                {
                    avatarView.ShowPlaceholder(speaker);
                    continue;
                }

                avatarTasks.Add(LoadAvatarAsync(avatarView, speaker, url, cancellationToken));
            }

            _scrollRect.normalizedPosition = new Vector2(0f, 1f);

            if (avatarTasks.Count > 0)
            {
                await UniTask.WhenAll(avatarTasks);
            }
        }

        private async UniTask LoadAvatarAsync(
            AvatarView avatarView,
            string speaker,
            string url,
            CancellationToken cancellationToken)
        {
            try
            {
                var texture = await _textures.DownloadAsync(url, cancellationToken);
                if (avatarView == null)
                {
                    return;
                }

                if (texture == null)
                {
                    avatarView.ShowPlaceholder(speaker);
                }
                else
                {
                    avatarView.ShowTexture(texture, speaker);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void ClearLines()
        {
            for (var i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null)
                {
                    Destroy(_spawned[i].gameObject);
                }
            }

            _spawned.Clear();
        }

        private void SetStatus(string message)
        {
            _statusLabel.color = AppColors.TextMuted;
            _statusLabel.text = message ?? string.Empty;
            _statusLabel.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        private void CancelLoad()
        {
            if (_loadCts == null)
            {
                return;
            }

            _loadCts.Cancel();
            _loadCts.Dispose();
            _loadCts = null;
        }
    }
}
