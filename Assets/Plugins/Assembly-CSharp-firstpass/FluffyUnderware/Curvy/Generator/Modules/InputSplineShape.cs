using FluffyUnderware.Curvy.Shapes;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Input/Spline Shape", ModuleName = "Input Spline Shape", Description = "Spline Shape")]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/cginputsplineshape")]
	public class InputSplineShape : SplineInputModuleBase, IExternalInput, IOnRequestPath, IOnRequestProcessing
	{
		[HideInInspector]
		[OutputSlotInfo(typeof(CGShape))]
		public CGModuleOutputSlot OutShape = new CGModuleOutputSlot();

		[Tab("General", Sort = 0)]
		[SerializeField]
		[CGResourceManager("Shape")]
		private CurvySpline m_Shape;

		public CurvySpline Shape
		{
			get
			{
				return m_Shape;
			}
			set
			{
				if (m_Shape != value)
				{
					m_Shape = value;
				}
				if ((bool)m_Shape)
				{
					m_Shape.OnRefresh.AddListenerOnce(m_Shape_OnRefresh);
					m_Shape.RestrictTo2D = true;
					if ((bool)base.StartCP && base.StartCP.Spline != m_Shape)
					{
						base.StartCP = null;
						base.EndCP = null;
					}
				}
				base.Dirty = true;
			}
		}

		public bool FreeForm
		{
			get
			{
				return Shape != null && Shape.GetComponent<CurvyShape>() == null;
			}
			set
			{
				if (Shape != null)
				{
					CurvyShape component = Shape.GetComponent<CurvyShape>();
					if (value && component != null)
					{
						component.Delete();
					}
					else if (!value && component == null)
					{
						Shape.gameObject.AddComponent<CSCircle>();
					}
				}
			}
		}

		public override bool IsInitialized
		{
			get
			{
				return base.IsInitialized && (Shape == null || Shape.IsInitialized);
			}
		}

		public override bool IsConfigured
		{
			get
			{
				return base.IsConfigured && Shape != null;
			}
		}

		public float PathLength
		{
			get
			{
				return (!IsConfigured) ? 0f : getPathLength(m_Shape);
			}
		}

		public bool PathIsClosed
		{
			get
			{
				return IsConfigured && getPathClosed(m_Shape);
			}
		}

		public bool SupportsIPE
		{
			get
			{
				return FreeForm;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Shape = m_Shape;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
		}

		public override void Reset()
		{
			base.Reset();
			Shape = null;
		}

		public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
		{
			CGDataRequestRasterization requestParameter = GetRequestParameter<CGDataRequestRasterization>(ref requests);
			CGDataRequestMetaCGOptions requestParameter2 = GetRequestParameter<CGDataRequestMetaCGOptions>(ref requests);
			if (!requestParameter || requestParameter.Length == 0f)
			{
				return null;
			}
			CGData splineData = GetSplineData(Shape, false, requestParameter, requestParameter2);
			return new CGData[1] { splineData };
		}

		public T SetManagedShape<T>() where T : CurvyShape2D
		{
			if (!Shape)
			{
				Shape = (CurvySpline)AddManagedResource("Shape", string.Empty);
			}
			CurvyShape component = Shape.GetComponent<CurvyShape>();
			if (component != null)
			{
				component.Delete();
			}
			return Shape.gameObject.AddComponent<T>();
		}

		public void RemoveManagedShape()
		{
			if ((bool)Shape)
			{
				DeleteManagedResource("Shape", Shape, string.Empty);
			}
		}

		private void m_Shape_OnRefresh(CurvySplineEventArgs e)
		{
			if (base.enabled && base.gameObject.activeInHierarchy)
			{
				if (m_Shape == e.Spline)
				{
					base.Dirty = true;
				}
				else
				{
					e.Spline.OnRefresh.RemoveListener(m_Shape_OnRefresh);
				}
			}
		}
	}
}
