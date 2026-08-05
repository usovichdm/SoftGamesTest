using Common.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.PhoenixFlame
{
	internal sealed class PhoenixFlameController : MonoBehaviour
	{
		private static readonly int ColorIndex = Animator.StringToHash("ColorIndex");

		private static readonly string[] States =
		{
			nameof(FireColorId.Orange),
			nameof(FireColorId.Green),
			nameof(FireColorId.Blue)
		};

		[SerializeField]
		private Animator _fireAnimator;

		[SerializeField]
		private FireParticleBinder _binder;

		[SerializeField]
		private Button _cycleButton;

		[SerializeField]
		private TMP_Text _buttonLabel;

		[SerializeField]
		private TMP_Text _title;

		[SerializeField]
		private TMP_Text _hint;

		[SerializeField]
		private float _reclickLockSeconds = 0.35f;

		private FireColorId _color = FireColorId.Orange;
		private float _lockUntil;

		private void Awake()
		{
			_title.text = "Phoenix Flame";
			_title.color = AppColors.TextPrimary;

			_hint.text = "Tap to shift the flame through orange, green, and blue.";
			_hint.color = AppColors.TextMuted;

			_cycleButton.onClick.RemoveAllListeners();
			_cycleButton.onClick.AddListener(OnCycleClicked);

			ApplyColor(_color, animate: false);
		}

		private void OnCycleClicked()
		{
			if (Time.unscaledTime < _lockUntil)
			{
				return;
			}

			_lockUntil = Time.unscaledTime + _reclickLockSeconds;
			_color = (FireColorId)(((int)_color + 1) % 3);
			ApplyColor(_color, animate: true);
		}

		private void ApplyColor(FireColorId id, bool animate)
		{
			_buttonLabel.text = "Flame: " + id;
			_buttonLabel.color = AppColors.TextPrimary;

			var index = (int)id;
			_fireAnimator.SetInteger(ColorIndex, index);

			if (!animate)
			{
				_binder.SnapTo(id);
				_fireAnimator.Play(States[index], 0, 0f);
			}
		}
	}
}
