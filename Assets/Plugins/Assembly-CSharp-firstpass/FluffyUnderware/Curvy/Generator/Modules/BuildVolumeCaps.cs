using System;
using System.Collections.Generic;
using FluffyUnderware.Curvy.ThirdParty.LibTessDotNet;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Build/Volume Caps", ModuleName = "Volume Caps", Description = "Build volume caps")]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/cgbuildvolumecaps")]
	public class BuildVolumeCaps : CGModule
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGVolume) })]
		public CGModuleInputSlot InVolume = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGVolume) }, Optional = true, Array = true)]
		public CGModuleInputSlot InVolumeHoles = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGVMesh), Array = true)]
		public CGModuleOutputSlot OutVMesh = new CGModuleOutputSlot();

		[Tab("General")]
		[SerializeField]
		private CGYesNoAuto m_StartCap = CGYesNoAuto.Auto;

		[SerializeField]
		private CGYesNoAuto m_EndCap = CGYesNoAuto.Auto;

		[SerializeField]
		[FormerlySerializedAs("m_ReverseNormals")]
		private bool m_ReverseTriOrder;

		[SerializeField]
		private bool m_GenerateUV = true;

		[Tab("Start Cap")]
		[Inline]
		[SerializeField]
		private CGMaterialSettings m_StartMaterialSettings = new CGMaterialSettings();

		[Label("Material", "")]
		[SerializeField]
		private Material m_StartMaterial;

		[Tab("End Cap")]
		[SerializeField]
		private bool m_CloneStartCap = true;

		[AsGroup(null, Invisible = true)]
		[GroupCondition("m_CloneStartCap", false, false)]
		[SerializeField]
		private CGMaterialSettings m_EndMaterialSettings = new CGMaterialSettings();

		[Group("Default/End Cap")]
		[Label("Material", "")]
		[FieldCondition("m_CloneStartCap", false, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private Material m_EndMaterial;

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

		public CGYesNoAuto StartCap
		{
			get
			{
				return m_StartCap;
			}
			set
			{
				if (m_StartCap != value)
				{
					m_StartCap = value;
				}
				base.Dirty = true;
			}
		}

		public Material StartMaterial
		{
			get
			{
				return m_StartMaterial;
			}
			set
			{
				if (m_StartMaterial != value)
				{
					m_StartMaterial = value;
				}
				base.Dirty = true;
			}
		}

		public CGMaterialSettings StartMaterialSettings
		{
			get
			{
				return m_StartMaterialSettings;
			}
		}

		public CGYesNoAuto EndCap
		{
			get
			{
				return m_EndCap;
			}
			set
			{
				if (m_EndCap != value)
				{
					m_EndCap = value;
				}
				base.Dirty = true;
			}
		}

		public bool CloneStartCap
		{
			get
			{
				return m_CloneStartCap;
			}
			set
			{
				if (m_CloneStartCap != value)
				{
					m_CloneStartCap = value;
				}
				base.Dirty = true;
			}
		}

		public CGMaterialSettings EndMaterialSettings
		{
			get
			{
				return m_EndMaterialSettings;
			}
		}

		public Material EndMaterial
		{
			get
			{
				return m_EndMaterial;
			}
			set
			{
				if (m_EndMaterial != value)
				{
					m_EndMaterial = value;
				}
				base.Dirty = true;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			if (StartMaterial == null)
			{
				StartMaterial = CurvyUtility.GetDefaultMaterial();
			}
			if (EndMaterial == null)
			{
				EndMaterial = CurvyUtility.GetDefaultMaterial();
			}
		}

		public override void Reset()
		{
			base.Reset();
			StartCap = CGYesNoAuto.Auto;
			EndCap = CGYesNoAuto.Auto;
			ReverseTriOrder = false;
			GenerateUV = true;
			m_StartMaterialSettings = new CGMaterialSettings();
			m_EndMaterialSettings = new CGMaterialSettings();
		}

		public override void Refresh()
		{
			base.Refresh();
			CGVolume data = InVolume.GetData<CGVolume>(new CGDataRequestParameter[0]);
			List<CGVolume> allData = InVolumeHoles.GetAllData<CGVolume>(new CGDataRequestParameter[0]);
			if (!data)
			{
				return;
			}
			bool flag = StartCap == CGYesNoAuto.Yes || (StartCap == CGYesNoAuto.Auto && !data.Seamless);
			bool flag2 = EndCap == CGYesNoAuto.Yes || (EndCap == CGYesNoAuto.Auto && !data.Seamless);
			if (!flag && !flag2)
			{
				OutVMesh.SetData((CGData[])null);
				return;
			}
			CGVMesh cGVMesh = new CGVMesh();
			Vector3[] array = new Vector3[0];
			Vector3[] array2 = new Vector3[0];
			cGVMesh.AddSubMesh(new CGVSubMesh());
			CGVSubMesh cGVSubMesh = cGVMesh.SubMeshes[0];
			if (flag)
			{
				Tess tess = new Tess();
				tess.UsePooling = true;
				tess.AddContour(make2DSegment(data, 0));
				for (int i = 0; i < allData.Count; i++)
				{
					if (allData[i].Count < 3)
					{
						OutVMesh.SetData((CGData[])null);
						UIMessages.Add("Hole Cross has <3 Vertices: Can't create Caps!");
						return;
					}
					tess.AddContour(make2DSegment(allData[i], 0));
				}
				tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
				array = UnityLibTessUtility.FromContourVertex(tess.Vertices);
				Bounds bounds;
				cGVMesh.Vertex = applyMat(array, getMat(data, 0, true), out bounds);
				cGVSubMesh.Material = StartMaterial;
				cGVSubMesh.Triangles = tess.Elements;
				if (ReverseTriOrder)
				{
					flipTris(ref cGVSubMesh.Triangles, 0, cGVSubMesh.Triangles.Length);
				}
				if (GenerateUV)
				{
					cGVMesh.UV = new Vector2[array.Length];
					applyUV(array, ref cGVMesh.UV, 0, array.Length, StartMaterialSettings, bounds);
				}
			}
			if (flag2)
			{
				Tess tess2 = new Tess();
				tess2.UsePooling = true;
				tess2.AddContour(make2DSegment(data, 0));
				for (int j = 0; j < allData.Count; j++)
				{
					if (allData[j].Count < 3)
					{
						OutVMesh.SetData((CGData[])null);
						UIMessages.Add("Hole Cross has <3 Vertices: Can't create Caps!");
						return;
					}
					tess2.AddContour(make2DSegment(allData[j], 0));
				}
				tess2.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
				array2 = UnityLibTessUtility.FromContourVertex(tess2.Vertices);
				int num = cGVMesh.Vertex.Length;
				Bounds bounds2;
				cGVMesh.Vertex = cGVMesh.Vertex.AddRange(applyMat(array2, getMat(data, data.Count - 1, true), out bounds2));
				int[] indices = tess2.Elements;
				if (!ReverseTriOrder)
				{
					flipTris(ref indices, 0, indices.Length);
				}
				for (int k = 0; k < indices.Length; k++)
				{
					indices[k] += num;
				}
				if (!CloneStartCap && StartMaterial != EndMaterial)
				{
					cGVMesh.AddSubMesh(new CGVSubMesh(indices, EndMaterial));
				}
				else
				{
					cGVSubMesh.Material = StartMaterial;
					cGVSubMesh.Triangles = cGVSubMesh.Triangles.AddRange(indices);
				}
				if (GenerateUV)
				{
					Array.Resize(ref cGVMesh.UV, cGVMesh.UV.Length + array2.Length);
					applyUV(array2, ref cGVMesh.UV, array.Length, array2.Length, (!CloneStartCap) ? EndMaterialSettings : StartMaterialSettings, bounds2);
				}
			}
			OutVMesh.SetData(cGVMesh);
		}

		private Matrix4x4 getMat(CGVolume vol, int index, bool inverse)
		{
			if (inverse)
			{
				Quaternion q = Quaternion.LookRotation(vol.Direction[index], vol.Normal[index]);
				return Matrix4x4.TRS(vol.Position[index], q, Vector3.one);
			}
			Quaternion quaternion = Quaternion.Inverse(Quaternion.LookRotation(vol.Direction[index], vol.Normal[index]));
			return Matrix4x4.TRS(-(quaternion * vol.Position[index]), quaternion, Vector3.one);
		}

		private void flipTris(ref int[] indices, int start, int end)
		{
			for (int i = start; i < end; i += 3)
			{
				int num = indices[i];
				indices[i] = indices[i + 2];
				indices[i + 2] = num;
			}
		}

		private Vector3[] applyMat(Vector3[] vt, Matrix4x4 mat, out Bounds bounds)
		{
			Vector3[] array = new Vector3[vt.Length];
			float num = float.MaxValue;
			float num2 = float.MaxValue;
			float num3 = float.MinValue;
			float num4 = float.MinValue;
			for (int i = 0; i < vt.Length; i++)
			{
				num = Mathf.Min(vt[i].x, num);
				num2 = Mathf.Min(vt[i].y, num2);
				num3 = Mathf.Max(vt[i].x, num3);
				num4 = Mathf.Max(vt[i].y, num4);
				array[i] = mat.MultiplyPoint(vt[i]);
			}
			Vector3 size = new Vector3(Mathf.Abs(num3 - num), Mathf.Abs(num4 - num2));
			bounds = new Bounds(new Vector3(num + size.x / 2f, num2 + size.y / 2f, 0f), size);
			return array;
		}

		private ContourVertex[] make2DSegment(CGVolume vol, int index)
		{
			Matrix4x4 mat = getMat(vol, index, false);
			int segmentIndex = vol.GetSegmentIndex(index);
			ContourVertex[] array = new ContourVertex[vol.CrossSize];
			for (int i = 0; i < vol.CrossSize; i++)
			{
				array[i] = mat.MultiplyPoint(vol.Vertex[segmentIndex + i]).ContourVertex();
			}
			return array;
		}

		private void applyUV(Vector3[] vts, ref Vector2[] uvArray, int index, int count, CGMaterialSettings mat, Bounds bounds)
		{
			float x = bounds.size.x;
			float y = bounds.size.y;
			float x2 = bounds.min.x;
			float y2 = bounds.min.y;
			float num = mat.UVScale.x;
			float num2 = mat.UVScale.y;
			switch (mat.KeepAspect)
			{
			case CGKeepAspectMode.ScaleU:
			{
				float num5 = x * mat.UVScale.x;
				float num6 = y * mat.UVScale.y;
				num *= num5 / num6;
				break;
			}
			case CGKeepAspectMode.ScaleV:
			{
				float num3 = x * mat.UVScale.x;
				float num4 = y * mat.UVScale.y;
				num2 *= num4 / num3;
				break;
			}
			}
			if (mat.UVRotation != 0f)
			{
				float f = mat.UVRotation * ((float)Math.PI / 180f);
				float num7 = Mathf.Sin(f);
				float num8 = Mathf.Cos(f);
				float num9 = num * 0.5f;
				float num10 = num2 * 0.5f;
				for (int i = 0; i < count; i++)
				{
					float num11 = (vts[i].x - x2) / x * num;
					float num12 = (vts[i].y - y2) / y * num2;
					float num13 = num11 - num9;
					float num14 = num12 - num10;
					num11 = num8 * num13 - num7 * num14 + num9 + mat.UVOffset.x;
					num12 = num7 * num13 + num8 * num14 + num10 + mat.UVOffset.y;
					uvArray[i + index] = ((!mat.SwapUV) ? new Vector2(num11, num12) : new Vector2(num12, num11));
				}
			}
			else
			{
				for (int j = 0; j < count; j++)
				{
					float num11 = mat.UVOffset.x + (vts[j].x - x2) / x * num;
					float num12 = mat.UVOffset.y + (vts[j].y - y2) / y * num2;
					uvArray[j + index] = ((!mat.SwapUV) ? new Vector2(num11, num12) : new Vector2(num12, num11));
				}
			}
		}
	}
}
