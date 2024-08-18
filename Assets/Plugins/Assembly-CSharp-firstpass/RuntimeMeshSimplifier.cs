using System.Collections;
using System.Collections.Generic;
using UltimateGameTools.MeshSimplifier;
using UnityEngine;

[RequireComponent(typeof(MeshSimplify))]
public class RuntimeMeshSimplifier : MonoBehaviour
{
	private Dictionary<GameObject, Material[]> m_objectMaterials;

	private MeshSimplify m_selectedMeshSimplify;

	private bool m_bFinished;

	private Mesh m_newMesh;

	private int m_nLastProgress = -1;

	private string m_strLastTitle = string.Empty;

	private string m_strLastMessage = string.Empty;

	public string ProgressTitle
	{
		get
		{
			return m_strLastTitle;
		}
	}

	public string ProgressMessage
	{
		get
		{
			return m_strLastMessage;
		}
	}

	public int ProgressPercent
	{
		get
		{
			return m_nLastProgress;
		}
	}

	public bool Finished
	{
		get
		{
			return m_bFinished;
		}
	}

	public void Simplify(float percent)
	{
		if (!m_bFinished)
		{
			StartCoroutine(ComputeMeshWithVertices(Mathf.Clamp01(percent / 100f)));
		}
	}

	private void Awake()
	{
		m_selectedMeshSimplify = GetComponent<MeshSimplify>();
		m_objectMaterials = new Dictionary<GameObject, Material[]>();
		AddMaterials(m_selectedMeshSimplify.gameObject, m_objectMaterials);
		m_bFinished = false;
	}

	private void AddMaterials(GameObject theGameObject, Dictionary<GameObject, Material[]> dicMaterials)
	{
		Renderer component = theGameObject.GetComponent<Renderer>();
		if (component != null && component.sharedMaterials != null && (MeshSimplify.HasValidMeshData(theGameObject) || theGameObject.GetComponent<MeshSimplify>() != null))
		{
			dicMaterials.Add(theGameObject, component.sharedMaterials);
		}
		if (m_selectedMeshSimplify.RecurseIntoChildren)
		{
			for (int i = 0; i < theGameObject.transform.childCount; i++)
			{
				AddMaterials(theGameObject.transform.GetChild(i).gameObject, dicMaterials);
			}
		}
	}

	private void Progress(string strTitle, string strMessage, float fT)
	{
		int num = Mathf.RoundToInt(fT * 100f);
		if (num != m_nLastProgress || m_strLastTitle != strTitle || m_strLastMessage != strMessage)
		{
			m_strLastTitle = strTitle;
			m_strLastMessage = strMessage;
			m_nLastProgress = num;
		}
	}

	private IEnumerator ComputeMeshWithVertices(float fAmount)
	{
		Simplifier.CoroutineFrameMiliseconds = 20;
		foreach (KeyValuePair<GameObject, Material[]> pair in m_objectMaterials)
		{
			MeshSimplify meshSimplify = pair.Key.GetComponent<MeshSimplify>();
			MeshFilter meshFilter = pair.Key.GetComponent<MeshFilter>();
			SkinnedMeshRenderer skin = pair.Key.GetComponent<SkinnedMeshRenderer>();
			if (meshSimplify == null)
			{
				meshSimplify = pair.Key.AddComponent<MeshSimplify>();
				meshSimplify.m_meshSimplifyRoot = m_selectedMeshSimplify;
				m_selectedMeshSimplify.m_listDependentChildren.Add(meshSimplify);
			}
			if (meshSimplify.MeshSimplifier == null)
			{
				meshSimplify.MeshSimplifier = meshSimplify.gameObject.AddComponent<Simplifier>();
				meshSimplify.MeshSimplifier.hideFlags = HideFlags.HideInInspector;
				meshSimplify.ConfigureSimplifier();
			}
			if (!meshSimplify || !MeshSimplify.HasValidMeshData(pair.Key))
			{
				continue;
			}
			Mesh newMesh = null;
			if (meshFilter != null)
			{
				newMesh = Object.Instantiate(meshFilter.sharedMesh);
			}
			else if (skin != null)
			{
				newMesh = Object.Instantiate(skin.sharedMesh);
			}
			if (!meshSimplify.HasData())
			{
				meshSimplify.GetMeshSimplifier().CoroutineEnded = false;
				StartCoroutine(meshSimplify.GetMeshSimplifier().ProgressiveMesh(pair.Key, (!(meshFilter != null)) ? skin.sharedMesh : meshFilter.sharedMesh, null, meshSimplify.name, Progress));
				while (!meshSimplify.GetMeshSimplifier().CoroutineEnded)
				{
					yield return null;
				}
			}
			if (meshSimplify.GetMeshSimplifier() != null)
			{
				meshSimplify.GetMeshSimplifier().CoroutineEnded = false;
				StartCoroutine(meshSimplify.GetMeshSimplifier().ComputeMeshWithVertexCount(pair.Key, newMesh, Mathf.RoundToInt(fAmount * (float)meshSimplify.GetMeshSimplifier().GetOriginalMeshUniqueVertexCount()), meshSimplify.name, Progress));
				while (!meshSimplify.GetMeshSimplifier().CoroutineEnded)
				{
					yield return null;
				}
				if (meshFilter != null)
				{
					meshFilter.mesh = newMesh;
				}
				else if (skin != null)
				{
					skin.sharedMesh = newMesh;
				}
				meshSimplify.m_simplifiedMesh = newMesh;
			}
		}
		m_bFinished = true;
	}
}
