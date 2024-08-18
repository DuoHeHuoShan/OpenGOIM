using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Input/Meshes", ModuleName = "Input Meshes", Description = "Create VMeshes")]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/cginputmesh")]
	public class InputMesh : CGModule, IExternalInput
	{
		[HideInInspector]
		[OutputSlotInfo(typeof(CGVMesh), Array = true)]
		public CGModuleOutputSlot OutVMesh = new CGModuleOutputSlot();

		[SerializeField]
		[ArrayEx]
		private List<CGMeshProperties> m_Meshes = new List<CGMeshProperties>(new CGMeshProperties[1]
		{
			new CGMeshProperties()
		});

		public List<CGMeshProperties> Meshes
		{
			get
			{
				return m_Meshes;
			}
		}

		public bool SupportsIPE
		{
			get
			{
				return false;
			}
		}

		public override void Reset()
		{
			base.Reset();
			Meshes.Clear();
			base.Dirty = true;
		}

		public override void Refresh()
		{
			base.Refresh();
			if (!OutVMesh.IsLinked)
			{
				return;
			}
			CGVMesh[] array = new CGVMesh[Meshes.Count];
			int newSize = 0;
			for (int i = 0; i < Meshes.Count; i++)
			{
				if ((bool)Meshes[i].Mesh)
				{
					array[newSize++] = new CGVMesh(Meshes[i]);
				}
			}
			Array.Resize(ref array, newSize);
			OutVMesh.SetData(array);
		}

		public override void OnTemplateCreated()
		{
			base.OnTemplateCreated();
			Meshes.Clear();
		}
	}
}
