using Common.Core;
using Common.Networking;
using Cysharp.Threading.Tasks;
using Common.UI;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.MagicWords
{
	internal sealed class MagicWordsPresenter : MonoBehaviour
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
		private string _endpoint = MagicWordsParse.DefaultEndpoint;

		[SerializeField]
		private EmojiCatalogAsset _emojiCatalog;

		private HttpJsonClient _http;
		private DialogueResolver _resolver;
		private DialogueFeed _feed;
		private LoadingController _loading;
		private CancellationTokenSource _loadCts;

		private void Awake()
		{
			_background.color = AppColors.BackgroundDeep;

			_title.text = MagicWordsTexts.Title;
			_title.color = AppColors.TextPrimary;

			_retryButton.onClick.RemoveAllListeners();
			_retryButton.onClick.AddListener(Load);
			_retryButton.gameObject.SetActive(false);

			_loading = LoadingController.Spawn(_loadingPrefab, transform);
			_http = new HttpJsonClient();
			_resolver = new DialogueResolver(new EmojiParser(_emojiCatalog));
			_feed = new DialogueFeed(_linePrefab, _contentRoot, _scrollRect);
		}

		private void Start()
		{
			Load();
		}

		private void OnDestroy()
		{
			CancelLoad();
			_feed?.Clear();
		}

		public void Load()
		{
			CancelLoad();
			_feed.Clear();
			ShowLoading(MagicWordsTexts.LoadingDialogue);

			_loadCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
			LoadAsync(_loadCts.Token).Forget();
		}

		private async UniTaskVoid LoadAsync(CancellationToken cancellationToken)
		{
			try
			{
				if (Application.internetReachability == NetworkReachability.NotReachable)
				{
					ShowError(MagicWordsTexts.NoInternet);
					return;
				}

				var fetched = await _http.GetJsonAsync<MagicWordsResponse>(_endpoint, cancellationToken);
				if (cancellationToken.IsCancellationRequested)
				{
					return;
				}

				var result = fetched.Success ? MagicWordsParse.Validate(fetched.Value) : fetched;
				if (!result.Success)
				{
					ShowError(FormatUserError(result.Error));
					return;
				}

				var lines = _resolver.BuildLines(result.Value);
				if (lines.Count == 0)
				{
					ShowError(MagicWordsTexts.NoDialogueLines);
					return;
				}

				ShowContent();
				await _feed.BindAsync(lines, cancellationToken);
			}
			catch (OperationCanceledException) { }
			catch (Exception ex)
			{
				if (!cancellationToken.IsCancellationRequested)
				{
					ShowError(MagicWordsTexts.UnexpectedErrorPrefix + ex.Message);
				}
			}
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
			SetStatus(message, AppColors.Accent);
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
				return MagicWordsTexts.LoadFailedGeneric;
			}

			if (technical.IndexOf('\n') >= 0)
			{
				return technical;
			}

			return MagicWordsTexts.LoadFailedPrefix + technical;
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
