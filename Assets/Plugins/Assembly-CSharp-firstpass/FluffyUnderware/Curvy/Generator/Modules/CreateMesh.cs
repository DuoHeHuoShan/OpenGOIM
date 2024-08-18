using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;
using UnityEngine.Rendering;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Create/Mesh", ModuleName = "Create Mesh")]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/cgcreatemesh")]
	public class CreateMesh : CGModule
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGVMesh) }, Array = true, Name = "VMesh")]
		public CGModuleInputSlot InVMeshArray = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGSpots) }, Name = "Spots", Optional = true)]
		public CGModuleInputSlot InSpots = new CGModuleInputSlot();

		[SerializeField]
		[CGResourceCollectionManager("Mesh", ShowCount = true)]
		private CGMeshResourceCollection m_MeshResources = new CGMeshResourceCollection();

		[Tab("General")]
		[Tooltip("Merge meshes")]
		[SerializeField]
		private bool m_Combine;

		[Tooltip("Merge meshes sharing the same Index")]
		[FieldCondition("canGroupMeshes", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		[SerializeField]
		private bool m_GroupMeshes = true;

		[SerializeField]
		private CGYesNoAuto m_AddNormals = CGYesNoAuto.Auto;

		[SerializeField]
		private CGYesNoAuto m_AddTangents = CGYesNoAuto.No;

		[SerializeField]
		private bool m_AddUV2 = true;

		[SerializeField]
		private bool m_MakeStatic;

		[SerializeField]
		[Layer("", "")]
		private int m_Layer;

		[Tab("Renderer")]
		[SerializeField]
		private ShadowCastingMode m_CastShadows = ShadowCastingMode.On;

		[SerializeField]
		private bool m_ReceiveShadows = true;

		[SerializeField]
		private LightProbeUsage m_LightProbeUsage = LightProbeUsage.BlendProbes;

		[HideInInspector]
		[SerializeField]
		private bool m_UseLightProbes = true;

		[SerializeField]
		private ReflectionProbeUsage m_ReflectionProbes = ReflectionProbeUsage.BlendProbes;

		[SerializeField]
		private Transform m_AnchorOverride;

		[Tab("Collider")]
		[SerializeField]
		private CGColliderEnum m_Collider = CGColliderEnum.Mesh;

		[FieldCondition("m_Collider", CGColliderEnum.Mesh, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private bool m_Convex;

		[FieldCondition("m_Collider", CGColliderEnum.None, true, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		[Label("Auto Update", "")]
		[SerializeField]
		private bool m_AutoUpdateColliders = true;

		[FieldCondition("m_Collider", CGColliderEnum.None, true, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		[SerializeField]
		private PhysicMaterial m_Material;

		private int mCurrentMeshCount;

		public bool Combine
		{
			get
			{
				return m_Combine;
			}
			set
			{
				if (m_Combine != value)
				{
					m_Combine = value;
				}
				base.Dirty = true;
			}
		}

		public bool GroupMeshes
		{
			get
			{
				return m_GroupMeshes;
			}
			set
			{
				if (m_GroupMeshes != value)
				{
					m_GroupMeshes = value;
				}
				base.Dirty = true;
			}
		}

		public CGYesNoAuto AddNormals
		{
			get
			{
				return m_AddNormals;
			}
			set
			{
				if (m_AddNormals != value)
				{
					m_AddNormals = value;
				}
				base.Dirty = true;
			}
		}

		public CGYesNoAuto AddTangents
		{
			get
			{
				return m_AddTangents;
			}
			set
			{
				if (m_AddTangents != value)
				{
					m_AddTangents = value;
				}
				base.Dirty = true;
			}
		}

		public bool AddUV2
		{
			get
			{
				return m_AddUV2;
			}
			set
			{
				if (m_AddUV2 != value)
				{
					m_AddUV2 = value;
				}
				base.Dirty = true;
			}
		}

		public int Layer
		{
			get
			{
				return m_Layer;
			}
			set
			{
				int num = Mathf.Clamp(value, 0, 32);
				if (m_Layer != num)
				{
					m_Layer = num;
				}
				base.Dirty = true;
			}
		}

		public bool MakeStatic
		{
			get
			{
				return m_MakeStatic;
			}
			set
			{
				if (m_MakeStatic != value)
				{
					m_MakeStatic = value;
				}
				base.Dirty = true;
			}
		}

		public ShadowCastingMode CastShadows
		{
			get
			{
				return m_CastShadows;
			}
			set
			{
				if (m_CastShadows != value)
				{
					m_CastShadows = value;
				}
				base.Dirty = true;
			}
		}

		public bool ReceiveShadows
		{
			get
			{
				return m_ReceiveShadows;
			}
			set
			{
				if (m_ReceiveShadows != value)
				{
					m_ReceiveShadows = value;
				}
				base.Dirty = true;
			}
		}

		public bool UseLightProbes
		{
			get
			{
				return m_UseLightProbes;
			}
			set
			{
				if (m_UseLightProbes != value)
				{
					m_UseLightProbes = value;
				}
				base.Dirty = true;
			}
		}

		public LightProbeUsage LightProbeUsage
		{
			get
			{
				return m_LightProbeUsage;
			}
			set
			{
				if (m_LightProbeUsage != value)
				{
					m_LightProbeUsage = value;
				}
				base.Dirty = true;
			}
		}

		public ReflectionProbeUsage ReflectionProbes
		{
			get
			{
				return m_ReflectionProbes;
			}
			set
			{
				if (m_ReflectionProbes != value)
				{
					m_ReflectionProbes = value;
				}
				base.Dirty = true;
			}
		}

		public Transform AnchorOverride
		{
			get
			{
				return m_AnchorOverride;
			}
			set
			{
				if (m_AnchorOverride != value)
				{
					m_AnchorOverride = value;
				}
				base.Dirty = true;
			}
		}

		public CGColliderEnum Collider
		{
			get
			{
				return m_Collider;
			}
			set
			{
				if (m_Collider != value)
				{
					m_Collider = value;
				}
				base.Dirty = true;
			}
		}

		public bool AutoUpdateColliders
		{
			get
			{
				return m_AutoUpdateColliders;
			}
			set
			{
				if (m_AutoUpdateColliders != value)
				{
					m_AutoUpdateColliders = value;
				}
				base.Dirty = true;
			}
		}

		public bool Convex
		{
			get
			{
				return m_Convex;
			}
			set
			{
				if (m_Convex != value)
				{
					m_Convex = value;
				}
				base.Dirty = true;
			}
		}

		public PhysicMaterial Material
		{
			get
			{
				return m_Material;
			}
			set
			{
				if (m_Material != value)
				{
					m_Material = value;
				}
				base.Dirty = true;
			}
		}

		public CGMeshResourceCollection Meshes
		{
			get
			{
				return m_MeshResources;
			}
		}

		public int MeshCount
		{
			get
			{
				return Meshes.Count;
			}
		}

		public int VertexCount { get; private set; }

		private bool canGroupMeshes
		{
			get
			{
				return InSpots.IsLinked && m_Combine;
			}
		}

		public override void Reset()
		{
			base.Reset();
			Combine = false;
			GroupMeshes = true;
			AddNormals = CGYesNoAuto.Auto;
			AddTangents = CGYesNoAuto.No;
			MakeStatic = false;
			Material = null;
			Convex = false;
			Layer = 0;
			CastShadows = ShadowCastingMode.On;
			ReceiveShadows = true;
			UseLightProbes = true;
			LightProbeUsage = LightProbeUsage.BlendProbes;
			ReflectionProbes = ReflectionProbeUsage.BlendProbes;
			AnchorOverride = null;
			Collider = CGColliderEnum.Mesh;
			AutoUpdateColliders = true;
			Convex = false;
			Clear();
		}

		public override void OnTemplateCreated()
		{
			Clear();
		}

		public void Clear()
		{
			mCurrentMeshCount = 0;
			removeUnusedResource();
			Resources.UnloadUnusedAssets();
		}

		public override void OnStateChange()
		{
			base.OnStateChange();
			if (!IsConfigured)
			{
				Clear();
			}
		}

		public override void Refresh()
		{
			base.Refresh();
			if (Application.isPlaying && MakeStatic)
			{
				return;
			}
			List<CGVMesh> vMeshes = InVMeshArray.GetAllData<CGVMesh>(new CGDataRequestParameter[0]);
			CGSpots spots = InSpots.GetData<CGSpots>(new CGDataRequestParameter[0]);
			mCurrentMeshCount = 0;
			VertexCount = 0;
			if (vMeshes.Count > 0 && (!InSpots.IsLinked || (spots != null && spots.Count > 0)))
			{
				if (spots != null && spots.Count > 0)
				{
					createSpotMeshes(ref vMeshes, ref spots, Combine);
				}
				else
				{
					createMeshes(ref vMeshes, Combine);
				}
			}
			removeUnusedResource();
			if (AutoUpdateColliders)
			{
				UpdateColliders();
			}
		}

		public GameObject SaveToScene(Transform parent = null)
		{
			List<Component> components;
			List<string> componentNames;
			GetManagedResources(out components, out componentNames);
			if (components.Count == 0)
			{
				return null;
			}
			if (components.Count > 1)
			{
				Transform transform = new GameObject(base.ModuleName).transform;
				transform.transform.parent = ((!(parent == null)) ? parent : base.Generator.transform.parent);
				for (int i = 0; i < components.Count; i++)
				{
					MeshFilter component = components[i].GetComponent<MeshFilter>();
					GameObject gameObject = components[i].gameObject.DuplicateGameObject(transform.transform);
					gameObject.name = components[i].name;
					gameObject.GetComponent<CGMeshResource>().Destroy();
					gameObject.GetComponent<MeshFilter>().sharedMesh = UnityEngine.Object.Instantiate(component.sharedMesh);
				}
				return transform.gameObject;
			}
			MeshFilter component2 = components[0].GetComponent<MeshFilter>();
			GameObject gameObject2 = components[0].gameObject.DuplicateGameObject(parent);
			gameObject2.name = components[0].name;
			gameObject2.GetComponent<CGMeshResource>().Destroy();
			gameObject2.GetComponent<MeshFilter>().sharedMesh = UnityEngine.Object.Instantiate(component2.sharedMesh);
			return gameObject2;
		}

		public void UpdateColliders()
		{
			bool flag = true;
			for (int i = 0; i < m_MeshResources.Count; i++)
			{
				if (!m_MeshResources.Items[i].UpdateCollider(Collider, Convex, Material))
				{
					flag = false;
				}
			}
			if (!flag)
			{
				UIMessages.Add("Error setting collider!");
			}
		}

		protected override bool UpgradeVersion(string oldVersion, string newVersion)
		{
			return true;
		}

		private void createMeshes(ref List<CGVMesh> vMeshes, bool combine)
		{
			int i = 0;
			if (combine)
			{
				sortByVertexCount(ref vMeshes);
				for (; vMeshes[i].Count > 65534; i++)
				{
				}
				CGVMesh vmesh = ((vMeshes.Count != 1) ? new CGVMesh(vMeshes[0]) : vMeshes[0]);
				for (int j = i + 1; j < vMeshes.Count; j++)
				{
					if (vmesh.Count + vMeshes[j].Count > 65534)
					{
						writeVMeshToMesh(ref vmesh);
						vmesh = ((j >= vMeshes.Count - 1) ? new CGVMesh(vMeshes[j]) : vMeshes[j]);
					}
					else
					{
						vmesh.MergeVMesh(vMeshes[j]);
					}
				}
				writeVMeshToMesh(ref vmesh);
			}
			else
			{
				for (int k = 0; k < vMeshes.Count; k++)
				{
					if (vMeshes[k].Count < 65535)
					{
						CGVMesh vmesh2 = vMeshes[k];
						writeVMeshToMesh(ref vmesh2);
					}
					else
					{
						i++;
					}
				}
			}
			if (i > 0)
			{
				UIMessages.Add(string.Format("{0} meshes skipped (VertexCount>65534)", i));
			}
		}

		private void createSpotMeshes(ref List<CGVMesh> vMeshes, ref CGSpots spots, bool combine)
		{
			int num = 0;
			int count = vMeshes.Count;
			if (combine)
			{
				List<CGSpot> list = new List<CGSpot>(spots.Points);
				if (GroupMeshes)
				{
					list.Sort((CGSpot a, CGSpot b) => a.Index.CompareTo(b.Index));
				}
				CGSpot cGSpot = list[0];
				CGVMesh vmesh = new CGVMesh(vMeshes[cGSpot.Index]);
				if (cGSpot.Position != Vector3.zero || cGSpot.Rotation != Quaternion.identity || cGSpot.Scale != Vector3.one)
				{
					vmesh.TRS(cGSpot.Matrix);
				}
				for (int i = 1; i < list.Count; i++)
				{
					cGSpot = list[i];
					if (cGSpot.Index <= -1 || cGSpot.Index >= count)
					{
						continue;
					}
					if (vmesh.Count + vMeshes[cGSpot.Index].Count > 65534 || (GroupMeshes && cGSpot.Index != list[i - 1].Index))
					{
						writeVMeshToMesh(ref vmesh);
						vmesh = new CGVMesh(vMeshes[cGSpot.Index]);
						if (!cGSpot.Matrix.isIdentity)
						{
							vmesh.TRS(cGSpot.Matrix);
						}
					}
					else if (!cGSpot.Matrix.isIdentity)
					{
						vmesh.MergeVMesh(vMeshes[cGSpot.Index], cGSpot.Matrix);
					}
					else
					{
						vmesh.MergeVMesh(vMeshes[cGSpot.Index]);
					}
				}
				writeVMeshToMesh(ref vmesh);
			}
			else
			{
				for (int j = 0; j < spots.Count; j++)
				{
					CGSpot cGSpot = spots.Points[j];
					if (cGSpot.Index <= -1 || cGSpot.Index >= count)
					{
						continue;
					}
					if (vMeshes[cGSpot.Index].Count < 65535)
					{
						CGVMesh vmesh2 = vMeshes[cGSpot.Index];
						CGMeshResource cGMeshResource = writeVMeshToMesh(ref vmesh2);
						if (cGSpot.Position != Vector3.zero || cGSpot.Rotation != Quaternion.identity || cGSpot.Scale != Vector3.one)
						{
							cGSpot.ToTransform(cGMeshResource.Filter.transform);
						}
					}
					else
					{
						num++;
					}
				}
			}
			if (num > 0)
			{
				UIMessages.Add(string.Format("{0} meshes skipped (VertexCount>65534)", num));
			}
		}

		private CGMeshResource writeVMeshToMesh(ref CGVMesh vmesh)
		{
			bool flag = AddNormals != CGYesNoAuto.No;
			bool flag2 = AddTangents != CGYesNoAuto.No;
			CGMeshResource newMesh = getNewMesh();
			Mesh msh = newMesh.Prepare();
			newMesh.gameObject.layer = Layer;
			vmesh.ToMesh(ref msh);
			VertexCount += vmesh.Count;
			if (AddUV2 && !vmesh.HasUV2)
			{
				msh.uv2 = CGUtility.CalculateUV2(vmesh.UV);
			}
			if (flag && !vmesh.HasNormals)
			{
				msh.RecalculateNormals();
			}
			if (flag2 && !vmesh.HasTangents)
			{
				newMesh.Filter.CalculateTangents();
			}
			newMesh.Filter.transform.localPosition = Vector3.zero;
			newMesh.Filter.transform.localRotation = Quaternion.identity;
			newMesh.Filter.transform.localScale = Vector3.one;
			newMesh.Renderer.sharedMaterials = vmesh.GetMaterials();
			return newMesh;
		}

		private void sortByVertexCount(ref List<CGVMesh> vMeshes)
		{
			vMeshes.Sort((CGVMesh a, CGVMesh b) => -a.Count.CompareTo(b.Count));
		}

		private void removeUnusedResource()
		{
			for (int i = mCurrentMeshCount; i < Meshes.Count; i++)
			{
				DeleteManagedResource("Mesh", Meshes.Items[i], string.Empty);
			}
			Meshes.Items.RemoveRange(mCurrentMeshCount, Meshes.Count - mCurrentMeshCount);
		}

		private CGMeshResource getNewMesh()
		{
			CGMeshResource cGMeshResource;
			if (mCurrentMeshCount < Meshes.Count)
			{
				cGMeshResource = Meshes.Items[mCurrentMeshCount];
				if (cGMeshResource == null)
				{
					cGMeshResource = (CGMeshResource)AddManagedResource("Mesh", string.Empty, mCurrentMeshCount);
					Meshes.Items[mCurrentMeshCount] = cGMeshResource;
				}
			}
			else
			{
				cGMeshResource = (CGMeshResource)AddManagedResource("Mesh", string.Empty, mCurrentMeshCount);
				Meshes.Items.Add(cGMeshResource);
			}
			cGMeshResource.Renderer.shadowCastingMode = CastShadows;
			cGMeshResource.Renderer.receiveShadows = ReceiveShadows;
			cGMeshResource.Renderer.lightProbeUsage = LightProbeUsage;
			cGMeshResource.Renderer.reflectionProbeUsage = ReflectionProbes;
			cGMeshResource.Renderer.probeAnchor = AnchorOverride;
			if (!cGMeshResource.ColliderMatches(Collider))
			{
				cGMeshResource.RemoveCollider();
			}
			mCurrentMeshCount++;
			return cGMeshResource;
		}
	}
}
