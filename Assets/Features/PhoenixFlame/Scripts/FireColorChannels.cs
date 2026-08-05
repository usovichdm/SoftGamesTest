using UnityEngine;

namespace Features.PhoenixFlame
{
	internal enum FireColorId
	{
		Orange = 0,
		Green = 1,
		Blue = 2
	}

	public sealed class FireColorChannels : MonoBehaviour
	{
		public float colorR = 1f;
		public float colorG = 0.45f;
		public float colorB = 0.12f;

		public Color Color => new Color(colorR, colorG, colorB, 1f);

		public void SetColor(Color color)
		{
			colorR = color.r;
			colorG = color.g;
			colorB = color.b;
		}
	}
}
