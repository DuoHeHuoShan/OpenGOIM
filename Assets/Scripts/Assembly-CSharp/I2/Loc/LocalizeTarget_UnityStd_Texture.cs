using UnityEngine;
using UnityEngine.UI;

namespace I2.Loc
{
	public class LocalizeTarget_UnityStd_Texture : LocalizeTarget<Image>
	{
		static LocalizeTarget_UnityStd_Texture()
		{
			AutoRegister();
		}

		[RuntimeInitializeOnLoadMethod]
		private static void AutoRegister()
		{
			LocalizationManager.RegisterTarget(new LocalizeTarget_UnityStd_Texture());
		}

		public override string GetName()
		{
			return "GUITexture";
		}

		public override bool CanUseSecondaryTerm()
		{
			return false;
		}

		public override bool AllowMainTermToBeRTL()
		{
			return false;
		}

		public override bool AllowSecondTermToBeRTL()
		{
			return false;
		}

		public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
		{
			Image target = GetTarget(cmp);
			primaryTerm = ((!target.sprite) ? string.Empty : target.sprite.name);
			secondaryTerm = null;
		}

		public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
		{
			Image target = GetTarget(cmp);
			Sprite texture = target.sprite;
			if (texture != null && texture.name != mainTranslation)
			{
				target.sprite = cmp.FindTranslatedObject<Sprite>(mainTranslation);
			}
		}
	}
}
