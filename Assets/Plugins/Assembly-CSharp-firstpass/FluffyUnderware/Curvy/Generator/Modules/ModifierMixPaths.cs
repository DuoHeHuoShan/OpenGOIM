using System;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Modifier/Mix Paths", ModuleName = "Mix Paths", Description = "Lerps between two paths")]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/cgmixpaths")]
	public class ModifierMixPaths : CGModule, IOnRequestPath, IOnRequestProcessing
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, Name = "Path A")]
		public CGModuleInputSlot InPathA = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, Name = "Path B")]
		public CGModuleInputSlot InPathB = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGPath))]
		public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();

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
				return (!IsConfigured) ? 0f : Mathf.Max(InPathA.SourceSlot().OnRequestPathModule.PathLength, InPathB.SourceSlot().OnRequestPathModule.PathLength);
			}
		}

		public bool PathIsClosed
		{
			get
			{
				return IsConfigured && InPathA.SourceSlot().OnRequestPathModule.PathIsClosed && InPathB.SourceSlot().OnRequestPathModule.PathIsClosed;
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
			CGPath data = InPathA.GetData<CGPath>(requests);
			CGPath data2 = InPathB.GetData<CGPath>(requests);
			return new CGData[1] { MixPath(data, data2, Mix) };
		}

		public static CGPath MixPath(CGPath pathA, CGPath pathB, float mix)
		{
			CGPath cGPath = new CGPath();
			CGPath cGPath2;
			CGPath cGPath3;
			if (pathA.Count > pathB.Count)
			{
				cGPath2 = pathA;
				cGPath3 = pathB;
			}
			else
			{
				cGPath2 = pathB;
				cGPath3 = pathA;
			}
			Vector3[] array = new Vector3[cGPath2.Count];
			Vector3[] array2 = new Vector3[cGPath2.Count];
			Vector3[] array3 = new Vector3[cGPath2.Count];
			float t = (mix + 1f) / 2f;
			for (int i = 0; i < cGPath2.Count; i++)
			{
				Vector3 pos;
				Vector3 dir;
				Vector3 up;
				cGPath3.Interpolate(cGPath2.F[i], out pos, out dir, out up);
				array[i] = Vector3.Lerp(cGPath2.Position[i], pos, t);
				array2[i] = Vector3.Lerp(cGPath2.Direction[i], dir, t);
				array3[i] = Vector3.Lerp(cGPath2.Normal[i], up, t);
			}
			cGPath.F = cGPath2.F;
			cGPath.Position = array;
			cGPath.Direction = array2;
			cGPath.Normal = array3;
			cGPath.Length = Mathf.Lerp(cGPath2.Length, cGPath3.Length, t);
			return cGPath;
		}
	}
}
