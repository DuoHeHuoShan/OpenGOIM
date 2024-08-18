using System;
using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Modifier/TRS Mesh", ModuleName = "TRS Mesh", Description = "Transform,Rotate,Scale a VMesh")]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/cgtrsmesh")]
	public class ModifierTRSMesh : TRSModuleBase
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGVMesh) }, Array = true, ModifiesData = true)]
		public CGModuleInputSlot InVMesh = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGVMesh), Array = true)]
		public CGModuleOutputSlot OutVMesh = new CGModuleOutputSlot();

		public override void Refresh()
		{
			base.Refresh();
			if (OutVMesh.IsLinked)
			{
				List<CGVMesh> allData = InVMesh.GetAllData<CGVMesh>(new CGDataRequestParameter[0]);
				Matrix4x4 matrix = base.Matrix;
				for (int i = 0; i < allData.Count; i++)
				{
					allData[i].TRS(matrix);
				}
				OutVMesh.SetData(allData);
			}
		}
	}
}
