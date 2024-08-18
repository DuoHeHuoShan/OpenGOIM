using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Input/Spline Path", ModuleName = "Input Spline Path", Description = "Spline Path")]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/cginputsplinepath")]
	public class InputSplinePath : SplineInputModuleBase, IExternalInput, IOnRequestPath, IOnRequestProcessing
	{
		[HideInInspector]
		[OutputSlotInfo(typeof(CGPath))]
		public CGModuleOutputSlot Path = new CGModuleOutputSlot();

		[Tab("General", Sort = 0)]
		[SerializeField]
		[CGResourceManager("Spline")]
		[FieldCondition("m_Spline", null, false, ActionAttribute.ActionEnum.ShowWarning, "Create or assign spline", ActionAttribute.ActionPositionEnum.Below)]
		private CurvySpline m_Spline;

		public CurvySpline Spline
		{
			get
			{
				return m_Spline;
			}
			set
			{
				if (m_Spline != value)
				{
					m_Spline = value;
				}
				if ((bool)m_Spline)
				{
					m_Spline.OnRefresh.AddListenerOnce(m_Spline_OnRefresh);
					if ((bool)base.StartCP && base.StartCP.Spline != m_Spline)
					{
						base.StartCP = null;
						base.EndCP = null;
					}
				}
				base.Dirty = true;
			}
		}

		public override bool IsInitialized
		{
			get
			{
				return base.IsInitialized && (Spline == null || Spline.IsInitialized);
			}
		}

		public override bool IsConfigured
		{
			get
			{
				return base.IsConfigured && Spline != null;
			}
		}

		public bool SupportsIPE
		{
			get
			{
				return false;
			}
		}

		public float PathLength
		{
			get
			{
				return (!IsConfigured) ? 0f : getPathLength(m_Spline);
			}
		}

		public bool PathIsClosed
		{
			get
			{
				return IsConfigured && getPathClosed(m_Spline);
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if ((bool)Spline)
			{
				Spline.OnRefresh.AddListenerOnce(m_Spline_OnRefresh);
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if ((bool)Spline)
			{
				Spline.OnRefresh.RemoveListener(m_Spline_OnRefresh);
			}
		}

		public override void Reset()
		{
			base.Reset();
			Spline = null;
		}

		public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
		{
			CGDataRequestRasterization requestParameter = GetRequestParameter<CGDataRequestRasterization>(ref requests);
			CGDataRequestMetaCGOptions requestParameter2 = GetRequestParameter<CGDataRequestMetaCGOptions>(ref requests);
			if ((bool)requestParameter2)
			{
				if (requestParameter2.CheckMaterialID)
				{
					requestParameter2.CheckMaterialID = false;
					UIMessages.Add("MaterialID option not supported!");
				}
				if (requestParameter2.IncludeControlPoints)
				{
					requestParameter2.IncludeControlPoints = false;
					UIMessages.Add("IncludeCP option not supported!");
				}
			}
			if (!requestParameter || requestParameter.Length == 0f)
			{
				return null;
			}
			CGData splineData = GetSplineData(Spline, true, requestParameter, requestParameter2);
			return new CGData[1] { splineData };
		}

		public override void Refresh()
		{
			base.Refresh();
		}

		public override void OnTemplateCreated()
		{
			base.OnTemplateCreated();
			if ((bool)Spline && !IsManagedResource(Spline))
			{
				Spline = null;
			}
		}

		private void m_Spline_OnRefresh(CurvySplineEventArgs e)
		{
			if (base.enabled && base.gameObject.activeInHierarchy)
			{
				if (m_Spline == e.Spline)
				{
					base.Dirty = true;
				}
				else
				{
					e.Spline.OnRefresh.RemoveListener(m_Spline_OnRefresh);
				}
			}
		}
	}
}
