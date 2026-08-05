using UnityEngine;

namespace Features.PhoenixFlame
{
	internal sealed class FireParticleBinder : MonoBehaviour
	{
		private static readonly GradientAlphaKey[] SharedAlphaKeys =
		{
			new GradientAlphaKey(0f, 0f),
			new GradientAlphaKey(0.95f, 0.15f),
			new GradientAlphaKey(0.55f, 0.7f),
			new GradientAlphaKey(0f, 1f)
		};

		[SerializeField]
		private FireColorChannels _channels;

		[SerializeField]
		private ParticleSystem _particles;

		[SerializeField]
		private ParticleSystem[] _extraLayers;

		private readonly Gradient _gradient = new Gradient();
		private readonly GradientColorKey[] _colorKeys =
		{
			new GradientColorKey(Color.white, 0f),
			new GradientColorKey(Color.white, 0.35f),
			new GradientColorKey(Color.white, 1f)
		};

		private Color _last = new Color(-1f, -1f, -1f, 1f);

		private void LateUpdate()
		{
			var color = _channels.Color;
			if (Approximately(color, _last))
			{
				return;
			}

			_last = color;
			ApplyColor(_particles, color);

			for (var i = 0; i < _extraLayers.Length; i++)
			{
				ApplyColor(_extraLayers[i], color);
			}
		}

		public void SnapTo(FireColorId id)
		{
			var c = ColorFor(id);
			_channels.SetColor(c);
			_last = new Color(-1f, -1f, -1f, 1f);
			ApplyColor(_particles, c);

			for (var i = 0; i < _extraLayers.Length; i++)
			{
				ApplyColor(_extraLayers[i], c);
			}
		}

		private static Color ColorFor(FireColorId id)
		{
			return id switch
			{
				FireColorId.Green => new Color(0.25f, 0.85f, 0.35f, 1f),
				FireColorId.Blue => new Color(0.25f, 0.55f, 1f, 1f),
				_ => new Color(1f, 0.45f, 0.12f, 1f)
			};
		}

		private static bool Approximately(Color a, Color b)
		{
			return Mathf.Abs(a.r - b.r) < 0.002f
			       && Mathf.Abs(a.g - b.g) < 0.002f
			       && Mathf.Abs(a.b - b.b) < 0.002f;
		}

		private void ApplyColor(ParticleSystem system, Color color)
		{
			var main = system.main;
			main.startColor = color;

			var colorOverLifetime = system.colorOverLifetime;
			if (!colorOverLifetime.enabled)
			{
				return;
			}

			_colorKeys[0] = new GradientColorKey(Color.Lerp(color, Color.white, 0.35f), 0f);
			_colorKeys[1] = new GradientColorKey(color, 0.35f);
			_colorKeys[2] = new GradientColorKey(Color.Lerp(color, Color.black, 0.55f), 1f);

			_gradient.SetKeys(_colorKeys, SharedAlphaKeys);
			colorOverLifetime.color = _gradient;
		}
	}
}
