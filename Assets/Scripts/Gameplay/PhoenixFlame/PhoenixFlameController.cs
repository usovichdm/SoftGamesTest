using SoftGames.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoftGames.Gameplay.PhoenixFlame
{
    /// <summary>
    /// Button cycles fire color through an Animator Controller for smooth blends.
    /// </summary>
    public sealed class PhoenixFlameController : MonoBehaviour
    {
        private static readonly int ToOrange = Animator.StringToHash("ToOrange");
        private static readonly int ToGreen = Animator.StringToHash("ToGreen");
        private static readonly int ToBlue = Animator.StringToHash("ToBlue");
        private static readonly int ColorIndex = Animator.StringToHash("ColorIndex");

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

        private readonly FireColorCycle _cycle = new FireColorCycle(FireColorId.Orange);

        private float _lockUntil;

        private void Awake()
        {
            _title.text = "Phoenix Flame";
            _title.color = AppColors.TextPrimary;

            _hint.text = "Tap to shift the flame through orange, green, and blue.";
            _hint.color = AppColors.TextMuted;

            _cycleButton.onClick.RemoveAllListeners();
            _cycleButton.onClick.AddListener(OnCycleClicked);

            ApplyColor(_cycle.Current, animate: false);
        }

        private void OnCycleClicked()
        {
            if (Time.unscaledTime < _lockUntil)
            {
                return;
            }

            _lockUntil = Time.unscaledTime + _reclickLockSeconds;
            ApplyColor(_cycle.Advance(), animate: true);
        }

        private void ApplyColor(FireColorId id, bool animate)
        {
            _buttonLabel.text = $"Flame: {id}";
            _buttonLabel.color = AppColors.TextPrimary;

            _fireAnimator.SetInteger(ColorIndex, (int)id);

            if (animate)
            {
                switch (id)
                {
                    case FireColorId.Green:
                        _fireAnimator.ResetTrigger(ToOrange);
                        _fireAnimator.ResetTrigger(ToBlue);
                        _fireAnimator.SetTrigger(ToGreen);
                        break;
                    case FireColorId.Blue:
                        _fireAnimator.ResetTrigger(ToOrange);
                        _fireAnimator.ResetTrigger(ToGreen);
                        _fireAnimator.SetTrigger(ToBlue);
                        break;
                    default:
                        _fireAnimator.ResetTrigger(ToGreen);
                        _fireAnimator.ResetTrigger(ToBlue);
                        _fireAnimator.SetTrigger(ToOrange);
                        break;
                }
            }
            else
            {
                _binder.SnapTo(id);
                var state = id switch
                {
                    FireColorId.Green => "Green",
                    FireColorId.Blue => "Blue",
                    _ => "Orange"
                };
                _fireAnimator.Play(state, 0, 0f);
            }
        }
    }
}
