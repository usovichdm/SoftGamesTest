using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoftGames.Core;
using SoftGames.Networking;
using SoftGames.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoftGames.Gameplay.MagicWords
{
    /// <summary>
    /// Loads Magic Words data and binds the scroll list. Domain resolution lives in
    /// <see cref="DialogueResolver"/>.
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
        private LoadingController _loadingPrefab;

        [SerializeField]
        private Image _background;

        [SerializeField]
        private string _endpoint = MagicWordsApiClient.DefaultEndpoint;

        private readonly List<DialogueLineView> _spawned = new List<DialogueLineView>(32);

        private HttpJsonClient _http;
        private MagicWordsApiClient _api;
        private TextureDownloader _textures;
        private DialogueResolver _resolver;
        private LoadingController _loading;
        private CancellationTokenSource _loadCts;

        private void Awake()
        {
            _background.color = AppColors.BackgroundDeep;

            _title.text = "Magic Words";
            _title.color = AppColors.TextPrimary;

            _retryButton.onClick.RemoveAllListeners();
            _retryButton.onClick.AddListener(Load);
            _retryButton.gameObject.SetActive(false);

            _loading = LoadingController.Spawn(_loadingPrefab, transform);
            _http = new HttpJsonClient();
            _api = new MagicWordsApiClient(_http, _endpoint);
            _textures = new TextureDownloader();
            _resolver = new DialogueResolver();
        }

        private void Start()
        {
            Load();
        }

        private void OnDestroy()
        {
            CancelLoad();
            ClearLines();
        }

        public void Load()
        {
            CancelLoad();
            ClearLines();
            ShowLoading("Loading dialogue…");

            _loadCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            LoadAsync(_loadCts.Token).Forget();
        }

        private async UniTaskVoid LoadAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    ShowError("No internet connection.\nCheck your network and try again.");
                    return;
                }

                var result = await _api.FetchAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (!result.Success)
                {
                    ShowError(FormatUserError(result.Error));
                    return;
                }

                var lines = _resolver.BuildLines(result.Value);
                if (lines.Count == 0)
                {
                    ShowError("No dialogue lines available.");
                    return;
                }

                ShowContent();
                await BindLinesAsync(lines, cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    ShowError($"Unexpected error.\n{ex.Message}");
                }
            }
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
                var line = _spawned[i];
                if (line == null)
                {
                    continue;
                }

                if (line.Avatar != null)
                {
                    line.Avatar.ReleaseResources();
                }

                Destroy(line.gameObject);
            }

            _spawned.Clear();
            _textures?.Clear();
        }

        private void ShowLoading(string message)
        {
            _retryButton.gameObject.SetActive(false);
            SetStatus(string.Empty, AppColors.TextMuted);
            _loading.Show(message);
        }

        private void ShowError(string message)
        {
            _loading.Hide();
            SetStatus(message, AppColors.Error);
            _retryButton.gameObject.SetActive(true);
        }

        private void ShowContent()
        {
            _loading.Hide();
            SetStatus(string.Empty, AppColors.TextMuted);
            _retryButton.gameObject.SetActive(false);
        }

        private void SetStatus(string message, Color color)
        {
            _statusLabel.color = color;
            _statusLabel.text = message ?? string.Empty;
            _statusLabel.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        private static string FormatUserError(string technical)
        {
            if (string.IsNullOrWhiteSpace(technical))
            {
                return "Failed to load dialogue.\nPlease try again.";
            }

            if (technical.IndexOf('\n') >= 0)
            {
                return technical;
            }

            return $"Failed to load dialogue.\n{technical}";
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
