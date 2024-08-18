using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Modifier/TRS Path", ModuleName = "TRS Path", Description = "Transform,Rotate,Scale a Path")]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/cgtrspath")]
	public class ModifierTRSPath : TRSModuleBase, IOnRequestPath, IOnRequestProcessing
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, Name = "Path A", ModifiesData = true)]
		public CGModuleInputSlot InPath = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGPath))]
		public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();

		public float PathLength
		{
			get
			{
				return (!IsConfigured) ? 0f : InPath.SourceSlot().OnRequestPathModule.PathLength;
			}
		}

		public bool PathIsClosed
		{
			get
			{
				return IsConfigured && InPath.SourceSlot().OnRequestPathModule.PathIsClosed;
			}
		}

		public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
		{
			if (requestedSlot == OutPath)
			{
				CGPath data = InPath.GetData<CGPath>(requests);
				Matrix4x4 matrix = base.Matrix;
				for (int i = 0; i < data.Count; i++)
				{
					data.Position[i] = matrix.MultiplyPoint3x4(data.Position[i]);
				}
				data.Recalculate();
				return new CGData[1] { data };
			}
			return null;
		}
	}
}
