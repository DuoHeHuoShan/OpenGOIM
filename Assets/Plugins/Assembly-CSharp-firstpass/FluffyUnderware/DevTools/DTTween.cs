using System;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public static class DTTween
	{
		public enum EasingMethod
		{
			Linear = 0,
			ExponentialIn = 1,
			ExponentialOut = 2,
			ExponentialInOut = 3,
			ExponentialOutIn = 4,
			CircularIn = 5,
			CircularOut = 6,
			CircularInOut = 7,
			CircularOutIn = 8,
			QuadraticIn = 9,
			QuadraticOut = 10,
			QuadraticInOut = 11,
			QuadraticOutIn = 12,
			SinusIn = 13,
			SinusOut = 14,
			SinusInOut = 15,
			SinusOutIn = 16,
			CubicIn = 17,
			CubicOut = 18,
			CubicInOut = 19,
			CubicOutIn = 20,
			QuarticIn = 21,
			QuarticOut = 22,
			QuarticInOut = 23,
			QuarticOutIn = 24,
			QuinticIn = 25,
			QuinticOut = 26,
			QuinticInOut = 27,
			QuinticOutIn = 28
		}

		public static float Ease(EasingMethod method, float t, float b, float c)
		{
			switch (method)
			{
			case EasingMethod.ExponentialIn:
				return ExpoIn(t, b, c);
			case EasingMethod.ExponentialOut:
				return ExpoOut(t, b, c);
			case EasingMethod.ExponentialInOut:
				return ExpoInOut(t, b, c);
			case EasingMethod.ExponentialOutIn:
				return ExpoOutIn(t, b, c);
			case EasingMethod.CircularIn:
				return CircIn(t, b, c);
			case EasingMethod.CircularOut:
				return CircOut(t, b, c);
			case EasingMethod.CircularInOut:
				return CircInOut(t, b, c);
			case EasingMethod.CircularOutIn:
				return CircOutIn(t, b, c);
			case EasingMethod.QuadraticIn:
				return QuadIn(t, b, c);
			case EasingMethod.QuadraticOut:
				return QuadOut(t, b, c);
			case EasingMethod.QuadraticInOut:
				return QuadInOut(t, b, c);
			case EasingMethod.QuadraticOutIn:
				return QuadOutIn(t, b, c);
			case EasingMethod.SinusIn:
				return SineIn(t, b, c);
			case EasingMethod.SinusOut:
				return SineOut(t, b, c);
			case EasingMethod.SinusInOut:
				return SineInOut(t, b, c);
			case EasingMethod.SinusOutIn:
				return SineOutIn(t, b, c);
			case EasingMethod.CubicIn:
				return CubicIn(t, b, c);
			case EasingMethod.CubicOut:
				return CubicOut(t, b, c);
			case EasingMethod.CubicInOut:
				return CubicInOut(t, b, c);
			case EasingMethod.CubicOutIn:
				return CubicOutIn(t, b, c);
			case EasingMethod.QuarticIn:
				return QuartIn(t, b, c);
			case EasingMethod.QuarticOut:
				return QuartOut(t, b, c);
			case EasingMethod.QuarticInOut:
				return QuartInOut(t, b, c);
			case EasingMethod.QuarticOutIn:
				return QuartOutIn(t, b, c);
			case EasingMethod.QuinticIn:
				return QuintIn(t, b, c);
			case EasingMethod.QuinticOut:
				return QuintOut(t, b, c);
			case EasingMethod.QuinticInOut:
				return QuintInOut(t, b, c);
			case EasingMethod.QuinticOutIn:
				return QuintOutIn(t, b, c);
			default:
				return Linear(t, b, c);
			}
		}

		public static float Ease(EasingMethod method, float t, float b, float c, float d)
		{
			switch (method)
			{
			case EasingMethod.ExponentialIn:
				return ExpoIn(t, b, c, d);
			case EasingMethod.ExponentialOut:
				return ExpoOut(t, b, c, d);
			case EasingMethod.ExponentialInOut:
				return ExpoInOut(t, b, c, d);
			case EasingMethod.ExponentialOutIn:
				return ExpoOutIn(t, b, c, d);
			case EasingMethod.CircularIn:
				return CircIn(t, b, c, d);
			case EasingMethod.CircularOut:
				return CircOut(t, b, c, d);
			case EasingMethod.CircularInOut:
				return CircInOut(t, b, c, d);
			case EasingMethod.CircularOutIn:
				return CircOutIn(t, b, c, d);
			case EasingMethod.QuadraticIn:
				return QuadIn(t, b, c, d);
			case EasingMethod.QuadraticOut:
				return QuadOut(t, b, c, d);
			case EasingMethod.QuadraticInOut:
				return QuadInOut(t, b, c, d);
			case EasingMethod.QuadraticOutIn:
				return QuadOutIn(t, b, c, d);
			case EasingMethod.SinusIn:
				return SineIn(t, b, c, d);
			case EasingMethod.SinusOut:
				return SineOut(t, b, c, d);
			case EasingMethod.SinusInOut:
				return SineInOut(t, b, c, d);
			case EasingMethod.SinusOutIn:
				return SineOutIn(t, b, c, d);
			case EasingMethod.CubicIn:
				return CubicIn(t, b, c, d);
			case EasingMethod.CubicOut:
				return CubicOut(t, b, c, d);
			case EasingMethod.CubicInOut:
				return CubicInOut(t, b, c, d);
			case EasingMethod.CubicOutIn:
				return CubicOutIn(t, b, c, d);
			case EasingMethod.QuarticIn:
				return QuartIn(t, b, c, d);
			case EasingMethod.QuarticOut:
				return QuartOut(t, b, c, d);
			case EasingMethod.QuarticInOut:
				return QuartInOut(t, b, c, d);
			case EasingMethod.QuarticOutIn:
				return QuartOutIn(t, b, c, d);
			case EasingMethod.QuinticIn:
				return QuintIn(t, b, c, d);
			case EasingMethod.QuinticOut:
				return QuintOut(t, b, c, d);
			case EasingMethod.QuinticInOut:
				return QuintInOut(t, b, c, d);
			case EasingMethod.QuinticOutIn:
				return QuintOutIn(t, b, c, d);
			default:
				return Linear(t, b, c, d);
			}
		}

		public static float Linear(float t, float b, float c)
		{
			return c * Mathf.Clamp01(t) + b;
		}

		public static float Linear(float t, float b, float c, float d)
		{
			return c * t / d + b;
		}

		public static float ExpoOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return (t != 1f) ? (c * (0f - Mathf.Pow(2f, -10f * t) + 1f) + b) : (b + c);
		}

		public static float ExpoOut(float t, float b, float c, float d)
		{
			return (t != d) ? (c * (0f - Mathf.Pow(2f, -10f * t / d) + 1f) + b) : (b + c);
		}

		public static float ExpoIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return (t != 0f) ? (c * Mathf.Pow(2f, 10f * (t - 1f)) + b) : b;
		}

		public static float ExpoIn(float t, float b, float c, float d)
		{
			return (t != 0f) ? (c * Mathf.Pow(2f, 10f * (t / d - 1f)) + b) : b;
		}

		public static float ExpoInOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t == 0f)
			{
				return b;
			}
			if (t == 1f)
			{
				return b + c;
			}
			if ((t /= 0.5f) < 1f)
			{
				return c / 2f * Mathf.Pow(2f, 10f * (t - 1f)) + b;
			}
			return c / 2f * (0f - Mathf.Pow(2f, -10f * (t -= 1f)) + 2f) + b;
		}

		public static float ExpoInOut(float t, float b, float c, float d)
		{
			if (t == 0f)
			{
				return b;
			}
			if (t == d)
			{
				return b + c;
			}
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * Mathf.Pow(2f, 10f * (t - 1f)) + b;
			}
			return c / 2f * (0f - Mathf.Pow(2f, -10f * (t -= 1f)) + 2f) + b;
		}

		public static float ExpoOutIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t < 0.5f)
			{
				return ExpoOut(t * 2f, b, c / 2f);
			}
			return ExpoIn(t * 2f - 1f, b + c / 2f, c / 2f);
		}

		public static float ExpoOutIn(float t, float b, float c, float d)
		{
			if (t < d / 2f)
			{
				return ExpoOut(t * 2f, b, c / 2f, d);
			}
			return ExpoIn(t * 2f - d, b + c / 2f, c / 2f, d);
		}

		public static float CircOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return c * Mathf.Sqrt(1f - (t -= 1f) * t) + b;
		}

		public static float CircOut(float t, float b, float c, float d)
		{
			return c * Mathf.Sqrt(1f - (t = t / d - 1f) * t) + b;
		}

		public static float CircIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return (0f - c) * (Mathf.Sqrt(1f - t * t) - 1f) + b;
		}

		public static float CircIn(float t, float b, float c, float d)
		{
			return (0f - c) * (Mathf.Sqrt(1f - (t /= d) * t) - 1f) + b;
		}

		public static float CircInOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if ((t /= 0.5f) < 1f)
			{
				return (0f - c) / 2f * (Mathf.Sqrt(1f - t * t) - 1f) + b;
			}
			return c / 2f * (Mathf.Sqrt(1f - (t -= 2f) * t) + 1f) + b;
		}

		public static float CircInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return (0f - c) / 2f * (Mathf.Sqrt(1f - t * t) - 1f) + b;
			}
			return c / 2f * (Mathf.Sqrt(1f - (t -= 2f) * t) + 1f) + b;
		}

		public static float CircOutIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t < 0.5f)
			{
				return CircOut(t * 2f, b, c / 2f);
			}
			return CircIn(t * 2f - 1f, b + c / 2f, c / 2f);
		}

		public static float CircOutIn(float t, float b, float c, float d)
		{
			if (t < d / 2f)
			{
				return CircOut(t * 2f, b, c / 2f, d);
			}
			return CircIn(t * 2f - d, b + c / 2f, c / 2f, d);
		}

		public static float QuadOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return (0f - c) * t * (t - 2f) + b;
		}

		public static float QuadOut(float t, float b, float c, float d)
		{
			return (0f - c) * (t /= d) * (t - 2f) + b;
		}

		public static float QuadIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return c * t * t + b;
		}

		public static float QuadIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t + b;
		}

		public static float QuadInOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if ((t /= 0.5f) < 1f)
			{
				return (0f - c) / 2f * (Mathf.Sqrt(1f - t * t) - 1f) + b;
			}
			return c / 2f * (Mathf.Sqrt(1f - (t -= 2f) * t) + 1f) + b;
		}

		public static float QuadInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return (0f - c) / 2f * (Mathf.Sqrt(1f - t * t) - 1f) + b;
			}
			return c / 2f * (Mathf.Sqrt(1f - (t -= 2f) * t) + 1f) + b;
		}

		public static float QuadOutIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t < 0.5f)
			{
				return QuadOut(t * 2f, b, c / 2f);
			}
			return QuadIn(t * 2f - 1f, b + c / 2f, c / 2f);
		}

		public static float QuadOutIn(float t, float b, float c, float d)
		{
			if (t < d / 2f)
			{
				return QuadOut(t * 2f, b, c / 2f, d);
			}
			return QuadIn(t * 2f - d, b + c / 2f, c / 2f, d);
		}

		public static float SineOut(float t, float b, float c)
		{
			return c * Mathf.Sin(Mathf.Clamp01(t) * ((float)Math.PI / 2f)) + b;
		}

		public static float SineOut(float t, float b, float c, float d)
		{
			return c * Mathf.Sin(t / d * ((float)Math.PI / 2f)) + b;
		}

		public static float SineIn(float t, float b, float c)
		{
			return (0f - c) * Mathf.Cos(Mathf.Clamp01(t) * ((float)Math.PI / 2f)) + c + b;
		}

		public static float SineIn(float t, float b, float c, float d)
		{
			return (0f - c) * Mathf.Cos(t / d * ((float)Math.PI / 2f)) + c + b;
		}

		public static float SineInOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if ((t /= 0.5f) < 1f)
			{
				return c / 2f * Mathf.Sin((float)Math.PI * t / 2f) + b;
			}
			return (0f - c) / 2f * (Mathf.Cos((float)Math.PI * (t -= 1f) / 2f) - 2f) + b;
		}

		public static float SineInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * Mathf.Sin((float)Math.PI * t / 2f) + b;
			}
			return (0f - c) / 2f * (Mathf.Cos((float)Math.PI * (t -= 1f) / 2f) - 2f) + b;
		}

		public static float SineOutIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t < 0.5f)
			{
				return SineOut(t * 2f, b, c / 2f);
			}
			return SineIn(t * 2f - 1f, b + c / 2f, c / 2f);
		}

		public static float SineOutIn(float t, float b, float c, float d)
		{
			if (t < d / 2f)
			{
				return SineOut(t * 2f, b, c / 2f, d);
			}
			return SineIn(t * 2f - d, b + c / 2f, c / 2f, d);
		}

		public static float CubicOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return c * ((t -= 1f) * t * t + 1f) + b;
		}

		public static float CubicOut(float t, float b, float c, float d)
		{
			return c * ((t = t / d - 1f) * t * t + 1f) + b;
		}

		public static float CubicIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return c * t * t * t + b;
		}

		public static float CubicIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t * t + b;
		}

		public static float CubicInOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if ((t /= 0.5f) < 1f)
			{
				return c / 2f * t * t * t + b;
			}
			return c / 2f * ((t -= 2f) * t * t + 2f) + b;
		}

		public static float CubicInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t * t + b;
			}
			return c / 2f * ((t -= 2f) * t * t + 2f) + b;
		}

		public static float CubicOutIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t < 0.5f)
			{
				return CubicOut(t * 2f, b, c / 2f);
			}
			return CubicIn(t * 2f - 1f, b + c / 2f, c / 2f);
		}

		public static float CubicOutIn(float t, float b, float c, float d)
		{
			if (t < d / 2f)
			{
				return CubicOut(t * 2f, b, c / 2f, d);
			}
			return CubicIn(t * 2f - d, b + c / 2f, c / 2f, d);
		}

		public static float QuartOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return (0f - c) * ((t -= 1f) * t * t * t - 1f) + b;
		}

		public static float QuartOut(float t, float b, float c, float d)
		{
			return (0f - c) * ((t = t / d - 1f) * t * t * t - 1f) + b;
		}

		public static float QuartIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return c * t * t * t * t + b;
		}

		public static float QuartIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t * t * t + b;
		}

		public static float QuartInOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if ((t /= 0.5f) < 1f)
			{
				return c / 2f * t * t * t * t + b;
			}
			return (0f - c) / 2f * ((t -= 2f) * t * t * t - 2f) + b;
		}

		public static float QuartInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t * t * t + b;
			}
			return (0f - c) / 2f * ((t -= 2f) * t * t * t - 2f) + b;
		}

		public static float QuartOutIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t < 0.5f)
			{
				return QuartOut(t * 2f, b, c / 2f);
			}
			return QuartIn(t * 2f - 1f, b + c / 2f, c / 2f);
		}

		public static float QuartOutIn(float t, float b, float c, float d)
		{
			if (t < d / 2f)
			{
				return QuartOut(t * 2f, b, c / 2f, d);
			}
			return QuartIn(t * 2f - d, b + c / 2f, c / 2f, d);
		}

		public static float QuintOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return c * ((t -= 1f) * t * t * t * t + 1f) + b;
		}

		public static float QuintOut(float t, float b, float c, float d)
		{
			return c * ((t = t / d - 1f) * t * t * t * t + 1f) + b;
		}

		public static float QuintIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			return c * t * t * t * t * t + b;
		}

		public static float QuintIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t * t * t * t + b;
		}

		public static float QuintInOut(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if ((t /= 0.5f) < 1f)
			{
				return c / 2f * t * t * t * t * t + b;
			}
			return c / 2f * ((t -= 2f) * t * t * t * t + 2f) + b;
		}

		public static float QuintInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t * t * t * t + b;
			}
			return c / 2f * ((t -= 2f) * t * t * t * t + 2f) + b;
		}

		public static float QuintOutIn(float t, float b, float c)
		{
			t = Mathf.Clamp01(t);
			if (t < 0.5f)
			{
				return QuintOut(t * 2f, b, c / 2f);
			}
			return QuintIn(t * 2f - 1f, b + c / 2f, c / 2f);
		}

		public static float QuintOutIn(float t, float b, float c, float d)
		{
			if (t < d / 2f)
			{
				return QuintOut(t * 2f, b, c / 2f, d);
			}
			return QuintIn(t * 2f - d, b + c / 2f, c / 2f, d);
		}
	}
}
