using UnityEngine;
using UnityEngine.UI;

namespace I2.Loc
{
	public class LocalizeTarget_UnityStd_GUIText : LocalizeTarget<Text>
	{
		public TextAnchor mAlignment_RTL = TextAnchor.MiddleRight;

		public TextAnchor mAlignment_LTR;

		public bool mAlignmentWasRTL;

		public bool mInitializeAlignment = true;

		static LocalizeTarget_UnityStd_GUIText()
		{
			AutoRegister();
		}

		[RuntimeInitializeOnLoadMethod]
		private static void AutoRegister()
		{
			LocalizationManager.RegisterTarget(new LocalizeTarget_UnityStd_GUIText());
		}

		public override string GetName()
		{
			return "GUIText";
		}

		public override bool CanUseSecondaryTerm()
		{
			return true;
		}

		public override bool AllowMainTermToBeRTL()
		{
			return true;
		}

		public override bool AllowSecondTermToBeRTL()
		{
			return false;
		}

		public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
		{
			Text target = GetTarget(cmp);
			primaryTerm = target.text;
			secondaryTerm = (!string.IsNullOrEmpty(Secondary) || !(target.font != null)) ? null : target.font.name;
		}

		public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
		{
			Text target = GetTarget(cmp);
			Font secondaryTranslatedObj = cmp.GetSecondaryTranslatedObj<Font>(ref mainTranslation, ref secondaryTranslation);
			if (secondaryTranslatedObj != null && target.font != secondaryTranslatedObj)
			{
				target.font = secondaryTranslatedObj;
			}
			if (mInitializeAlignment)
			{
				mInitializeAlignment = false;
				mAlignment_LTR = mAlignment_RTL = target.alignment;
				if (LocalizationManager.IsRight2Left && mAlignment_RTL == TextAnchor.MiddleRight)
				{
					mAlignment_LTR = TextAnchor.MiddleLeft;
				}
				if (!LocalizationManager.IsRight2Left && mAlignment_LTR == TextAnchor.MiddleLeft)
				{
					mAlignment_RTL = TextAnchor.MiddleRight;
				}
			}
			if (mainTranslation != null && target.text != mainTranslation)
			{
				if (cmp.CorrectAlignmentForRTL && target.alignment != TextAnchor.MiddleCenter)
				{
					target.alignment = (!LocalizationManager.IsRight2Left) ? mAlignment_LTR : mAlignment_RTL;
				}
				target.text = mainTranslation;
			}
		}
	}
}
