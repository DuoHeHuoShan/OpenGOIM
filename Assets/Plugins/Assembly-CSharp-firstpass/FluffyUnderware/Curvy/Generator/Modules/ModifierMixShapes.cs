using System;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Modifier/Mix Shapes", ModuleName = "Mix Shapes", Description = "Lerps between two shapes")]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/cgmixshapes")]
	public class ModifierMixShapes : CGModule, IOnRequestPath, IOnRequestProcessing
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGShape) }, Name = "Shape A")]
		public CGModuleInputSlot InShapeA = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGShape) }, Name = "Shape B")]
		public CGModuleInputSlot InShapeB = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGShape))]
		public CGModuleOutputSlot OutShape = new CGModuleOutputSlot();

		[SerializeField]
		[RangeEx(-1f, 1f, "", "", Tooltip = "Mix between the paths")]
		private float m_Mix;

		public float Mix
		{
			get
			{
				return m_Mix;
			}
			set
			{
				if (m_Mix != value)
				{
					m_Mix = value;
				}
				base.Dirty = true;
			}
		}

		public float PathLength
		{
			get
			{
				return (!IsConfigured) ? 0f : Mathf.Max(InShapeA.SourceSlot().OnRequestPathModule.PathLength, InShapeB.SourceSlot().OnRequestPathModule.PathLength);
			}
		}

		public bool PathIsClosed
		{
			get
			{
				return IsConfigured && InShapeA.SourceSlot().OnRequestPathModule.PathIsClosed && InShapeB.SourceSlot().OnRequestPathModule.PathIsClosed;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 200f;
			Properties.LabelWidth = 50f;
		}

		public override void Reset()
		{
			base.Reset();
			Mix = 0f;
		}

		public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
		{
			CGDataRequestRasterization requestParameter = GetRequestParameter<CGDataRequestRasterization>(ref requests);
			if (!requestParameter)
			{
				return null;
			}
			CGShape data = InShapeA.GetData<CGShape>(requests);
			CGShape data2 = InShapeB.GetData<CGShape>(requests);
			CGPath cGPath = new CGPath();
			CGShape cGShape;
			CGShape cGShape2;
			if (data.Count > data2.Count)
			{
				cGShape = data;
				cGShape2 = data2;
			}
			else
			{
				cGShape = data2;
				cGShape2 = data;
			}
			Vector3[] array = new Vector3[cGShape.Count];
			Vector3[] array2 = new Vector3[cGShape.Count];
			float t = (Mix + 1f) / 2f;
			for (int i = 0; i < cGShape.Count; i++)
			{
				array[i] = Vector3.Lerp(cGShape.Position[i], cGShape2.InterpolatePosition(cGShape.F[i]), t);
				array2[i] = Vector3.Lerp(cGShape.Normal[i], cGShape2.Normal[i], t);
			}
			cGPath.F = cGShape.F;
			cGPath.Position = array;
			cGPath.Normal = array2;
			return new CGData[1] { cGPath };
		}
	}
}
