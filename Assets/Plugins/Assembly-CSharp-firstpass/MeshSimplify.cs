using System.Collections;
using System.Collections.Generic;
using UltimateGameTools.MeshSimplifier;
using UnityEngine;

public class MeshSimplify : MonoBehaviour
{
	[HideInInspector]
	public Mesh m_originalMesh;

	[HideInInspector]
	public Mesh m_simplifiedMesh;

	[HideInInspector]
	public bool m_bEnablePrefabUsage;

	[HideInInspector]
	public float m_fVertexAmount = 1f;

	[HideInInspector]
	public string m_strAssetPath;

	[HideInInspector]
	public MeshSimplify m_meshSimplifyRoot;

	[HideInInspector]
	public List<MeshSimplify> m_listDependentChildren = new List<MeshSimplify>();

	[HideInInspector]
	public bool m_bExpandRelevanceSpheres = true;

	public RelevanceSphere[] m_aRelevanceSpheres;

	[SerializeField]
	[HideInInspector]
	private Simplifier m_meshSimplifier;

	[SerializeField]
	[HideInInspector]
	private bool m_bGenerateIncludeChildren = true;

	[SerializeField]
	[HideInInspector]
	private bool m_bOverrideRootSettings;

	[SerializeField]
	[HideInInspector]
	private bool m_bUseEdgeLength = true;

	[SerializeField]
	[HideInInspector]
	private bool m_bUseCurvature = true;

	[SerializeField]
	[HideInInspector]
	private bool m_bProtectTexture = true;

	[SerializeField]
	[HideInInspector]
	private bool m_bLockBorder = true;

	[SerializeField]
	[HideInInspector]
	private bool m_bDataDirty = true;

	[SerializeField]
	[HideInInspector]
	private bool m_bExcludedFromTree;

	public bool RecurseIntoChildren
	{
		get
		{
			return m_bGenerateIncludeChildren;
		}
	}

	public Simplifier MeshSimplifier
	{
		get
		{
			return m_meshSimplifier;
		}
		set
		{
			m_meshSimplifier = value;
		}
	}

	public static bool HasValidMeshData(GameObject go)
	{
		MeshFilter component = go.GetComponent<MeshFilter>();
		if (component != null)
		{
			return true;
		}
		SkinnedMeshRenderer component2 = go.GetComponent<SkinnedMeshRenderer>();
		if (component2 != null)
		{
			return true;
		}
		return false;
	}

	public static bool IsRootOrBelongsToTree(MeshSimplify meshSimplify, MeshSimplify root)
	{
		if (meshSimplify == null)
		{
			return false;
		}
		return !meshSimplify.m_bExcludedFromTree && (meshSimplify.m_meshSimplifyRoot == null || meshSimplify.m_meshSimplifyRoot == root || meshSimplify == root || meshSimplify.m_meshSimplifyRoot == root.m_meshSimplifyRoot);
	}

	public bool IsGenerateIncludeChildrenActive()
	{
		return m_bGenerateIncludeChildren;
	}

	public bool HasDependentChildren()
	{
		return m_listDependentChildren != null && m_listDependentChildren.Count > 0;
	}

	public bool HasDataDirty()
	{
		return m_bDataDirty;
	}

	public bool SetDataDirty(bool bDirty)
	{
		return m_bDataDirty = bDirty;
	}

	public bool HasNonMeshSimplifyGameObjectsInTree()
	{
		return HasNonMeshSimplifyGameObjectsInTreeRecursive(this, base.gameObject);
	}

	private bool HasNonMeshSimplifyGameObjectsInTreeRecursive(MeshSimplify root, GameObject gameObject)
	{
		MeshSimplify component = gameObject.GetComponent<MeshSimplify>();
		if (component == null && HasValidMeshData(gameObject))
		{
			return true;
		}
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			if (HasNonMeshSimplifyGameObjectsInTreeRecursive(root, gameObject.transform.GetChild(i).gameObject))
			{
				return true;
			}
		}
		return false;
	}

	public void ConfigureSimplifier()
	{
		m_meshSimplifier.UseEdgeLength = ((!(m_meshSimplifyRoot != null) || m_bOverrideRootSettings) ? m_bUseEdgeLength : m_meshSimplifyRoot.m_bUseEdgeLength);
		m_meshSimplifier.UseCurvature = ((!(m_meshSimplifyRoot != null) || m_bOverrideRootSettings) ? m_bUseCurvature : m_meshSimplifyRoot.m_bUseCurvature);
		m_meshSimplifier.ProtectTexture = ((!(m_meshSimplifyRoot != null) || m_bOverrideRootSettings) ? m_bProtectTexture : m_meshSimplifyRoot.m_bProtectTexture);
		m_meshSimplifier.LockBorder = ((!(m_meshSimplifyRoot != null) || m_bOverrideRootSettings) ? m_bLockBorder : m_meshSimplifyRoot.m_bLockBorder);
	}

	public Simplifier GetMeshSimplifier()
	{
		return m_meshSimplifier;
	}

	public void ComputeData(bool bRecurseIntoChildren, Simplifier.ProgressDelegate progress = null)
	{
		ComputeDataRecursive(this, base.gameObject, bRecurseIntoChildren, progress);
	}

	private static void ComputeDataRecursive(MeshSimplify root, GameObject gameObject, bool bRecurseIntoChildren, Simplifier.ProgressDelegate progress = null)
	{
		MeshSimplify meshSimplify = gameObject.GetComponent<MeshSimplify>();
		if (meshSimplify == null && root.m_bGenerateIncludeChildren && HasValidMeshData(gameObject))
		{
			meshSimplify = gameObject.AddComponent<MeshSimplify>();
			meshSimplify.m_meshSimplifyRoot = root;
			root.m_listDependentChildren.Add(meshSimplify);
		}
		if (meshSimplify != null && IsRootOrBelongsToTree(meshSimplify, root))
		{
			meshSimplify.FreeData(false);
			MeshFilter component = meshSimplify.GetComponent<MeshFilter>();
			if (component != null && component.sharedMesh != null)
			{
				if (component.sharedMesh.vertexCount > 0)
				{
					if (meshSimplify.m_originalMesh == null)
					{
						meshSimplify.m_originalMesh = component.sharedMesh;
					}
					Simplifier[] components = meshSimplify.GetComponents<Simplifier>();
					for (int i = 0; i < components.Length; i++)
					{
						if (Application.isEditor && !Application.isPlaying)
						{
							Object.DestroyImmediate(components[i]);
						}
						else
						{
							Object.Destroy(components[i]);
						}
					}
					meshSimplify.m_meshSimplifier = meshSimplify.gameObject.AddComponent<Simplifier>();
					meshSimplify.m_meshSimplifier.hideFlags = HideFlags.HideInInspector;
					meshSimplify.ConfigureSimplifier();
					IEnumerator enumerator = meshSimplify.m_meshSimplifier.ProgressiveMesh(gameObject, meshSimplify.m_originalMesh, root.m_aRelevanceSpheres, meshSimplify.name, progress);
					while (enumerator.MoveNext())
					{
						if (Simplifier.Cancelled)
						{
							return;
						}
					}
					if (Simplifier.Cancelled)
					{
						return;
					}
				}
			}
			else
			{
				SkinnedMeshRenderer component2 = meshSimplify.GetComponent<SkinnedMeshRenderer>();
				if (component2 != null && component2.sharedMesh.vertexCount > 0)
				{
					if (meshSimplify.m_originalMesh == null)
					{
						meshSimplify.m_originalMesh = component2.sharedMesh;
					}
					Simplifier[] components2 = meshSimplify.GetComponents<Simplifier>();
					for (int j = 0; j < components2.Length; j++)
					{
						if (Application.isEditor && !Application.isPlaying)
						{
							Object.DestroyImmediate(components2[j]);
						}
						else
						{
							Object.Destroy(components2[j]);
						}
					}
					meshSimplify.m_meshSimplifier = meshSimplify.gameObject.AddComponent<Simplifier>();
					meshSimplify.m_meshSimplifier.hideFlags = HideFlags.HideInInspector;
					meshSimplify.ConfigureSimplifier();
					IEnumerator enumerator2 = meshSimplify.m_meshSimplifier.ProgressiveMesh(gameObject, meshSimplify.m_originalMesh, root.m_aRelevanceSpheres, meshSimplify.name, progress);
					while (enumerator2.MoveNext())
					{
						if (Simplifier.Cancelled)
						{
							return;
						}
					}
					if (Simplifier.Cancelled)
					{
						return;
					}
				}
			}
			meshSimplify.m_bDataDirty = false;
		}
		if (!bRecurseIntoChildren)
		{
			return;
		}
		for (int k = 0; k < gameObject.transform.childCount; k++)
		{
			ComputeDataRecursive(root, gameObject.transform.GetChild(k).gameObject, bRecurseIntoChildren, progress);
			if (Simplifier.Cancelled)
			{
				break;
			}
		}
	}

	public bool HasData()
	{
		return (m_meshSimplifier != null && m_simplifiedMesh != null) || (m_listDependentChildren != null && m_listDependentChildren.Count != 0);
	}

	public bool HasSimplifiedMesh()
	{
		return m_simplifiedMesh != null && m_simplifiedMesh.vertexCount > 0;
	}

	public void ComputeMesh(bool bRecurseIntoChildren, Simplifier.ProgressDelegate progress = null)
	{
		ComputeMeshRecursive(this, base.gameObject, bRecurseIntoChildren, progress);
	}

	private static void ComputeMeshRecursive(MeshSimplify root, GameObject gameObject, bool bRecurseIntoChildren, Simplifier.ProgressDelegate progress = null)
	{
		MeshSimplify component = gameObject.GetComponent<MeshSimplify>();
		if (component != null && IsRootOrBelongsToTree(component, root) && component.m_meshSimplifier != null)
		{
			if ((bool)component.m_simplifiedMesh)
			{
				component.m_simplifiedMesh.Clear();
			}
			float fVertexAmount = component.m_fVertexAmount;
			if (!component.m_bOverrideRootSettings && component.m_meshSimplifyRoot != null)
			{
				fVertexAmount = component.m_meshSimplifyRoot.m_fVertexAmount;
			}
			if (component.m_simplifiedMesh == null)
			{
				component.m_simplifiedMesh = CreateNewEmptyMesh(component);
			}
			component.ConfigureSimplifier();
			IEnumerator enumerator = component.m_meshSimplifier.ComputeMeshWithVertexCount(gameObject, component.m_simplifiedMesh, Mathf.RoundToInt(fVertexAmount * (float)component.m_meshSimplifier.GetOriginalMeshUniqueVertexCount()), component.name + " Simplified", progress);
			while (enumerator.MoveNext())
			{
				if (Simplifier.Cancelled)
				{
					return;
				}
			}
			if (Simplifier.Cancelled)
			{
				return;
			}
		}
		if (!bRecurseIntoChildren)
		{
			return;
		}
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			ComputeMeshRecursive(root, gameObject.transform.GetChild(i).gameObject, bRecurseIntoChildren, progress);
			if (Simplifier.Cancelled)
			{
				break;
			}
		}
	}

	public void AssignSimplifiedMesh(bool bRecurseIntoChildren)
	{
		AssignSimplifiedMeshRecursive(this, base.gameObject, bRecurseIntoChildren);
	}

	private static void AssignSimplifiedMeshRecursive(MeshSimplify root, GameObject gameObject, bool bRecurseIntoChildren)
	{
		MeshSimplify component = gameObject.GetComponent<MeshSimplify>();
		if (component != null && IsRootOrBelongsToTree(component, root) && component.m_simplifiedMesh != null)
		{
			MeshFilter component2 = component.GetComponent<MeshFilter>();
			if (component2 != null)
			{
				component2.sharedMesh = component.m_simplifiedMesh;
			}
			else
			{
				SkinnedMeshRenderer component3 = component.GetComponent<SkinnedMeshRenderer>();
				if (component3 != null)
				{
					component3.sharedMesh = component.m_simplifiedMesh;
				}
			}
		}
		if (bRecurseIntoChildren)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				AssignSimplifiedMeshRecursive(root, gameObject.transform.GetChild(i).gameObject, bRecurseIntoChildren);
			}
		}
	}

	public void RestoreOriginalMesh(bool bDeleteData, bool bRecurseIntoChildren)
	{
		RestoreOriginalMeshRecursive(this, base.gameObject, bDeleteData, bRecurseIntoChildren);
	}

	private static void RestoreOriginalMeshRecursive(MeshSimplify root, GameObject gameObject, bool bDeleteData, bool bRecurseIntoChildren)
	{
		MeshSimplify component = gameObject.GetComponent<MeshSimplify>();
		if (component != null && IsRootOrBelongsToTree(component, root))
		{
			if (component.m_originalMesh != null)
			{
				MeshFilter component2 = component.GetComponent<MeshFilter>();
				if (component2 != null)
				{
					component2.sharedMesh = component.m_originalMesh;
				}
				else
				{
					SkinnedMeshRenderer component3 = component.GetComponent<SkinnedMeshRenderer>();
					if (component3 != null)
					{
						component3.sharedMesh = component.m_originalMesh;
					}
				}
			}
			if (bDeleteData)
			{
				component.FreeData(false);
				component.m_listDependentChildren.Clear();
			}
		}
		if (bRecurseIntoChildren)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				RestoreOriginalMeshRecursive(root, gameObject.transform.GetChild(i).gameObject, bDeleteData, bRecurseIntoChildren);
			}
		}
	}

	public bool HasOriginalMeshActive(bool bRecurseIntoChildren)
	{
		return HasOriginalMeshActiveRecursive(this, base.gameObject, bRecurseIntoChildren);
	}

	private static bool HasOriginalMeshActiveRecursive(MeshSimplify root, GameObject gameObject, bool bRecurseIntoChildren)
	{
		MeshSimplify component = gameObject.GetComponent<MeshSimplify>();
		bool flag = false;
		if (component != null && IsRootOrBelongsToTree(component, root) && component.m_originalMesh != null)
		{
			MeshFilter component2 = component.GetComponent<MeshFilter>();
			if (component2 != null)
			{
				if (component2.sharedMesh == component.m_originalMesh)
				{
					flag = true;
				}
			}
			else
			{
				SkinnedMeshRenderer component3 = component.GetComponent<SkinnedMeshRenderer>();
				if (component3 != null && component3.sharedMesh == component.m_originalMesh)
				{
					flag = true;
				}
			}
		}
		if (bRecurseIntoChildren)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				flag = flag || HasOriginalMeshActiveRecursive(root, gameObject.transform.GetChild(i).gameObject, bRecurseIntoChildren);
			}
		}
		return flag;
	}

	public bool HasVertexData(bool bRecurseIntoChildren)
	{
		return HasVertexDataRecursive(this, base.gameObject, bRecurseIntoChildren);
	}

	private static bool HasVertexDataRecursive(MeshSimplify root, GameObject gameObject, bool bRecurseIntoChildren)
	{
		MeshSimplify component = gameObject.GetComponent<MeshSimplify>();
		if (component != null && IsRootOrBelongsToTree(component, root) && (bool)component.m_simplifiedMesh && component.m_simplifiedMesh.vertexCount > 0)
		{
			return true;
		}
		if (bRecurseIntoChildren)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				if (HasVertexDataRecursive(root, gameObject.transform.GetChild(i).gameObject, bRecurseIntoChildren))
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetOriginalVertexCount(bool bRecurseIntoChildren)
	{
		int nVertexCount = 0;
		GetOriginalVertexCountRecursive(this, base.gameObject, ref nVertexCount, bRecurseIntoChildren);
		return nVertexCount;
	}

	private static void GetOriginalVertexCountRecursive(MeshSimplify root, GameObject gameObject, ref int nVertexCount, bool bRecurseIntoChildren)
	{
		MeshSimplify component = gameObject.GetComponent<MeshSimplify>();
		if (component != null && IsRootOrBelongsToTree(component, root) && component.m_originalMesh != null)
		{
			nVertexCount += component.m_originalMesh.vertexCount;
		}
		if (bRecurseIntoChildren)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				GetOriginalVertexCountRecursive(root, gameObject.transform.GetChild(i).gameObject, ref nVertexCount, bRecurseIntoChildren);
			}
		}
	}

	public int GetOriginalTriangleCount(bool bRecurseIntoChildren)
	{
		int nTriangleCount = 0;
		GetOriginalTriangleCountRecursive(this, base.gameObject, ref nTriangleCount, bRecurseIntoChildren);
		return nTriangleCount;
	}

	private static void GetOriginalTriangleCountRecursive(MeshSimplify root, GameObject gameObject, ref int nTriangleCount, bool bRecurseIntoChildren)
	{
		MeshSimplify component = gameObject.GetComponent<MeshSimplify>();
		if (component != null && IsRootOrBelongsToTree(component, root) && component.m_originalMesh != null)
		{
			nTriangleCount += component.m_originalMesh.triangles.Length / 3;
		}
		if (bRecurseIntoChildren)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				GetOriginalTriangleCountRecursive(root, gameObject.transform.GetChild(i).gameObject, ref nTriangleCount, bRecurseIntoChildren);
			}
		}
	}

	public int GetSimplifiedVertexCount(bool bRecurseIntoChildren)
	{
		int nVertexCount = 0;
		GetSimplifiedVertexCountRecursive(this, base.gameObject, ref nVertexCount, bRecurseIntoChildren);
		return nVertexCount;
	}

	private static void GetSimplifiedVertexCountRecursive(MeshSimplify root, GameObject gameObject, ref int nVertexCount, bool bRecurseIntoChildren)
	{
		MeshSimplify component = gameObject.GetComponent<MeshSimplify>();
		if (component != null && IsRootOrBelongsToTree(component, root) && component.m_simplifiedMesh != null)
		{
			nVertexCount += component.m_simplifiedMesh.vertexCount;
		}
		if (bRecurseIntoChildren)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				GetSimplifiedVertexCountRecursive(root, gameObject.transform.GetChild(i).gameObject, ref nVertexCount, bRecurseIntoChildren);
			}
		}
	}

	public int GetSimplifiedTriangleCount(bool bRecurseIntoChildren)
	{
		int nTriangleCount = 0;
		GetSimplifiedTriangleCountRecursive(this, base.gameObject, ref nTriangleCount, bRecurseIntoChildren);
		return nTriangleCount;
	}

	private static void GetSimplifiedTriangleCountRecursive(MeshSimplify root, GameObject gameObject, ref int nTriangleCount, bool bRecurseIntoChildren)
	{
		MeshSimplify component = gameObject.GetComponent<MeshSimplify>();
		if (component != null && IsRootOrBelongsToTree(component, root) && component.m_simplifiedMesh != null)
		{
			nTriangleCount += component.m_simplifiedMesh.triangles.Length / 3;
		}
		if (bRecurseIntoChildren)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				GetSimplifiedTriangleCountRecursive(root, gameObject.transform.GetChild(i).gameObject, ref nTriangleCount, bRecurseIntoChildren);
			}
		}
	}

	public void RemoveFromTree()
	{
		if (m_meshSimplifyRoot != null)
		{
			m_meshSimplifyRoot.m_listDependentChildren.Remove(this);
		}
		RestoreOriginalMesh(true, false);
		m_bExcludedFromTree = true;
	}

	public void FreeData(bool bRecurseIntoChildren)
	{
		FreeDataRecursive(this, base.gameObject, bRecurseIntoChildren);
	}

	private static void FreeDataRecursive(MeshSimplify root, GameObject gameObject, bool bRecurseIntoChildren)
	{
		MeshSimplify component = gameObject.GetComponent<MeshSimplify>();
		if (component != null && IsRootOrBelongsToTree(component, root))
		{
			if ((bool)component.m_simplifiedMesh)
			{
				component.m_simplifiedMesh.Clear();
			}
			Simplifier[] components = gameObject.GetComponents<Simplifier>();
			for (int i = 0; i < components.Length; i++)
			{
				if (Application.isEditor && !Application.isPlaying)
				{
					Object.DestroyImmediate(components[i]);
				}
				else
				{
					Object.Destroy(components[i]);
				}
			}
			component.m_bDataDirty = true;
		}
		if (bRecurseIntoChildren)
		{
			for (int j = 0; j < gameObject.transform.childCount; j++)
			{
				FreeDataRecursive(root, gameObject.transform.GetChild(j).gameObject, bRecurseIntoChildren);
			}
		}
	}

	private static Mesh CreateNewEmptyMesh(MeshSimplify meshSimplify)
	{
		if (meshSimplify.m_originalMesh == null)
		{
			return new Mesh();
		}
		Mesh mesh = Object.Instantiate(meshSimplify.m_originalMesh);
		mesh.Clear();
		return mesh;
	}
}
