using System;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Build/Volume Mesh", ModuleName = "Volume Mesh", Description = "Build a volume mesh")]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/cgbuildvolumemesh")]
	public class BuildVolumeMesh : CGModule
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGVolume) })]
		public CGModuleInputSlot InVolume = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGVMesh), Array = true)]
		public CGModuleOutputSlot OutVMesh = new CGModuleOutputSlot();

		[Tab("General")]
		[SerializeField]
		private bool m_GenerateUV = true;

		[SerializeField]
		private bool m_Split;

		[Positive(MinValue = 1f)]
		[FieldCondition("m_Split", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private float m_SplitLength = 100f;

		[FieldAction("CBAddMaterial", ActionAttribute.ActionEnum.Callback)]
		[SerializeField]
		[FormerlySerializedAs("m_ReverseNormals")]
		private bool m_ReverseTriOrder;

		[SerializeField]
		[HideInInspector]
		private List<CGMaterialSettingsEx> m_MaterialSettings = new List<CGMaterialSettingsEx>();

		[SerializeField]
		[HideInInspector]
		private Material[] m_Material = new Material[0];

		private List<SamplePointsMaterialGroupCollection> groupsByMatID;

		public bool GenerateUV
		{
			get
			{
				return m_GenerateUV;
			}
			set
			{
				if (m_GenerateUV != value)
				{
					m_GenerateUV = value;
				}
				base.Dirty = true;
			}
		}

		public bool ReverseTriOrder
		{
			get
			{
				return m_ReverseTriOrder;
			}
			set
			{
				if (m_ReverseTriOrder != value)
				{
					m_ReverseTriOrder = value;
				}
				base.Dirty = true;
			}
		}

		public bool Split
		{
			get
			{
				return m_Split;
			}
			set
			{
				if (m_Split != value)
				{
					m_Split = value;
				}
				base.Dirty = true;
			}
		}

		public float SplitLength
		{
			get
			{
				return m_SplitLength;
			}
			set
			{
				float num = Mathf.Max(1f, value);
				if (m_SplitLength != num)
				{
					m_SplitLength = num;
				}
				base.Dirty = true;
			}
		}

		public List<CGMaterialSettingsEx> MaterialSetttings
		{
			get
			{
				return m_MaterialSettings;
			}
		}

		public int MaterialCount
		{
			get
			{
				return m_MaterialSettings.Count;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			if (MaterialCount == 0)
			{
				AddMaterial();
			}
		}

		public override void Reset()
		{
			base.Reset();
			GenerateUV = true;
			Split = false;
			SplitLength = 100f;
			ReverseTriOrder = false;
			m_MaterialSettings = new List<CGMaterialSettingsEx>(new CGMaterialSettingsEx[1]
			{
				new CGMaterialSettingsEx()
			});
			m_Material = new Material[1] { CurvyUtility.GetDefaultMaterial() };
		}

		public override void Refresh()
		{
			base.Refresh();
			CGVolume data = InVolume.GetData<CGVolume>(new CGDataRequestParameter[0]);
			if ((bool)data && data.Count > 0 && data.CrossSize > 0 && data.CrossMaterialGroups.Count > 0)
			{
				List<IntRegion> list = new List<IntRegion>();
				if (Split)
				{
					float num = 0f;
					int num2 = 0;
					for (int i = 0; i < data.Count; i++)
					{
						float num3 = data.FToDistance(data.F[i]);
						if (num3 - num >= SplitLength)
						{
							list.Add(new IntRegion(num2, i));
							num = num3;
							num2 = i;
						}
					}
					if (num2 < data.Count - 1)
					{
						list.Add(new IntRegion(num2, data.Count - 1));
					}
				}
				else
				{
					list.Add(new IntRegion(0, data.Count - 1));
				}
				CGVMesh[] array = OutVMesh.GetAllData<CGVMesh>();
				Array.Resize(ref array, list.Count);
				prepare(data);
				for (int j = 0; j < list.Count; j++)
				{
					array[j] = CGVMesh.Get(array[j], data, list[j], GenerateUV, ReverseTriOrder);
					build(array[j], data, list[j]);
				}
				OutVMesh.SetData(array);
			}
			else
			{
				OutVMesh.SetData((CGData[])null);
			}
		}

		public int AddMaterial()
		{
			m_MaterialSettings.Add(new CGMaterialSettingsEx());
			m_Material = m_Material.Add(CurvyUtility.GetDefaultMaterial());
			base.Dirty = true;
			return MaterialCount;
		}

		public void RemoveMaterial(int index)
		{
			if (validateMaterialIndex(index))
			{
				m_MaterialSettings.RemoveAt(index);
				m_Material = m_Material.RemoveAt(index);
				base.Dirty = true;
			}
		}

		public void SetMaterial(int index, Material mat)
		{
			if (validateMaterialIndex(index) && !(mat == m_Material[index]) && m_Material[index] != mat)
			{
				m_Material[index] = mat;
				base.Dirty = true;
			}
		}

		public Material GetMaterial(int index)
		{
			if (!validateMaterialIndex(index))
			{
				return null;
			}
			return m_Material[index];
		}

		private void prepare(CGVolume vol)
		{
			groupsByMatID = getMaterialIDGroups(vol);
		}

		private void build(CGVMesh vmesh, CGVolume vol, IntRegion subset)
		{
			if (GenerateUV)
			{
				Array.Resize(ref vmesh.UV, vmesh.Count);
			}
			prepareSubMeshes(vmesh, groupsByMatID, subset.Length, ref m_Material);
			int num = 0;
			int[] array = new int[groupsByMatID.Count];
			for (int i = subset.From; i < subset.To; i++)
			{
				for (int j = 0; j < groupsByMatID.Count; j++)
				{
					SamplePointsMaterialGroupCollection samplePointsMaterialGroupCollection = groupsByMatID[j];
					for (int k = 0; k < samplePointsMaterialGroupCollection.Count; k++)
					{
						SamplePointsMaterialGroup grp = samplePointsMaterialGroupCollection[k];
						if (GenerateUV)
						{
							createMaterialGroupUV(ref vmesh, ref vol, ref grp, samplePointsMaterialGroupCollection.MaterialID, samplePointsMaterialGroupCollection.AspectCorrection, i, num);
						}
						for (int l = 0; l < grp.Patches.Count; l++)
						{
							createPatchTriangles(ref vmesh.SubMeshes[j].Triangles, ref array[j], num + grp.Patches[l].Start, grp.Patches[l].Count, vol.CrossSize, ReverseTriOrder);
						}
					}
				}
				num += vol.CrossSize;
			}
			if (!GenerateUV)
			{
				return;
			}
			for (int m = 0; m < groupsByMatID.Count; m++)
			{
				SamplePointsMaterialGroupCollection samplePointsMaterialGroupCollection = groupsByMatID[m];
				for (int n = 0; n < samplePointsMaterialGroupCollection.Count; n++)
				{
					SamplePointsMaterialGroup grp = samplePointsMaterialGroupCollection[n];
					createMaterialGroupUV(ref vmesh, ref vol, ref grp, samplePointsMaterialGroupCollection.MaterialID, samplePointsMaterialGroupCollection.AspectCorrection, subset.To, num);
				}
			}
		}

		private void prepareSubMeshes(CGVMesh vmesh, List<SamplePointsMaterialGroupCollection> groupsBySubMeshes, int extrusions, ref Material[] materials)
		{
			vmesh.SetSubMeshCount(groupsBySubMeshes.Count);
			for (int i = 0; i < groupsBySubMeshes.Count; i++)
			{
				CGVSubMesh data = vmesh.SubMeshes[i];
				vmesh.SubMeshes[i] = CGVSubMesh.Get(data, groupsBySubMeshes[i].TriangleCount * extrusions * 3, materials[Mathf.Min(groupsBySubMeshes[i].MaterialID, materials.Length - 1)]);
			}
		}

		private void createMaterialGroupUV(ref CGVMesh vmesh, ref CGVolume vol, ref SamplePointsMaterialGroup grp, int matIndex, float grpAspectCorrection, int sample, int baseVertex)
		{
			CGMaterialSettingsEx cGMaterialSettingsEx = m_MaterialSettings[matIndex];
			float num = cGMaterialSettingsEx.UVOffset.y + vol.F[sample] * cGMaterialSettingsEx.UVScale.y * grpAspectCorrection;
			int endVertex = grp.EndVertex;
			for (int i = grp.StartVertex; i <= endVertex; i++)
			{
				float num2 = cGMaterialSettingsEx.UVOffset.x + vol.CrossMap[i] * cGMaterialSettingsEx.UVScale.x;
				if (cGMaterialSettingsEx.SwapUV)
				{
					vmesh.UV[baseVertex + i] = new Vector2(num, num2);
				}
				else
				{
					vmesh.UV[baseVertex + i] = new Vector2(num2, num);
				}
			}
		}

		private int createPatchTriangles(ref int[] triangles, ref int triIdx, int curVTIndex, int patchSize, int crossSize, bool reverse)
		{
			int num = (reverse ? 1 : 0);
			int num2 = 1 - num;
			int num3 = curVTIndex + crossSize;
			for (int i = 0; i < patchSize; i++)
			{
				triangles[triIdx + num] = curVTIndex + i;
				triangles[triIdx + num2] = num3 + i;
				triangles[triIdx + 2] = curVTIndex + i + 1;
				triangles[triIdx + num + 3] = curVTIndex + i + 1;
				triangles[triIdx + num2 + 3] = num3 + i;
				triangles[triIdx + 5] = num3 + i + 1;
				triIdx += 6;
			}
			return curVTIndex + patchSize + 1;
		}

		private List<SamplePointsMaterialGroupCollection> getMaterialIDGroups(CGVolume volume)
		{
			Dictionary<int, SamplePointsMaterialGroupCollection> dictionary = new Dictionary<int, SamplePointsMaterialGroupCollection>();
			for (int i = 0; i < volume.CrossMaterialGroups.Count; i++)
			{
				int num = Mathf.Min(volume.CrossMaterialGroups[i].MaterialID, MaterialCount - 1);
				SamplePointsMaterialGroupCollection value;
				if (!dictionary.TryGetValue(num, out value))
				{
					value = new SamplePointsMaterialGroupCollection();
					value.MaterialID = num;
					dictionary.Add(num, value);
				}
				value.Add(volume.CrossMaterialGroups[i]);
			}
			List<SamplePointsMaterialGroupCollection> list = new List<SamplePointsMaterialGroupCollection>();
			foreach (SamplePointsMaterialGroupCollection value2 in dictionary.Values)
			{
				value2.CalculateAspectCorrection(volume, MaterialSetttings[value2.MaterialID]);
				list.Add(value2);
			}
			return list;
		}

		private bool validateMaterialIndex(int index)
		{
			if (index < 0 || index >= m_MaterialSettings.Count)
			{
				Debug.LogError("TriangulateTube: Invalid Material Index!");
				return false;
			}
			return true;
		}
	}
}
