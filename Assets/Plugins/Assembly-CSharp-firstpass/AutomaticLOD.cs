using System;
using System.Collections;
using System.Collections.Generic;
using UltimateGameTools.MeshSimplifier;
using UnityEngine;

public class AutomaticLOD : MonoBehaviour
{
	[Serializable]
	public enum EvalMode
	{
		CameraDistance = 0,
		ScreenCoverage = 1
	}

	[Serializable]
	public enum LevelsToGenerate
	{
		_1 = 1,
		_2 = 2,
		_3 = 3,
		_4 = 4,
		_5 = 5,
		_6 = 6
	}

	[Serializable]
	public enum SwitchMode
	{
		SwitchMesh = 0,
		SwitchGameObject = 1,
		UnityLODGroup = 2
	}

	[Serializable]
	public class LODLevelData
	{
		public float m_fScreenCoverage;

		public float m_fMaxCameraDistance;

		public float m_fMeshVerticesAmount;

		public int m_nColorEditorBarIndex;

		public Mesh m_mesh;

		public bool m_bUsesOriginalMesh;

		public GameObject m_gameObject;

		public LODLevelData GetCopy()
		{
			LODLevelData lODLevelData = new LODLevelData();
			lODLevelData.m_fScreenCoverage = m_fScreenCoverage;
			lODLevelData.m_fMaxCameraDistance = m_fMaxCameraDistance;
			lODLevelData.m_fMeshVerticesAmount = m_fMeshVerticesAmount;
			lODLevelData.m_nColorEditorBarIndex = m_nColorEditorBarIndex;
			lODLevelData.m_mesh = m_mesh;
			lODLevelData.m_bUsesOriginalMesh = m_bUsesOriginalMesh;
			return lODLevelData;
		}
	}

	[HideInInspector]
	public Mesh m_originalMesh;

	[HideInInspector]
	public EvalMode m_evalMode = EvalMode.ScreenCoverage;

	[HideInInspector]
	public bool m_bEnablePrefabUsage;

	[HideInInspector]
	public string m_strAssetPath;

	[HideInInspector]
	public float m_fMaxCameraDistance = 1000f;

	[HideInInspector]
	public int m_nColorEditorBarNewIndex;

	[HideInInspector]
	public List<LODLevelData> m_listLODLevels;

	[HideInInspector]
	public AutomaticLOD m_LODObjectRoot;

	[HideInInspector]
	public List<AutomaticLOD> m_listDependentChildren = new List<AutomaticLOD>();

	public bool m_bExpandRelevanceSpheres = true;

	public RelevanceSphere[] m_aRelevanceSpheres;

	[SerializeField]
	private Simplifier m_meshSimplifier;

	[SerializeField]
	private bool m_bGenerateIncludeChildren = true;

	[SerializeField]
	private LevelsToGenerate m_levelsToGenerate = LevelsToGenerate._3;

	[SerializeField]
	private SwitchMode m_switchMode = SwitchMode.UnityLODGroup;

	[SerializeField]
	private bool m_bOverrideRootSettings;

	[SerializeField]
	[HideInInspector]
	private bool m_bLODDataDirty = true;

	[SerializeField]
	[HideInInspector]
	private AutomaticLOD m_LODObjectRootPersist;

	[SerializeField]
	[HideInInspector]
	private LODGroup m_LODGroup;

	private bool m_bUseAutomaticCameraLODSwitch = true;

	private const int k_MaxLODChecksPerFrame = 4;

	private const int k_MaxFrameCheckLoop = 100;

	private static int s_currentCheckIndex;

	private static int s_currentFrameCheckIndex;

	private static int s_checkLoopLength;

	private static int s_lastFrameComputedModulus = -1;

	private static int s_currentFrameInLoop = -1;

	private static Camera s_userDefinedCamera;

	private Camera m_renderCamera;

	private int m_nCurrentLOD = -1;

	private Dictionary<Camera, int> m_cachedFrameLODLevel;

	private Vector3 m_localCenter;

	private Vector3[] _corners;

	private int m_frameToCheck;

	private bool b_performCheck;

	public static Camera UserDefinedLODCamera
	{
		get
		{
			return s_userDefinedCamera;
		}
		set
		{
			s_userDefinedCamera = value;
		}
	}

	public SwitchMode LODSwitchMode
	{
		get
		{
			if (m_LODObjectRootPersist != null)
			{
				return m_LODObjectRootPersist.m_switchMode;
			}
			if (m_LODObjectRoot != null)
			{
				return m_LODObjectRoot.m_switchMode;
			}
			return m_switchMode;
		}
	}

	private void Awake()
	{
		if ((bool)m_originalMesh)
		{
			MeshFilter component = GetComponent<MeshFilter>();
			if (component != null)
			{
				component.sharedMesh = m_originalMesh;
			}
			else
			{
				SkinnedMeshRenderer component2 = GetComponent<SkinnedMeshRenderer>();
				if (component2 != null)
				{
					component2.sharedMesh = m_originalMesh;
				}
			}
		}
		m_localCenter = base.transform.InverseTransformPoint(ComputeWorldCenter());
		m_cachedFrameLODLevel = new Dictionary<Camera, int>();
		b_performCheck = false;
		m_frameToCheck = -1;
		if (!IsDependent())
		{
			m_frameToCheck = s_currentFrameCheckIndex;
			s_currentCheckIndex++;
			if (s_currentCheckIndex >= 4)
			{
				s_currentCheckIndex = 0;
				s_currentFrameCheckIndex++;
				if (s_currentFrameCheckIndex >= 100)
				{
					s_currentFrameCheckIndex = 0;
				}
				s_checkLoopLength = Mathf.Min(s_checkLoopLength + 1, 100);
			}
		}
		s_lastFrameComputedModulus = 0;
		s_currentFrameInLoop = 0;
	}

	private void Update()
	{
		if (!m_bUseAutomaticCameraLODSwitch || LODSwitchMode == SwitchMode.UnityLODGroup)
		{
			return;
		}
		if (Time.frameCount != s_lastFrameComputedModulus && s_checkLoopLength > 0)
		{
			s_currentFrameInLoop = Time.frameCount % s_checkLoopLength;
			s_lastFrameComputedModulus = Time.frameCount;
		}
		if (!IsDependent())
		{
			if (s_currentFrameInLoop == m_frameToCheck)
			{
				b_performCheck = true;
				m_cachedFrameLODLevel.Clear();
			}
			else
			{
				b_performCheck = false;
			}
		}
		Camera camera = ((s_userDefinedCamera != null) ? s_userDefinedCamera : ((!(m_renderCamera != null)) ? Camera.main : m_renderCamera));
		if (IsDependent())
		{
			bool flag = ((!(m_LODObjectRootPersist != null)) ? m_LODObjectRoot.b_performCheck : m_LODObjectRootPersist.b_performCheck);
			if (!flag)
			{
				return;
			}
			camera = ((s_userDefinedCamera != null) ? s_userDefinedCamera : ((m_LODObjectRootPersist != null && m_LODObjectRootPersist.m_renderCamera != null) ? m_LODObjectRootPersist.m_renderCamera : ((m_LODObjectRoot != null && m_LODObjectRoot.m_renderCamera != null) ? m_LODObjectRoot.m_renderCamera : ((!(m_renderCamera != null)) ? Camera.main : m_renderCamera))));
			if (flag && camera != null)
			{
				int num = ((!(m_LODObjectRootPersist != null)) ? m_LODObjectRoot.GetLODLevelUsingCamera(camera) : m_LODObjectRootPersist.GetLODLevelUsingCamera(camera));
				if (num != -1)
				{
					SwitchToLOD(num, false);
				}
			}
		}
		else if ((bool)m_originalMesh && b_performCheck && (bool)camera)
		{
			int lODLevelUsingCamera = GetLODLevelUsingCamera(camera);
			if (lODLevelUsingCamera != -1)
			{
				SwitchToLOD(lODLevelUsingCamera, false);
			}
		}
	}

	private void OnWillRenderObject()
	{
		m_renderCamera = Camera.current;
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

	public static bool IsRootOrBelongsToLODTree(AutomaticLOD automaticLOD, AutomaticLOD root)
	{
		if (automaticLOD == null)
		{
			return false;
		}
		return automaticLOD.m_LODObjectRoot == null || automaticLOD.m_LODObjectRoot == root || automaticLOD == root || automaticLOD.m_LODObjectRoot == root.m_LODObjectRoot;
	}

	public int GetNumberOfLevelsToGenerate()
	{
		return (int)m_levelsToGenerate;
	}

	public bool IsGenerateIncludeChildrenActive()
	{
		return m_bGenerateIncludeChildren;
	}

	public bool IsRootAutomaticLOD()
	{
		return m_LODObjectRoot == null;
	}

	public bool HasDependentChildren()
	{
		return m_listDependentChildren != null && m_listDependentChildren.Count > 0;
	}

	public bool HasLODDataDirty()
	{
		return m_bLODDataDirty;
	}

	public bool SetLODDataDirty(bool bDirty)
	{
		return m_bLODDataDirty = bDirty;
	}

	public int GetLODLevelCount()
	{
		return (m_listLODLevels != null) ? m_listLODLevels.Count : 0;
	}

	public float ComputeScreenCoverage(Camera camera)
	{
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		float num3 = float.MinValue;
		float num4 = float.MinValue;
		if ((bool)m_originalMesh)
		{
			if (_corners == null)
			{
				_corners = new Vector3[8];
			}
			if (_corners.Length != 8)
			{
				_corners = new Vector3[8];
			}
			BuildCornerData(ref _corners, m_originalMesh.bounds);
			for (int i = 0; i < _corners.Length; i++)
			{
				Vector3 vector = camera.WorldToViewportPoint(base.transform.TransformPoint(_corners[i]));
				if (vector.x < num)
				{
					num = vector.x;
				}
				if (vector.y < num2)
				{
					num2 = vector.y;
				}
				if (vector.x > num3)
				{
					num3 = vector.x;
				}
				if (vector.y > num4)
				{
					num4 = vector.y;
				}
			}
		}
		for (int j = 0; j < m_listDependentChildren.Count; j++)
		{
			if (!(m_listDependentChildren[j] != null) || !m_listDependentChildren[j].m_originalMesh)
			{
				continue;
			}
			if (m_listDependentChildren[j]._corners == null)
			{
				m_listDependentChildren[j]._corners = new Vector3[8];
			}
			if (m_listDependentChildren[j]._corners.Length != 8)
			{
				m_listDependentChildren[j]._corners = new Vector3[8];
			}
			BuildCornerData(ref m_listDependentChildren[j]._corners, m_listDependentChildren[j].m_originalMesh.bounds);
			for (int k = 0; k < m_listDependentChildren[j]._corners.Length; k++)
			{
				Vector3 vector2 = camera.WorldToViewportPoint(m_listDependentChildren[j].transform.TransformPoint(m_listDependentChildren[j]._corners[k]));
				if (vector2.x < num)
				{
					num = vector2.x;
				}
				if (vector2.y < num2)
				{
					num2 = vector2.y;
				}
				if (vector2.x > num3)
				{
					num3 = vector2.x;
				}
				if (vector2.y > num4)
				{
					num4 = vector2.y;
				}
			}
		}
		return (num3 - num) * (num4 - num2);
	}

	public Vector3 ComputeWorldCenter()
	{
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		float num3 = float.MaxValue;
		float num4 = float.MinValue;
		float num5 = float.MinValue;
		float num6 = float.MinValue;
		if ((bool)m_originalMesh)
		{
			for (int i = 0; i < 2; i++)
			{
				Vector3 vector = ((i != 0) ? GetComponent<Renderer>().bounds.max : GetComponent<Renderer>().bounds.min);
				if (vector.x < num)
				{
					num = vector.x;
				}
				if (vector.y < num2)
				{
					num2 = vector.y;
				}
				if (vector.z < num3)
				{
					num3 = vector.z;
				}
				if (vector.x > num4)
				{
					num4 = vector.x;
				}
				if (vector.y > num5)
				{
					num5 = vector.y;
				}
				if (vector.z > num6)
				{
					num6 = vector.z;
				}
			}
		}
		for (int j = 0; j < m_listDependentChildren.Count; j++)
		{
			if (!(m_listDependentChildren[j] != null) || !m_listDependentChildren[j].m_originalMesh)
			{
				continue;
			}
			for (int k = 0; k < 2; k++)
			{
				Vector3 vector2 = ((k != 0) ? m_listDependentChildren[j].GetComponent<Renderer>().bounds.max : m_listDependentChildren[j].GetComponent<Renderer>().bounds.min);
				if (vector2.x < num)
				{
					num = vector2.x;
				}
				if (vector2.y < num2)
				{
					num2 = vector2.y;
				}
				if (vector2.z < num3)
				{
					num3 = vector2.z;
				}
				if (vector2.x > num4)
				{
					num4 = vector2.x;
				}
				if (vector2.y > num5)
				{
					num5 = vector2.y;
				}
				if (vector2.z > num6)
				{
					num6 = vector2.z;
				}
			}
		}
		Vector3 vector3 = new Vector3(num, num2, num3);
		Vector3 vector4 = new Vector3(num4, num5, num6);
		return (vector4 + vector3) * 0.5f;
	}

	public float ComputeViewSpaceBounds(Vector3 v3CameraPos, Vector3 v3CameraDir, Vector3 v3CameraUp, out Vector3 v3Min, out Vector3 v3Max, out Vector3 v3Center)
	{
		Matrix4x4 matrix4x = Matrix4x4.TRS(v3CameraPos, Quaternion.LookRotation(v3CameraDir, v3CameraUp), Vector3.one);
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		float num3 = float.MaxValue;
		float num4 = float.MinValue;
		float num5 = float.MinValue;
		float num6 = float.MinValue;
		v3Center = matrix4x.inverse.MultiplyPoint(base.transform.TransformPoint(Vector3.zero));
		if ((bool)m_originalMesh)
		{
			for (int i = 0; i < 2; i++)
			{
				Vector3 point = ((i != 0) ? GetComponent<Renderer>().bounds.max : GetComponent<Renderer>().bounds.min);
				Vector3 vector = matrix4x.inverse.MultiplyPoint(point);
				if (vector.x < num)
				{
					num = vector.x;
				}
				if (vector.y < num2)
				{
					num2 = vector.y;
				}
				if (vector.z < num3)
				{
					num3 = vector.z;
				}
				if (vector.x > num4)
				{
					num4 = vector.x;
				}
				if (vector.y > num5)
				{
					num5 = vector.y;
				}
				if (vector.z > num6)
				{
					num6 = vector.z;
				}
			}
		}
		for (int j = 0; j < m_listDependentChildren.Count; j++)
		{
			if (!(m_listDependentChildren[j] != null) || !m_listDependentChildren[j].m_originalMesh)
			{
				continue;
			}
			for (int k = 0; k < 2; k++)
			{
				Vector3 point2 = ((k != 0) ? m_listDependentChildren[j].GetComponent<Renderer>().bounds.max : m_listDependentChildren[j].GetComponent<Renderer>().bounds.min);
				Vector3 vector2 = matrix4x.inverse.MultiplyPoint(point2);
				if (vector2.x < num)
				{
					num = vector2.x;
				}
				if (vector2.y < num2)
				{
					num2 = vector2.y;
				}
				if (vector2.z < num3)
				{
					num3 = vector2.z;
				}
				if (vector2.x > num4)
				{
					num4 = vector2.x;
				}
				if (vector2.y > num5)
				{
					num5 = vector2.y;
				}
				if (vector2.z > num6)
				{
					num6 = vector2.z;
				}
			}
		}
		v3Min = new Vector3(num, num2, num3);
		v3Max = new Vector3(num4, num5, num6);
		return (num4 - num) * (num5 - num2);
	}

	public void SetAutomaticCameraLODSwitch(bool bEnabled)
	{
		SetAutomaticCameraLODSwitchRecursive(this, base.gameObject, bEnabled);
	}

	private static void SetAutomaticCameraLODSwitchRecursive(AutomaticLOD root, GameObject gameObject, bool bEnabled)
	{
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		if (component != null && IsRootOrBelongsToLODTree(component, root))
		{
			component.m_bUseAutomaticCameraLODSwitch = bEnabled;
		}
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			SetAutomaticCameraLODSwitchRecursive(root, gameObject.transform.GetChild(i).gameObject, bEnabled);
		}
	}

	public void SetLODLevels(List<LODLevelData> listLODLevelData, EvalMode evalMode, float fMaxCameraDistance, bool bRecurseIntoChildren)
	{
		m_listLODLevels = listLODLevelData;
		m_fMaxCameraDistance = fMaxCameraDistance;
		m_nColorEditorBarNewIndex = listLODLevelData.Count;
		m_evalMode = evalMode;
		m_LODObjectRoot = null;
		m_LODObjectRootPersist = null;
		m_listDependentChildren = new List<AutomaticLOD>();
		if (bRecurseIntoChildren)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				SetLODLevelsRecursive(this, base.transform.GetChild(i).gameObject);
			}
		}
	}

	private static void SetLODLevelsRecursive(AutomaticLOD root, GameObject gameObject)
	{
		AutomaticLOD automaticLOD = gameObject.GetComponent<AutomaticLOD>();
		bool flag = false;
		if (automaticLOD != null)
		{
			if (IsRootOrBelongsToLODTree(automaticLOD, root))
			{
				flag = true;
			}
		}
		else if (HasValidMeshData(gameObject))
		{
			automaticLOD = gameObject.AddComponent<AutomaticLOD>();
			flag = true;
		}
		if (flag && (bool)automaticLOD)
		{
			automaticLOD.m_fMaxCameraDistance = root.m_fMaxCameraDistance;
			automaticLOD.m_nColorEditorBarNewIndex = root.m_nColorEditorBarNewIndex;
			automaticLOD.m_evalMode = root.m_evalMode;
			automaticLOD.m_listLODLevels = new List<LODLevelData>();
			automaticLOD.m_LODObjectRoot = root;
			automaticLOD.m_LODObjectRootPersist = root;
			root.m_listDependentChildren.Add(automaticLOD);
			for (int i = 0; i < root.m_listLODLevels.Count; i++)
			{
				automaticLOD.m_listLODLevels.Add(root.m_listLODLevels[i].GetCopy());
				automaticLOD.m_listLODLevels[i].m_mesh = CreateNewEmptyMesh(automaticLOD);
			}
		}
		for (int j = 0; j < gameObject.transform.childCount; j++)
		{
			SetLODLevelsRecursive(root, gameObject.transform.GetChild(j).gameObject);
		}
		if (flag && (bool)automaticLOD)
		{
			for (int k = 0; k < root.m_listLODLevels.Count; k++)
			{
				CheckForAdditionalLODSetup(root, automaticLOD, automaticLOD.m_listLODLevels[k], k);
			}
		}
	}

	public void AddLODLevel(int nLevel, bool bBefore, bool bCreateMesh, bool bRecurseIntoChildren)
	{
		AddLODLevelRecursive(this, base.gameObject, nLevel, bBefore, bCreateMesh, bRecurseIntoChildren);
	}

	public static void AddLODLevelRecursive(AutomaticLOD root, GameObject gameObject, int nLevel, bool bBefore, bool bCreateMesh, bool bRecurseIntoChildren)
	{
		if (Simplifier.Cancelled)
		{
			return;
		}
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		if (component != null && IsRootOrBelongsToLODTree(component, root))
		{
			bool flag = true;
			if (component.m_listLODLevels == null)
			{
				flag = false;
			}
			else if (nLevel < 0 || nLevel >= component.m_listLODLevels.Count)
			{
				flag = false;
			}
			if (flag)
			{
				LODLevelData lODLevelData = new LODLevelData();
				lODLevelData.m_bUsesOriginalMesh = false;
				lODLevelData.m_gameObject = null;
				if (bBefore)
				{
					if (nLevel == 0)
					{
						lODLevelData.m_fScreenCoverage = component.m_listLODLevels[0].m_fScreenCoverage;
						lODLevelData.m_fMaxCameraDistance = component.m_listLODLevels[0].m_fMaxCameraDistance;
						lODLevelData.m_fMeshVerticesAmount = 1f;
						lODLevelData.m_nColorEditorBarIndex = component.m_nColorEditorBarNewIndex++;
						if (component.m_listLODLevels.Count > 1)
						{
							component.m_listLODLevels[0].m_fScreenCoverage = (component.m_listLODLevels[0].m_fScreenCoverage + component.m_listLODLevels[1].m_fScreenCoverage) / 2f;
							component.m_listLODLevels[0].m_fMaxCameraDistance = (component.m_listLODLevels[0].m_fMaxCameraDistance + component.m_listLODLevels[1].m_fMaxCameraDistance) / 2f;
						}
						else
						{
							component.m_listLODLevels[0].m_fScreenCoverage *= 0.5f;
							component.m_listLODLevels[0].m_fMaxCameraDistance *= 2f;
							if (Mathf.Approximately(component.m_listLODLevels[0].m_fMaxCameraDistance, 0f))
							{
								component.m_listLODLevels[0].m_fMaxCameraDistance = component.m_fMaxCameraDistance * 0.5f;
							}
						}
					}
					else
					{
						lODLevelData.m_fScreenCoverage = (component.m_listLODLevels[nLevel - 1].m_fScreenCoverage + component.m_listLODLevels[nLevel].m_fScreenCoverage) / 2f;
						lODLevelData.m_fMaxCameraDistance = (component.m_listLODLevels[nLevel - 1].m_fMaxCameraDistance + component.m_listLODLevels[nLevel].m_fMaxCameraDistance) / 2f;
						lODLevelData.m_fMeshVerticesAmount = (component.m_listLODLevels[nLevel - 1].m_fMeshVerticesAmount + component.m_listLODLevels[nLevel].m_fMeshVerticesAmount) / 2f;
						lODLevelData.m_nColorEditorBarIndex = component.m_nColorEditorBarNewIndex++;
					}
					if (bCreateMesh && lODLevelData.m_mesh == null)
					{
						lODLevelData.m_mesh = CreateNewEmptyMesh(component);
					}
					component.m_listLODLevels.Insert(nLevel, lODLevelData);
					if (bCreateMesh)
					{
						CheckForAdditionalLODSetup(root, component, lODLevelData, (nLevel != 0) ? (nLevel - 1) : 0);
					}
				}
				else
				{
					int num = component.m_listLODLevels.Count - 1;
					if (nLevel == num)
					{
						lODLevelData.m_fScreenCoverage = component.m_listLODLevels[num].m_fScreenCoverage * 0.5f;
						lODLevelData.m_fMaxCameraDistance = (component.m_listLODLevels[num].m_fMaxCameraDistance + component.m_fMaxCameraDistance) * 0.5f;
						lODLevelData.m_fMeshVerticesAmount = component.m_listLODLevels[num].m_fMeshVerticesAmount * 0.5f;
						lODLevelData.m_nColorEditorBarIndex = component.m_nColorEditorBarNewIndex++;
					}
					else
					{
						lODLevelData.m_fScreenCoverage = (component.m_listLODLevels[nLevel + 1].m_fScreenCoverage + component.m_listLODLevels[nLevel].m_fScreenCoverage) / 2f;
						lODLevelData.m_fMaxCameraDistance = (component.m_listLODLevels[nLevel + 1].m_fMaxCameraDistance + component.m_listLODLevels[nLevel].m_fMaxCameraDistance) / 2f;
						lODLevelData.m_fMeshVerticesAmount = (component.m_listLODLevels[nLevel + 1].m_fMeshVerticesAmount + component.m_listLODLevels[nLevel].m_fMeshVerticesAmount) / 2f;
						lODLevelData.m_nColorEditorBarIndex = component.m_nColorEditorBarNewIndex++;
					}
					if (bCreateMesh && lODLevelData.m_mesh == null)
					{
						lODLevelData.m_mesh = CreateNewEmptyMesh(component);
					}
					if (nLevel == num)
					{
						component.m_listLODLevels.Add(lODLevelData);
					}
					else
					{
						component.m_listLODLevels.Insert(nLevel + 1, lODLevelData);
					}
					if (bCreateMesh)
					{
						CheckForAdditionalLODSetup(root, component, lODLevelData, (nLevel != num) ? (nLevel + 1) : num);
					}
				}
			}
		}
		if (!bRecurseIntoChildren)
		{
			return;
		}
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			AddLODLevelRecursive(root, gameObject.transform.GetChild(i).gameObject, nLevel, bBefore, bCreateMesh, bRecurseIntoChildren);
			if (Simplifier.Cancelled)
			{
				break;
			}
		}
	}

	public void RemoveLODLevel(int nLevel, bool bDeleteMesh, bool bRecurseIntoChildren)
	{
		RemoveLODLevelRecursive(this, base.gameObject, nLevel, bDeleteMesh, bRecurseIntoChildren);
	}

	public static void RemoveLODLevelRecursive(AutomaticLOD root, GameObject gameObject, int nLevel, bool bDeleteMesh, bool bRecurseIntoChildren)
	{
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		if (component != null && IsRootOrBelongsToLODTree(component, root))
		{
			bool flag = true;
			if (component.m_listLODLevels == null)
			{
				flag = false;
			}
			else if (nLevel < 0 || nLevel >= component.m_listLODLevels.Count || component.m_listLODLevels.Count == 1)
			{
				flag = false;
			}
			if (flag)
			{
				if (bDeleteMesh)
				{
					if (component.m_listLODLevels[nLevel].m_mesh != null)
					{
						component.m_listLODLevels[nLevel].m_mesh.Clear();
					}
					if (component.m_listLODLevels[nLevel].m_gameObject != null)
					{
						if (Application.isEditor && !Application.isPlaying)
						{
							UnityEngine.Object.DestroyImmediate(component.m_listLODLevels[nLevel].m_gameObject);
						}
						else
						{
							UnityEngine.Object.Destroy(component.m_listLODLevels[nLevel].m_gameObject);
						}
					}
				}
				if (nLevel == 0 && component.m_listLODLevels.Count > 1)
				{
					component.m_listLODLevels[1].m_fMaxCameraDistance = 0f;
					component.m_listLODLevels[1].m_fScreenCoverage = 1f;
				}
				component.m_listLODLevels.RemoveAt(nLevel);
			}
			for (int i = 0; i < component.m_listLODLevels.Count; i++)
			{
				if (component.m_listLODLevels[i].m_gameObject != null)
				{
					component.m_listLODLevels[i].m_gameObject.name = "LOD" + i;
				}
			}
		}
		if (bRecurseIntoChildren)
		{
			for (int j = 0; j < gameObject.transform.childCount; j++)
			{
				RemoveLODLevelRecursive(root, gameObject.transform.GetChild(j).gameObject, nLevel, bDeleteMesh, bRecurseIntoChildren);
			}
		}
	}

	public Simplifier GetMeshSimplifier()
	{
		return m_meshSimplifier;
	}

	public void ComputeLODData(bool bRecurseIntoChildren, Simplifier.ProgressDelegate progress = null)
	{
		ComputeLODDataRecursive(this, base.gameObject, bRecurseIntoChildren, progress);
	}

	private void ComputeLODDataRecursive(AutomaticLOD root, GameObject gameObject, bool bRecurseIntoChildren, Simplifier.ProgressDelegate progress = null)
	{
		if (Simplifier.Cancelled)
		{
			return;
		}
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		if (component != null)
		{
			if (IsRootOrBelongsToLODTree(component, root))
			{
				component.FreeLODData(false);
				MeshFilter component2 = component.GetComponent<MeshFilter>();
				if (component2 != null)
				{
					if (component.m_originalMesh == null)
					{
						component.m_originalMesh = component2.sharedMesh;
					}
					Simplifier[] components = component.GetComponents<Simplifier>();
					for (int i = 0; i < components.Length; i++)
					{
						if (Application.isEditor && !Application.isPlaying)
						{
							UnityEngine.Object.DestroyImmediate(components[i]);
						}
						else
						{
							UnityEngine.Object.Destroy(components[i]);
						}
					}
					component.m_meshSimplifier = component.gameObject.AddComponent<Simplifier>();
					component.m_meshSimplifier.hideFlags = HideFlags.HideInInspector;
					IEnumerator enumerator = component.m_meshSimplifier.ProgressiveMesh(gameObject, component.m_originalMesh, root.m_aRelevanceSpheres, component.name, progress);
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
				else
				{
					SkinnedMeshRenderer component3 = component.GetComponent<SkinnedMeshRenderer>();
					if (component3 != null)
					{
						if (component.m_originalMesh == null)
						{
							component.m_originalMesh = component3.sharedMesh;
						}
						Simplifier[] components2 = component.GetComponents<Simplifier>();
						for (int j = 0; j < components2.Length; j++)
						{
							if (Application.isEditor && !Application.isPlaying)
							{
								UnityEngine.Object.DestroyImmediate(components2[j]);
							}
							else
							{
								UnityEngine.Object.Destroy(components2[j]);
							}
						}
						component.m_meshSimplifier = component.gameObject.AddComponent<Simplifier>();
						component.m_meshSimplifier.hideFlags = HideFlags.HideInInspector;
						IEnumerator enumerator2 = component.m_meshSimplifier.ProgressiveMesh(gameObject, component.m_originalMesh, root.m_aRelevanceSpheres, component.name, progress);
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
			}
			for (int k = 0; k < component.m_listLODLevels.Count; k++)
			{
				component.m_listLODLevels[k].m_mesh = null;
			}
			component.m_bLODDataDirty = false;
		}
		if (!bRecurseIntoChildren)
		{
			return;
		}
		for (int l = 0; l < gameObject.transform.childCount; l++)
		{
			if (Simplifier.Cancelled)
			{
				break;
			}
			ComputeLODDataRecursive(root, gameObject.transform.GetChild(l).gameObject, bRecurseIntoChildren, progress);
			if (Simplifier.Cancelled)
			{
				break;
			}
		}
	}

	public bool HasLODData()
	{
		return m_meshSimplifier != null && m_listLODLevels != null && m_listLODLevels.Count > 0;
	}

	public int GetLODLevelUsingCamera(Camera currentCamera)
	{
		if (m_cachedFrameLODLevel.ContainsKey(currentCamera))
		{
			return m_cachedFrameLODLevel[currentCamera];
		}
		if (m_listLODLevels == null || m_listLODLevels.Count == 0)
		{
			return -1;
		}
		float num = 0f;
		float num2 = 0f;
		if (m_evalMode == EvalMode.CameraDistance)
		{
			Vector3 a = base.transform.TransformPoint(m_localCenter.x, m_localCenter.y, m_localCenter.z);
			num = Vector3.Distance(a, currentCamera.transform.position);
		}
		else if (m_evalMode == EvalMode.ScreenCoverage)
		{
			num2 = ComputeScreenCoverage(currentCamera);
		}
		int num3 = 0;
		for (num3 = 0; num3 < m_listLODLevels.Count && num3 != m_listLODLevels.Count - 1; num3++)
		{
			if (m_evalMode == EvalMode.CameraDistance)
			{
				if (num < m_listLODLevels[num3 + 1].m_fMaxCameraDistance)
				{
					break;
				}
			}
			else if (m_evalMode == EvalMode.ScreenCoverage && num2 > m_listLODLevels[num3 + 1].m_fScreenCoverage)
			{
				break;
			}
		}
		m_cachedFrameLODLevel.Add(currentCamera, num3);
		return num3;
	}

	public int GetCurrentLODLevel()
	{
		return m_nCurrentLOD;
	}

	public void SwitchToLOD(int nLevel, bool bRecurseIntoChildren)
	{
		if (m_LODGroup != null)
		{
			m_LODGroup.ForceLOD(nLevel);
		}
		else
		{
			SwitchToLODRecursive(this, base.gameObject, nLevel, bRecurseIntoChildren);
		}
	}

	private static void SwitchToLODRecursive(AutomaticLOD root, GameObject gameObject, int nLODLevel, bool bRecurseIntoChildren)
	{
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		if (component != null && IsRootOrBelongsToLODTree(component, root) && nLODLevel >= 0 && nLODLevel < component.m_listLODLevels.Count && component.m_nCurrentLOD != nLODLevel)
		{
			if (component.LODSwitchMode == SwitchMode.SwitchMesh)
			{
				MeshFilter component2 = gameObject.GetComponent<MeshFilter>();
				if (component2 != null)
				{
					Mesh mesh = ((!component.m_listLODLevels[nLODLevel].m_bUsesOriginalMesh) ? component.m_listLODLevels[nLODLevel].m_mesh : component.m_originalMesh);
					if (component2.sharedMesh != mesh)
					{
						component2.sharedMesh = mesh;
					}
				}
				else
				{
					SkinnedMeshRenderer component3 = gameObject.GetComponent<SkinnedMeshRenderer>();
					if (component3 != null)
					{
						Mesh mesh2 = ((!component.m_listLODLevels[nLODLevel].m_bUsesOriginalMesh) ? component.m_listLODLevels[nLODLevel].m_mesh : component.m_originalMesh);
						if (component3.sharedMesh != mesh2)
						{
							if (mesh2 != null && mesh2.vertexCount == 0)
							{
								if (component3.sharedMesh != null)
								{
									component3.sharedMesh = null;
								}
							}
							else
							{
								component3.sharedMesh = mesh2;
							}
						}
					}
				}
			}
			else if (component.LODSwitchMode == SwitchMode.SwitchGameObject)
			{
				if (gameObject.GetComponent<Renderer>() != null)
				{
					gameObject.GetComponent<Renderer>().enabled = component.m_listLODLevels[nLODLevel].m_bUsesOriginalMesh;
				}
				for (int i = 0; i < component.m_listLODLevels.Count; i++)
				{
					if (component.m_listLODLevels[i].m_gameObject != null)
					{
						component.m_listLODLevels[i].m_gameObject.SetActive(!component.m_listLODLevels[nLODLevel].m_bUsesOriginalMesh && i == nLODLevel && component.gameObject.activeSelf);
					}
				}
			}
			else if (component.LODSwitchMode == SwitchMode.UnityLODGroup)
			{
				return;
			}
			component.m_nCurrentLOD = nLODLevel;
		}
		if (bRecurseIntoChildren)
		{
			for (int j = 0; j < gameObject.transform.childCount; j++)
			{
				SwitchToLODRecursive(root, gameObject.transform.GetChild(j).gameObject, nLODLevel, true);
			}
		}
	}

	public void ComputeAllLODMeshes(bool bRecurseIntoChildren, Simplifier.ProgressDelegate progress = null)
	{
		if (m_listLODLevels == null)
		{
			return;
		}
		for (int i = 0; i < m_listLODLevels.Count; i++)
		{
			ComputeLODMeshRecursive(this, base.gameObject, i, bRecurseIntoChildren, progress);
			if (Simplifier.Cancelled)
			{
				return;
			}
		}
		SetupLODGroup();
	}

	public void ComputeLODMesh(int nLevel, bool bRecurseIntoChildren, Simplifier.ProgressDelegate progress = null)
	{
		ComputeLODMeshRecursive(this, base.gameObject, nLevel, bRecurseIntoChildren, progress);
		SetupLODGroup();
	}

	private static void ComputeLODMeshRecursive(AutomaticLOD root, GameObject gameObject, int nLevel, bool bRecurseIntoChildren, Simplifier.ProgressDelegate progress = null)
	{
		if (Simplifier.Cancelled)
		{
			return;
		}
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		if (component != null && IsRootOrBelongsToLODTree(component, root) && component.m_meshSimplifier != null)
		{
			if ((bool)component.m_listLODLevels[nLevel].m_mesh)
			{
				component.m_listLODLevels[nLevel].m_mesh.Clear();
			}
			float fMeshVerticesAmount = component.m_listLODLevels[nLevel].m_fMeshVerticesAmount;
			if (!component.m_bOverrideRootSettings && component.m_LODObjectRoot != null)
			{
				fMeshVerticesAmount = component.m_LODObjectRoot.m_listLODLevels[nLevel].m_fMeshVerticesAmount;
			}
			if (component.m_listLODLevels[nLevel].m_mesh == null)
			{
				component.m_listLODLevels[nLevel].m_mesh = CreateNewEmptyMesh(component);
			}
			CheckForAdditionalLODSetup(root, component, component.m_listLODLevels[nLevel], nLevel);
			int num = Mathf.RoundToInt(fMeshVerticesAmount * (float)component.m_meshSimplifier.GetOriginalMeshUniqueVertexCount());
			if (num < component.m_meshSimplifier.GetOriginalMeshUniqueVertexCount())
			{
				component.m_listLODLevels[nLevel].m_bUsesOriginalMesh = false;
				IEnumerator enumerator = component.m_meshSimplifier.ComputeMeshWithVertexCount(gameObject, component.m_listLODLevels[nLevel].m_mesh, num, component.name + " LOD " + nLevel, progress);
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
			else
			{
				component.m_listLODLevels[nLevel].m_bUsesOriginalMesh = true;
				if (component.m_listLODLevels[nLevel].m_gameObject != null && component.m_listLODLevels[nLevel].m_gameObject.GetComponent<Renderer>() != null)
				{
					component.m_listLODLevels[nLevel].m_gameObject.GetComponent<Renderer>().enabled = !component.m_listLODLevels[nLevel].m_bUsesOriginalMesh;
				}
			}
		}
		if (!bRecurseIntoChildren)
		{
			return;
		}
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			ComputeLODMeshRecursive(root, gameObject.transform.GetChild(i).gameObject, nLevel, bRecurseIntoChildren, progress);
			if (Simplifier.Cancelled)
			{
				break;
			}
		}
	}

	public void RestoreOriginalMesh(bool bDeleteLODData, bool bRecurseIntoChildren)
	{
		if (m_LODGroup != null)
		{
			m_LODGroup.ForceLOD(-1);
		}
		RestoreOriginalMeshRecursive(this, base.gameObject, bDeleteLODData, bRecurseIntoChildren);
	}

	private static void RestoreOriginalMeshRecursive(AutomaticLOD root, GameObject gameObject, bool bDeleteLODData, bool bRecurseIntoChildren)
	{
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		if (component != null && IsRootOrBelongsToLODTree(component, root))
		{
			if (component.LODSwitchMode != SwitchMode.SwitchGameObject && component.m_originalMesh != null)
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
			component.m_nCurrentLOD = -1;
			if (component.LODSwitchMode == SwitchMode.SwitchGameObject)
			{
				if (gameObject.GetComponent<Renderer>() != null)
				{
					gameObject.GetComponent<Renderer>().enabled = true;
				}
				for (int i = 0; i < component.m_listLODLevels.Count; i++)
				{
					if (component.m_listLODLevels[i].m_gameObject != null)
					{
						component.m_listLODLevels[i].m_gameObject.SetActive(false);
					}
				}
			}
			if (bDeleteLODData)
			{
				component.FreeLODData(false);
				component.m_listLODLevels.Clear();
				component.m_listLODLevels = null;
				component.m_listDependentChildren.Clear();
			}
		}
		if (bRecurseIntoChildren)
		{
			for (int j = 0; j < gameObject.transform.childCount; j++)
			{
				RestoreOriginalMeshRecursive(root, gameObject.transform.GetChild(j).gameObject, bDeleteLODData, bRecurseIntoChildren);
			}
		}
	}

	public bool HasOriginalMeshActive(bool bRecurseIntoChildren)
	{
		return HasOriginalMeshActiveRecursive(this, base.gameObject, bRecurseIntoChildren);
	}

	private static bool HasOriginalMeshActiveRecursive(AutomaticLOD root, GameObject gameObject, bool bRecurseIntoChildren)
	{
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		bool flag = false;
		if (component != null && IsRootOrBelongsToLODTree(component, root) && component.m_originalMesh != null)
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

	public bool HasVertexData(int nLevel, bool bRecurseIntoChildren)
	{
		return HasVertexDataRecursive(this, base.gameObject, nLevel, bRecurseIntoChildren);
	}

	private static bool HasVertexDataRecursive(AutomaticLOD root, GameObject gameObject, int nLevel, bool bRecurseIntoChildren)
	{
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		if (component != null && IsRootOrBelongsToLODTree(component, root))
		{
			if (component.m_listLODLevels[nLevel].m_bUsesOriginalMesh)
			{
				if ((bool)component.m_originalMesh && component.m_originalMesh.vertexCount > 0)
				{
					return true;
				}
			}
			else if ((bool)component.m_listLODLevels[nLevel].m_mesh && component.m_listLODLevels[nLevel].m_mesh.vertexCount > 0)
			{
				return true;
			}
		}
		if (bRecurseIntoChildren)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				if (HasVertexDataRecursive(root, gameObject.transform.GetChild(i).gameObject, nLevel, bRecurseIntoChildren))
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

	private static void GetOriginalVertexCountRecursive(AutomaticLOD root, GameObject gameObject, ref int nVertexCount, bool bRecurseIntoChildren)
	{
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		if (component != null && IsRootOrBelongsToLODTree(component, root) && component.m_originalMesh != null)
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

	private static void GetOriginalTriangleCountRecursive(AutomaticLOD root, GameObject gameObject, ref int nTriangleCount, bool bRecurseIntoChildren)
	{
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		if (component != null && IsRootOrBelongsToLODTree(component, root) && component.m_originalMesh != null)
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

	public int GetCurrentVertexCount(bool bRecurseIntoChildren)
	{
		int nVertexCount = 0;
		GetCurrentVertexCountRecursive(this, base.gameObject, ref nVertexCount, bRecurseIntoChildren);
		return nVertexCount;
	}

	private static void GetCurrentVertexCountRecursive(AutomaticLOD root, GameObject gameObject, ref int nVertexCount, bool bRecurseIntoChildren)
	{
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		if (component != null && IsRootOrBelongsToLODTree(component, root))
		{
			MeshFilter component2 = gameObject.GetComponent<MeshFilter>();
			if (component2 != null && component2.sharedMesh != null)
			{
				nVertexCount += component2.sharedMesh.vertexCount;
			}
			else
			{
				SkinnedMeshRenderer component3 = gameObject.GetComponent<SkinnedMeshRenderer>();
				if (component3 != null && component3.sharedMesh != null)
				{
					nVertexCount += component3.sharedMesh.vertexCount;
				}
			}
		}
		if (bRecurseIntoChildren)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				GetCurrentVertexCountRecursive(root, gameObject.transform.GetChild(i).gameObject, ref nVertexCount, bRecurseIntoChildren);
			}
		}
	}

	public int GetLODVertexCount(int nLevel, bool bRecurseIntoChildren)
	{
		int nVertexCount = 0;
		GetLODVertexCountRecursive(this, base.gameObject, nLevel, ref nVertexCount, bRecurseIntoChildren);
		return nVertexCount;
	}

	private static void GetLODVertexCountRecursive(AutomaticLOD root, GameObject gameObject, int nLevel, ref int nVertexCount, bool bRecurseIntoChildren)
	{
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		if (component != null && IsRootOrBelongsToLODTree(component, root))
		{
			if (component.m_listLODLevels[nLevel].m_bUsesOriginalMesh && component.m_originalMesh != null)
			{
				nVertexCount += component.m_originalMesh.vertexCount;
			}
			else if (component.m_listLODLevels[nLevel].m_mesh != null)
			{
				nVertexCount += component.m_listLODLevels[nLevel].m_mesh.vertexCount;
			}
		}
		if (bRecurseIntoChildren)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				GetLODVertexCountRecursive(root, gameObject.transform.GetChild(i).gameObject, nLevel, ref nVertexCount, bRecurseIntoChildren);
			}
		}
	}

	public int GetLODTriangleCount(int nLevel, bool bRecurseIntoChildren)
	{
		int nTriangleCount = 0;
		GetLODTriangleCountRecursive(this, base.gameObject, nLevel, ref nTriangleCount, bRecurseIntoChildren);
		return nTriangleCount;
	}

	private static void GetLODTriangleCountRecursive(AutomaticLOD root, GameObject gameObject, int nLevel, ref int nTriangleCount, bool bRecurseIntoChildren)
	{
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		if (component != null && IsRootOrBelongsToLODTree(component, root))
		{
			if (component.m_listLODLevels[nLevel].m_bUsesOriginalMesh && component.m_originalMesh != null)
			{
				nTriangleCount += component.m_originalMesh.triangles.Length / 3;
			}
			else if (component.m_listLODLevels[nLevel].m_mesh != null)
			{
				nTriangleCount += component.m_listLODLevels[nLevel].m_mesh.triangles.Length / 3;
			}
		}
		if (bRecurseIntoChildren)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				GetLODTriangleCountRecursive(root, gameObject.transform.GetChild(i).gameObject, nLevel, ref nTriangleCount, bRecurseIntoChildren);
			}
		}
	}

	public void RemoveFromLODTree()
	{
		if (m_LODObjectRoot != null)
		{
			m_LODObjectRoot.m_listDependentChildren.Remove(this);
		}
		RestoreOriginalMesh(true, false);
	}

	public void FreeLODData(bool bRecurseIntoChildren)
	{
		FreeLODDataRecursive(this, base.gameObject, bRecurseIntoChildren);
	}

	private static void FreeLODDataRecursive(AutomaticLOD root, GameObject gameObject, bool bRecurseIntoChildren)
	{
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		if (component != null && IsRootOrBelongsToLODTree(component, root))
		{
			if (component.m_listLODLevels != null)
			{
				foreach (LODLevelData listLODLevel in component.m_listLODLevels)
				{
					if ((bool)listLODLevel.m_mesh)
					{
						listLODLevel.m_mesh.Clear();
					}
					if (listLODLevel.m_gameObject != null)
					{
						if (Application.isEditor && !Application.isPlaying)
						{
							UnityEngine.Object.DestroyImmediate(listLODLevel.m_gameObject);
						}
						else
						{
							UnityEngine.Object.Destroy(listLODLevel.m_gameObject);
						}
					}
					listLODLevel.m_bUsesOriginalMesh = false;
				}
			}
			Simplifier[] components = component.GetComponents<Simplifier>();
			for (int i = 0; i < components.Length; i++)
			{
				if (Application.isEditor && !Application.isPlaying)
				{
					UnityEngine.Object.DestroyImmediate(components[i]);
				}
				else
				{
					UnityEngine.Object.Destroy(components[i]);
				}
			}
			if (component.m_meshSimplifier != null)
			{
				component.m_meshSimplifier = null;
			}
			if (component.m_LODGroup != null)
			{
				if (Application.isEditor && !Application.isPlaying)
				{
					UnityEngine.Object.DestroyImmediate(component.m_LODGroup);
				}
				else
				{
					UnityEngine.Object.Destroy(component.m_LODGroup);
				}
			}
			component.m_bLODDataDirty = true;
		}
		if (bRecurseIntoChildren)
		{
			for (int j = 0; j < gameObject.transform.childCount; j++)
			{
				FreeLODDataRecursive(root, gameObject.transform.GetChild(j).gameObject, bRecurseIntoChildren);
			}
		}
	}

	private static Mesh CreateNewEmptyMesh(AutomaticLOD automaticLOD)
	{
		if (automaticLOD.m_originalMesh == null)
		{
			return new Mesh();
		}
		Mesh mesh = UnityEngine.Object.Instantiate(automaticLOD.m_originalMesh);
		mesh.Clear();
		return mesh;
	}

	private static GameObject CreateBasicObjectCopy(GameObject gameObject, Mesh mesh, Transform parent)
	{
		GameObject gameObject2 = new GameObject();
		gameObject2.layer = gameObject.layer;
		gameObject2.isStatic = gameObject.isStatic;
		gameObject2.tag = gameObject.tag;
		gameObject2.transform.parent = parent;
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.transform.localRotation = Quaternion.identity;
		gameObject2.transform.localScale = Vector3.one;
		Component[] components = gameObject.GetComponents<Component>();
		foreach (Component component in components)
		{
			if (component.GetType() == typeof(MeshRenderer) || component.GetType() == typeof(MeshFilter) || component.GetType() == typeof(SkinnedMeshRenderer))
			{
				CopyComponent(component, gameObject2);
			}
		}
		MeshFilter component2 = gameObject2.GetComponent<MeshFilter>();
		if (component2 != null)
		{
			component2.sharedMesh = mesh;
		}
		else
		{
			SkinnedMeshRenderer component3 = gameObject2.GetComponent<SkinnedMeshRenderer>();
			if (component3 != null)
			{
				component3.sharedMesh = mesh;
			}
		}
		if (gameObject2.GetComponent<Renderer>() != null)
		{
			gameObject2.GetComponent<Renderer>().enabled = true;
		}
		return gameObject2;
	}

	private static void CheckForAdditionalLODSetup(AutomaticLOD root, AutomaticLOD automaticLOD, LODLevelData levelData, int level)
	{
		if (automaticLOD.LODSwitchMode == SwitchMode.SwitchGameObject || automaticLOD.LODSwitchMode == SwitchMode.UnityLODGroup)
		{
			if (levelData.m_gameObject != null)
			{
				if (Application.isEditor && !Application.isPlaying)
				{
					UnityEngine.Object.DestroyImmediate(levelData.m_gameObject);
				}
				else
				{
					UnityEngine.Object.Destroy(levelData.m_gameObject);
				}
			}
			levelData.m_gameObject = CreateBasicObjectCopy(automaticLOD.gameObject, levelData.m_mesh, automaticLOD.gameObject.transform);
			levelData.m_gameObject.SetActive(automaticLOD.LODSwitchMode == SwitchMode.UnityLODGroup);
			for (int i = 0; i < automaticLOD.m_listLODLevels.Count; i++)
			{
				if (automaticLOD.m_listLODLevels[i].m_gameObject != null)
				{
					automaticLOD.m_listLODLevels[i].m_gameObject.name = "LOD" + i;
					automaticLOD.m_listLODLevels[i].m_gameObject.transform.SetSiblingIndex(i);
				}
			}
		}
		else
		{
			levelData.m_gameObject = null;
		}
	}

	public void SetupLODGroup()
	{
		AutomaticLOD automaticLOD = ((!(m_LODObjectRoot == null)) ? m_LODObjectRoot : this);
		if (!(automaticLOD != null) || automaticLOD.m_switchMode != SwitchMode.UnityLODGroup)
		{
			return;
		}
		if (automaticLOD.m_LODGroup == null)
		{
			automaticLOD.m_LODGroup = automaticLOD.gameObject.GetComponent<LODGroup>();
			if (automaticLOD.m_LODGroup == null)
			{
				automaticLOD.m_LODGroup = automaticLOD.gameObject.AddComponent<LODGroup>();
			}
		}
		LOD[] lODs = automaticLOD.m_LODGroup.GetLODs();
		List<List<Renderer>> renderers = new List<List<Renderer>>();
		for (int i = 0; i < automaticLOD.GetNumberOfLevelsToGenerate(); i++)
		{
			renderers.Add(new List<Renderer>());
		}
		SetupLODGroupRecursive(automaticLOD, automaticLOD.gameObject, ref renderers);
		for (int j = 0; j < renderers.Count; j++)
		{
			lODs[j].renderers = renderers[j].ToArray();
		}
		for (int k = renderers.Count; k < lODs.Length; k++)
		{
			lODs[k].renderers = null;
		}
		automaticLOD.m_LODGroup.SetLODs(lODs);
		automaticLOD.m_LODGroup.RecalculateBounds();
	}

	private static void SetupLODGroupRecursive(AutomaticLOD root, GameObject gameObject, ref List<List<Renderer>> renderers)
	{
		AutomaticLOD component = gameObject.GetComponent<AutomaticLOD>();
		if (component != null && IsRootOrBelongsToLODTree(component, root) && component.m_listLODLevels != null)
		{
			bool flag = false;
			for (int i = 0; i < component.m_listLODLevels.Count; i++)
			{
				LODLevelData lODLevelData = component.m_listLODLevels[i];
				Renderer renderer = (lODLevelData.m_bUsesOriginalMesh ? gameObject.GetComponent<Renderer>() : ((!(lODLevelData.m_gameObject != null)) ? null : lODLevelData.m_gameObject.GetComponent<Renderer>()));
				if (renderer != null && !renderers[i].Contains(renderer))
				{
					renderers[i].Add(renderer);
				}
				if (lODLevelData.m_bUsesOriginalMesh)
				{
					flag = true;
				}
			}
			Renderer component2 = gameObject.GetComponent<Renderer>();
			if (component2 != null)
			{
				component2.enabled = flag;
			}
		}
		for (int j = 0; j < gameObject.transform.childCount; j++)
		{
			SetupLODGroupRecursive(root, gameObject.transform.GetChild(j).gameObject, ref renderers);
		}
	}

	private static Component CopyComponent(Component original, GameObject destination)
	{
		return null;
	}

	private void BuildCornerData(ref Vector3[] av3Corners, Bounds bounds)
	{
		av3Corners[0].x = bounds.min.x;
		av3Corners[0].y = bounds.min.y;
		av3Corners[0].z = bounds.min.z;
		av3Corners[1].x = bounds.min.x;
		av3Corners[1].y = bounds.min.y;
		av3Corners[1].z = bounds.max.z;
		av3Corners[2].x = bounds.min.x;
		av3Corners[2].y = bounds.max.y;
		av3Corners[2].z = bounds.min.z;
		av3Corners[3].x = bounds.min.x;
		av3Corners[3].y = bounds.max.y;
		av3Corners[3].z = bounds.max.z;
		av3Corners[4].x = bounds.max.x;
		av3Corners[4].y = bounds.min.y;
		av3Corners[4].z = bounds.min.z;
		av3Corners[5].x = bounds.max.x;
		av3Corners[5].y = bounds.min.y;
		av3Corners[5].z = bounds.max.z;
		av3Corners[6].x = bounds.max.x;
		av3Corners[6].y = bounds.max.y;
		av3Corners[6].z = bounds.min.z;
		av3Corners[7].x = bounds.max.x;
		av3Corners[7].y = bounds.max.y;
		av3Corners[7].z = bounds.max.z;
	}

	private bool IsDependent()
	{
		return m_LODObjectRoot != null || m_LODObjectRootPersist != null;
	}
}
