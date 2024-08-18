using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeshCombineStudio
{
	[ExecuteInEditMode]
	public class MeshCombiner : MonoBehaviour
	{
		public enum HandleComponent
		{
			Disable = 0,
			Destroy = 1
		}

		public enum ObjectCenter
		{
			BoundsCenter = 0,
			TransformPosition = 1
		}

		public enum BackFaceTriangleMode
		{
			Box = 0,
			Direction = 1
		}

		public delegate void DefaultMethod();

		public enum RebakeLightingMode
		{
			CopyLightmapUvs = 0,
			RegenarateLightmapUvs = 1
		}

		[Serializable]
		public class SearchOptions
		{
			public enum ComponentCondition
			{
				And = 0,
				Or = 1
			}

			public GameObject parent;

			public ObjectCenter objectCenter;

			public bool useSearchBox;

			public Bounds searchBoxBounds;

			public bool searchBoxSquare;

			public Vector3 searchBoxPivot;

			public Vector3 searchBoxSize = new Vector3(25f, 25f, 25f);

			public bool useMaxBoundsFactor = true;

			public float maxBoundsFactor = 1.5f;

			public bool useVertexInputLimit = true;

			public int vertexInputLimit = 5000;

			public bool useVertexInputLimitLod = true;

			public int vertexInputLimitLod = 10000;

			public bool useLayerMask;

			public LayerMask layerMask = -1;

			public bool useTag;

			public string tag;

			public bool useNameContains;

			public List<string> nameContainList = new List<string>();

			public bool onlyActive = true;

			public bool onlyStatic = true;

			public bool useComponentsFilter;

			public ComponentCondition componentCondition;

			public List<string> componentNameList = new List<string>();

			public SearchOptions(GameObject parent)
			{
				this.parent = parent;
			}

			public void GetSearchBoxBounds()
			{
				searchBoxBounds = new Bounds(searchBoxPivot + new Vector3(0f, searchBoxSize.y * 0.5f, 0f), searchBoxSize);
			}
		}

		[Serializable]
		public class LODGroupSettings
		{
			public LODSettings[] lodSettings;

			public LODGroupSettings(int lodParentIndex)
			{
				int num = lodParentIndex + 1;
				lodSettings = new LODSettings[num];
				float num2 = 1f / (float)num;
				for (int i = 0; i < lodSettings.Length; i++)
				{
					lodSettings[i] = new LODSettings(1f - num2 * (float)(i + 1));
				}
			}
		}

		[Serializable]
		public class LODSettings
		{
			public float screenRelativeTransitionHeight;

			public float fadeTransitionWidth;

			public LODSettings(float screenRelativeTransitionHeight)
			{
				this.screenRelativeTransitionHeight = screenRelativeTransitionHeight;
			}
		}

		public class LodParentHolder
		{
			public GameObject go;

			public Transform t;

			public bool found;

			public int[] lods;

			public LodParentHolder(int lodCount)
			{
				lods = new int[lodCount];
			}

			public void Create(MeshCombiner meshCombiner, int lodParentIndex)
			{
				go = new GameObject("LODGroup " + (lodParentIndex + 1));
				LODGroupSetup lODGroupSetup = go.AddComponent<LODGroupSetup>();
				lODGroupSetup.Init(meshCombiner, lodParentIndex);
				t = go.transform;
				Transform transform = t.transform;
				transform.parent = meshCombiner.transform;
			}

			public void Reset()
			{
				found = false;
				Array.Clear(lods, 0, lods.Length);
			}
		}

		public static List<MeshCombiner> instances = new List<MeshCombiner>();

		public MeshCombineJobManager.JobSettings jobSettings = new MeshCombineJobManager.JobSettings();

		public LODGroupSettings[] lodGroupsSettings;

		public GameObject instantiatePrefab;

		public const int maxLodCount = 8;

		public string saveMeshesFolder;

		public ObjectOctree.Cell octree;

		public List<ObjectOctree.MaxCell> changedCells;

		[NonSerialized]
		public bool octreeContainsObjects;

		public bool useCells = true;

		public int cellSize = 32;

		public Vector3 cellOffset;

		public bool useVertexOutputLimit;

		public int vertexOutputLimit = 65534;

		public RebakeLightingMode rebakeLightingMode;

		public bool copyBakedLighting;

		public bool validCopyBakedLighting;

		public bool rebakeLighting;

		public bool validRebakeLighting;

		public int outputLayer;

		public float scaleInLightmap = 1f;

		public bool addMeshColliders;

		public bool makeMeshesUnreadable = true;

		public bool removeTrianglesBelowSurface;

		public LayerMask surfaceLayerMask;

		public int maxSurfaceHeight = 1000;

		public bool removeBackFaceTriangles;

		public BackFaceTriangleMode backFaceTriangleMode;

		public Vector3 backFaceDirection;

		public Bounds backFaceBounds;

		public bool twoSidedShadows = true;

		public bool combineInRuntime;

		public bool combineOnStart = true;

		public bool useCombineSwapKey;

		public KeyCode combineSwapKey = KeyCode.Tab;

		public HandleComponent originalMeshRenderers;

		public HandleComponent originalLODGroups;

		public SearchOptions searchOptions;

		public Vector3 oldPosition;

		public Vector3 oldScale;

		public LodParentHolder[] lodParentHolders = new LodParentHolder[8];

		[NonSerialized]
		public List<CachedGameObject> foundObjects = new List<CachedGameObject>();

		[NonSerialized]
		public List<CachedLodGameObject> foundLodObjects = new List<CachedLodGameObject>();

		private HashSet<Transform> uniqueLodObjects = new HashSet<Transform>();

		private HashSet<LODGroup> foundLodGroups = new HashSet<LODGroup>();

		public List<Mesh> unreadableMeshes = new List<Mesh>();

		public int mrDisabledCount;

		public bool combined;

		public bool activeOriginal = true;

		public bool combinedActive;

		public bool drawGizmos = true;

		public bool drawMeshBounds = true;

		public int originalTotalVertices;

		public int originalTotalTriangles;

		public int totalVertices;

		public int totalTriangles;

		public int originalDrawCalls;

		public int newDrawCalls;

		[NonSerialized]
		private MeshCombiner thisInstance;

		[NonSerialized]
		public int jobCount;

		private bool hasFoundFirstObject;

		private Bounds bounds;

		public event DefaultMethod OnCombiningReady;

		public void ExecuteOnCombiningReady()
		{
			if (this.OnCombiningReady != null)
			{
				this.OnCombiningReady();
			}
		}

		private void Awake()
		{
			instances.Add(this);
			thisInstance = this;
		}

		private void OnEnable()
		{
			if (thisInstance == null)
			{
				thisInstance = this;
				instances.Add(this);
			}
		}

		private void Start()
		{
			InitMeshCombineJobManager();
			if (instances[0] == this)
			{
				MeshCombineJobManager.instance.SetJobMode(jobSettings);
			}
			if (Application.isPlaying || !Application.isEditor)
			{
				StartRuntime();
			}
		}

		private void OnDestroy()
		{
			thisInstance = null;
			instances.Remove(this);
			if (instances.Count == 0 && MeshCombineJobManager.instance != null)
			{
				Methods.Destroy(MeshCombineJobManager.instance.gameObject);
				MeshCombineJobManager.instance = null;
			}
		}

		public static MeshCombiner GetInstance(string name)
		{
			for (int i = 0; i < instances.Count; i++)
			{
				if (instances[i].gameObject.name == name)
				{
					return instances[i];
				}
			}
			return null;
		}

		public void CopyJobSettingsToAllInstances()
		{
			for (int i = 0; i < instances.Count; i++)
			{
				instances[i].jobSettings.CopySettings(jobSettings);
			}
		}

		public void InitMeshCombineJobManager()
		{
			if (MeshCombineJobManager.instance == null)
			{
				MeshCombineJobManager.CreateInstance(this, instantiatePrefab);
			}
		}

		public void CreateLodGroupsSettings()
		{
			lodGroupsSettings = new LODGroupSettings[8];
			for (int i = 0; i < lodGroupsSettings.Length; i++)
			{
				lodGroupsSettings[i] = new LODGroupSettings(i);
			}
		}

		private void StartRuntime()
		{
			if (!combineInRuntime)
			{
				return;
			}
			if (combineOnStart)
			{
				CombineAll();
			}
			if (useCombineSwapKey && originalMeshRenderers == HandleComponent.Disable && originalLODGroups == HandleComponent.Disable)
			{
				if (SwapCombineKey.instance == null)
				{
					base.gameObject.AddComponent<SwapCombineKey>();
				}
				else
				{
					SwapCombineKey.instance.meshCombinerList.Add(this);
				}
			}
		}

		public void DestroyCombinedObjects()
		{
			RestoreOriginalRenderersAndLODGroups();
			Methods.DestroyChildren(base.transform);
			combined = false;
		}

		private void Reset()
		{
			RestoreOriginalRenderersAndLODGroups();
			foundObjects.Clear();
			uniqueLodObjects.Clear();
			foundLodGroups.Clear();
			foundLodObjects.Clear();
			unreadableMeshes.Clear();
			ResetOctree();
			hasFoundFirstObject = false;
			ref Bounds reference = ref bounds;
			Vector3 zero = Vector3.zero;
			bounds.size = zero;
			reference.center = zero;
			if (searchOptions.useSearchBox)
			{
				searchOptions.GetSearchBoxBounds();
			}
			InitAndResetLodParentsCount();
		}

		public void AddObjects(List<Transform> transforms, bool useSearchOptions, bool checkForLODGroups = true)
		{
			List<LODGroup> list = new List<LODGroup>();
			if (checkForLODGroups)
			{
				for (int i = 0; i < transforms.Count; i++)
				{
					LODGroup component = transforms[i].GetComponent<LODGroup>();
					if (component != null)
					{
						list.Add(component);
					}
				}
				if (list.Count > 0)
				{
					AddLodGroups(list.ToArray(), useSearchOptions);
				}
			}
			AddTransforms(transforms.ToArray(), useSearchOptions);
		}

		public void AddObjectsAutomatically()
		{
			Reset();
			AddObjectsFromSearchParent();
			AddFoundObjectsToOctree();
			if (octreeContainsObjects)
			{
				octree.SortObjects(this);
			}
			if (Console.instance != null)
			{
				LogOctreeInfo();
			}
		}

		public void AddFoundObjectsToOctree()
		{
			if (foundObjects.Count > 0 || foundLodObjects.Count > 0)
			{
				octreeContainsObjects = true;
				CalcOctreeSize(bounds);
				ObjectOctree.MaxCell.maxCellCount = 0;
				for (int i = 0; i < foundObjects.Count; i++)
				{
					octree.AddObject(this, foundObjects[i], 0, 0);
				}
				for (int j = 0; j < foundLodObjects.Count; j++)
				{
					CachedLodGameObject cachedLodGameObject = foundLodObjects[j];
					octree.AddObject(this, cachedLodGameObject, cachedLodGameObject.lodCount, cachedLodGameObject.lodLevel);
				}
			}
			else
			{
				Debug.Log("No matching GameObjects with chosen search options are found for combining.");
			}
		}

		public void ResetOctree()
		{
			octreeContainsObjects = false;
			if (octree == null)
			{
				octree = new ObjectOctree.Cell();
				return;
			}
			BaseOctree.Cell[] cells = octree.cells;
			octree.Reset(ref cells);
		}

		public void CalcOctreeSize(Bounds bounds)
		{
			Methods.SnapBoundsAndPreserveArea(ref bounds, cellSize, (!useCells) ? Vector3.zero : cellOffset);
			int num2;
			float num3;
			if (useCells)
			{
				float num = Mathf.Max(Methods.GetMax(bounds.size), cellSize);
				num2 = Mathf.CeilToInt(Mathf.Log(num / (float)cellSize, 2f));
				num3 = (int)Mathf.Pow(2f, num2) * cellSize;
			}
			else
			{
				num3 = Methods.GetMax(bounds.size);
				num2 = 0;
			}
			if (num2 == 0 && octree != null)
			{
				octree = new ObjectOctree.MaxCell();
			}
			else if (num2 > 0 && octree is ObjectOctree.MaxCell)
			{
				octree = new ObjectOctree.Cell();
			}
			octree.maxLevels = num2;
			octree.bounds.center = bounds.center;
			octree.bounds.size = new Vector3(num3, num3, num3);
		}

		public void ApplyChanges()
		{
			validRebakeLighting = rebakeLighting && !validCopyBakedLighting && !Application.isPlaying && Application.isEditor;
			for (int i = 0; i < changedCells.Count; i++)
			{
				ObjectOctree.MaxCell maxCell = changedCells[i];
				maxCell.hasChanged = false;
				maxCell.ApplyChanges(this);
			}
			changedCells.Clear();
		}

		public void CombineAll()
		{
			DestroyCombinedObjects();
			if (!octreeContainsObjects)
			{
				AddObjectsAutomatically();
			}
			if (!octreeContainsObjects)
			{
				return;
			}
			validRebakeLighting = rebakeLighting && !validCopyBakedLighting && !Application.isPlaying && Application.isEditor;
			totalVertices = (totalTriangles = (originalTotalVertices = (originalTotalTriangles = (originalDrawCalls = (newDrawCalls = 0)))));
			for (int i = 0; i < lodParentHolders.Length; i++)
			{
				LodParentHolder lodParentHolder = lodParentHolders[i];
				if (lodParentHolder.found)
				{
					if (lodParentHolder.go == null)
					{
						lodParentHolder.Create(this, i);
					}
					octree.CombineMeshes(this, i);
				}
			}
			if (MeshCombineJobManager.instance.jobSettings.combineJobMode == MeshCombineJobManager.CombineJobMode.CombineAtOnce)
			{
				MeshCombineJobManager.instance.ExecuteJobs();
			}
			combinedActive = true;
			combined = true;
			activeOriginal = false;
			ExecuteHandleObjects(activeOriginal, HandleComponent.Disable, HandleComponent.Disable);
		}

		private void InitAndResetLodParentsCount()
		{
			for (int i = 0; i < lodParentHolders.Length; i++)
			{
				if (lodParentHolders[i] == null)
				{
					lodParentHolders[i] = new LodParentHolder(i + 1);
				}
				else
				{
					lodParentHolders[i].Reset();
				}
			}
		}

		public void AddObjectsFromSearchParent()
		{
			if (searchOptions.parent == null)
			{
				Debug.Log("You need to assign a 'Parent' GameObject in which meshes will be searched");
				return;
			}
			LODGroup[] componentsInChildren = searchOptions.parent.GetComponentsInChildren<LODGroup>(true);
			AddLodGroups(componentsInChildren);
			Transform[] componentsInChildren2 = searchOptions.parent.GetComponentsInChildren<Transform>(true);
			AddTransforms(componentsInChildren2);
		}

		private void AddLodGroups(LODGroup[] lodGroups, bool useSearchOptions = true)
		{
			List<CachedLodGameObject> list = new List<CachedLodGameObject>();
			foreach (LODGroup lODGroup in lodGroups)
			{
				if (searchOptions.onlyActive && !lODGroup.gameObject.activeInHierarchy)
				{
					continue;
				}
				LOD[] lODs = lODGroup.GetLODs();
				int num = lODs.Length - 1;
				if (num <= 0)
				{
					continue;
				}
				for (int j = 0; j < lODs.Length; j++)
				{
					LOD lOD = lODs[j];
					int num2 = 0;
					while (num2 < lOD.renderers.Length)
					{
						Renderer renderer = lOD.renderers[num2];
						if (renderer == null)
						{
							goto IL_0089;
						}
						Transform transform = renderer.transform;
						uniqueLodObjects.Add(transform);
						CachedGameObject cachedGameObject = ValidObject(transform, true, useSearchOptions);
						if (cachedGameObject != null)
						{
							list.Add(new CachedLodGameObject(cachedGameObject, num, j));
							foundLodGroups.Add(lODGroup);
							num2++;
							continue;
						}
						goto IL_00be;
					}
					continue;
					IL_00be:
					list.Clear();
					break;
					IL_0089:
					list.Clear();
					break;
				}
				for (int k = 0; k < list.Count; k++)
				{
					CachedLodGameObject cachedLodGameObject = list[k];
					if (!hasFoundFirstObject)
					{
						bounds.center = cachedLodGameObject.mr.bounds.center;
						hasFoundFirstObject = true;
					}
					bounds.Encapsulate(cachedLodGameObject.mr.bounds);
					foundLodObjects.Add(cachedLodGameObject);
					lodParentHolders[num].found = true;
					lodParentHolders[num].lods[cachedLodGameObject.lodLevel]++;
				}
				list.Clear();
			}
		}

		private void AddTransforms(Transform[] transforms, bool useSearchOptions = true)
		{
			int count = uniqueLodObjects.Count;
			foreach (Transform transform in transforms)
			{
				if (count > 0 && uniqueLodObjects.Contains(transform))
				{
					continue;
				}
				CachedGameObject cachedGameObject = ValidObject(transform, false, useSearchOptions);
				if (cachedGameObject != null)
				{
					if (!hasFoundFirstObject)
					{
						bounds.center = cachedGameObject.mr.bounds.center;
						hasFoundFirstObject = true;
					}
					bounds.Encapsulate(cachedGameObject.mr.bounds);
					foundObjects.Add(cachedGameObject);
					lodParentHolders[0].lods[0]++;
				}
			}
			if (foundObjects.Count > 0)
			{
				lodParentHolders[0].found = true;
			}
		}

		private CachedGameObject ValidObject(Transform t, bool isLodObject, bool useSearchOptions = true)
		{
			GameObject gameObject = t.gameObject;
			MeshRenderer component = t.GetComponent<MeshRenderer>();
			if (component == null)
			{
				return null;
			}
			MeshFilter component2 = t.GetComponent<MeshFilter>();
			if (component2 == null)
			{
				return null;
			}
			Mesh sharedMesh = component2.sharedMesh;
			if (sharedMesh == null)
			{
				return null;
			}
			if (!sharedMesh.isReadable)
			{
				Debug.LogError("Mesh Combine Studio -> Read/Write is disabled on the mesh on GameObject " + gameObject.name + " and can't be combined. Click the 'Make Meshes Readable' in the MCS Inspector to make it automatically readable in the mesh import settings.");
				unreadableMeshes.Add(sharedMesh);
				return null;
			}
			if (useSearchOptions)
			{
				if (searchOptions.onlyActive && !gameObject.activeInHierarchy)
				{
					return null;
				}
				if (searchOptions.useLayerMask)
				{
					int num = 1 << t.gameObject.layer;
					if ((searchOptions.layerMask.value & num) != num)
					{
						return null;
					}
				}
				if (searchOptions.useTag && !t.CompareTag(searchOptions.tag))
				{
					return null;
				}
				if (searchOptions.useComponentsFilter)
				{
					if (searchOptions.componentCondition == SearchOptions.ComponentCondition.And)
					{
						bool flag = true;
						for (int i = 0; i < searchOptions.componentNameList.Count; i++)
						{
							if (t.GetComponent(searchOptions.componentNameList[i]) == null)
							{
								flag = false;
								break;
							}
						}
						if (!flag)
						{
							return null;
						}
					}
					else
					{
						bool flag2 = false;
						for (int j = 0; j < searchOptions.componentNameList.Count; j++)
						{
							if (t.GetComponent(searchOptions.componentNameList[j]) != null)
							{
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							return null;
						}
					}
				}
				if (searchOptions.useSearchBox)
				{
					if (searchOptions.objectCenter == ObjectCenter.BoundsCenter)
					{
						if (!searchOptions.searchBoxBounds.Contains(component.bounds.center))
						{
							return null;
						}
					}
					else if (!searchOptions.searchBoxBounds.Contains(t.position))
					{
						return null;
					}
				}
				if (searchOptions.onlyStatic && !gameObject.isStatic)
				{
					return null;
				}
				if (isLodObject)
				{
					if (searchOptions.useVertexInputLimitLod && sharedMesh.vertexCount > searchOptions.vertexInputLimitLod)
					{
						return null;
					}
				}
				else if (searchOptions.useVertexInputLimit && sharedMesh.vertexCount > searchOptions.vertexInputLimit)
				{
					return null;
				}
				if (useVertexOutputLimit && sharedMesh.vertexCount > vertexOutputLimit)
				{
					return null;
				}
				if (searchOptions.useMaxBoundsFactor && useCells && Methods.GetMax(component.bounds.size) > (float)cellSize * searchOptions.maxBoundsFactor)
				{
					return null;
				}
				if (searchOptions.useNameContains)
				{
					bool flag3 = false;
					for (int k = 0; k < searchOptions.nameContainList.Count; k++)
					{
						if (Methods.Contains(t.name, searchOptions.nameContainList[k]))
						{
							flag3 = true;
							break;
						}
					}
					if (!flag3)
					{
						return null;
					}
				}
			}
			return new CachedGameObject(gameObject, t, component, component2, sharedMesh);
		}

		public void RestoreOriginalRenderersAndLODGroups()
		{
			if (!activeOriginal)
			{
				activeOriginal = true;
				ExecuteHandleObjects(activeOriginal, HandleComponent.Disable, HandleComponent.Disable);
			}
		}

		public void GetOriginalRenderersAndLODGroups()
		{
			if (foundObjects.Count != 0 || foundLodObjects.Count != 0 || foundLodGroups.Count != 0)
			{
				return;
			}
			foundObjects.Clear();
			foundLodObjects.Clear();
			DisabledMeshRenderer[] array = UnityEngine.Object.FindObjectsOfType<DisabledMeshRenderer>();
			foreach (DisabledMeshRenderer disabledMeshRenderer in array)
			{
				if (disabledMeshRenderer.meshCombiner == this)
				{
					foundObjects.Add(disabledMeshRenderer.cachedGO);
				}
			}
			DisabledLodMeshRender[] array2 = UnityEngine.Object.FindObjectsOfType<DisabledLodMeshRender>();
			for (int j = 0; j < array2.Length; j++)
			{
				DisabledLodMeshRender disabledLodMeshRender = array2[j];
				if (array2[j].meshCombiner == this)
				{
					foundLodObjects.Add(disabledLodMeshRender.cachedLodGO);
				}
			}
			DisabledLODGroup[] array3 = UnityEngine.Object.FindObjectsOfType<DisabledLODGroup>();
			foreach (DisabledLODGroup disabledLODGroup in array3)
			{
				if (disabledLODGroup.meshCombiner == this)
				{
					foundLodGroups.Add(disabledLODGroup.lodGroup);
				}
			}
			foundLodGroups.Clear();
		}

		public void SwapCombine()
		{
			if (!combined)
			{
				CombineAll();
			}
			combinedActive = !combinedActive;
			ExecuteHandleObjects(!combinedActive, originalMeshRenderers, originalLODGroups);
		}

		public void ExecuteHandleObjects(bool active, HandleComponent handleOriginalObjects, HandleComponent handleOriginalLodGroups)
		{
			Methods.SetChildrenActive(base.transform, !active);
			if (handleOriginalObjects == HandleComponent.Disable)
			{
				for (int i = 0; i < foundObjects.Count; i++)
				{
					CachedGameObject cachedGameObject = foundObjects[i];
					if (cachedGameObject.mr != null)
					{
						cachedGameObject.mr.enabled = active;
					}
					else
					{
						Methods.ListRemoveAt(foundObjects, i--);
					}
				}
				for (int j = 0; j < foundLodObjects.Count; j++)
				{
					CachedLodGameObject cachedLodGameObject = foundLodObjects[j];
					if (cachedLodGameObject.mr != null)
					{
						cachedLodGameObject.mr.enabled = active;
					}
					else
					{
						Methods.ListRemoveAt(foundLodObjects, j--);
					}
				}
			}
			if (handleOriginalObjects == HandleComponent.Destroy)
			{
				for (int k = 0; k < foundObjects.Count; k++)
				{
					bool flag = false;
					CachedGameObject cachedGameObject2 = foundObjects[k];
					if (cachedGameObject2.mf != null)
					{
						UnityEngine.Object.Destroy(cachedGameObject2.mf);
					}
					else
					{
						flag = true;
					}
					if (cachedGameObject2.mr != null)
					{
						UnityEngine.Object.Destroy(cachedGameObject2.mr);
					}
					else
					{
						flag = true;
					}
					if (flag)
					{
						Methods.ListRemoveAt(foundObjects, k--);
					}
				}
				for (int l = 0; l < foundLodObjects.Count; l++)
				{
					bool flag2 = false;
					CachedGameObject cachedGameObject3 = foundLodObjects[l];
					if (cachedGameObject3.mf != null)
					{
						UnityEngine.Object.Destroy(cachedGameObject3.mf);
					}
					else
					{
						flag2 = true;
					}
					if (cachedGameObject3.mr != null)
					{
						UnityEngine.Object.Destroy(cachedGameObject3.mr);
					}
					else
					{
						flag2 = true;
					}
					if (flag2)
					{
						Methods.ListRemoveAt(foundLodObjects, l--);
					}
				}
			}
			switch (handleOriginalLodGroups)
			{
			case HandleComponent.Disable:
			{
				foreach (LODGroup foundLodGroup in foundLodGroups)
				{
					if (foundLodGroup != null)
					{
						foundLodGroup.enabled = active;
					}
				}
				break;
			}
			case HandleComponent.Destroy:
			{
				foreach (LODGroup foundLodGroup2 in foundLodGroups)
				{
					if (foundLodGroup2 != null)
					{
						UnityEngine.Object.Destroy(foundLodGroup2);
					}
				}
				break;
			}
			}
		}

		private void AddDisabledMeshRenderer(CachedGameObject cachedGO)
		{
			if (!(cachedGO.go == null))
			{
				DisabledMeshRenderer disabledMeshRenderer = cachedGO.go.GetComponent<DisabledMeshRenderer>();
				if (disabledMeshRenderer == null)
				{
					disabledMeshRenderer = cachedGO.go.AddComponent<DisabledMeshRenderer>();
					disabledMeshRenderer.hideFlags = HideFlags.HideInInspector;
				}
				disabledMeshRenderer.meshCombiner = this;
				disabledMeshRenderer.cachedGO = cachedGO;
			}
		}

		private void AddDisabledLodMeshRenderer(CachedLodGameObject cachedLodGO)
		{
			if (!(cachedLodGO.go == null))
			{
				DisabledLodMeshRender disabledLodMeshRender = cachedLodGO.go.GetComponent<DisabledLodMeshRender>();
				if (disabledLodMeshRender == null)
				{
					disabledLodMeshRender = cachedLodGO.go.AddComponent<DisabledLodMeshRender>();
					disabledLodMeshRender.hideFlags = HideFlags.HideInInspector;
				}
				disabledLodMeshRender.meshCombiner = this;
				disabledLodMeshRender.cachedLodGO = cachedLodGO;
			}
		}

		private void RemoveDisabledMeshRenderer(CachedGameObject cachedGO)
		{
			if (!(cachedGO.go == null))
			{
				DisabledMeshRenderer component = cachedGO.go.GetComponent<DisabledMeshRenderer>();
				if (component != null && component.meshCombiner == this)
				{
					UnityEngine.Object.DestroyImmediate(component);
				}
			}
		}

		private void RemoveDisabledLodMeshRenderer(CachedLodGameObject cachedLodGO)
		{
			if (!(cachedLodGO.go == null))
			{
				DisabledLodMeshRender component = cachedLodGO.go.GetComponent<DisabledLodMeshRender>();
				if (component != null && component.meshCombiner == this)
				{
					UnityEngine.Object.DestroyImmediate(component);
				}
			}
		}

		public void MakeMeshesReadableInImportSettings()
		{
		}

		private void OnDrawGizmosSelected()
		{
			if (removeBackFaceTriangles && backFaceTriangleMode == BackFaceTriangleMode.Box)
			{
				Gizmos.DrawWireCube(backFaceBounds.center, backFaceBounds.size);
			}
			if (drawGizmos)
			{
				if (octree != null && octreeContainsObjects)
				{
					octree.Draw(this, true, !searchOptions.useSearchBox);
				}
				if (searchOptions.useSearchBox)
				{
					searchOptions.GetSearchBoxBounds();
					Gizmos.color = Color.green;
					Gizmos.DrawWireCube(searchOptions.searchBoxBounds.center, searchOptions.searchBoxBounds.size);
					Gizmos.color = Color.white;
				}
			}
		}

		private void LogOctreeInfo()
		{
			Console.Log("Cells " + ObjectOctree.MaxCell.maxCellCount + " -> Found Objects: ");
			LodParentHolder[] array = lodParentHolders;
			if (array == null || array.Length == 0)
			{
				return;
			}
			for (int i = 0; i < array.Length; i++)
			{
				LodParentHolder lodParentHolder = array[i];
				if (lodParentHolder.found)
				{
					string empty = string.Empty;
					empty = "LOD Group " + (i + 1) + " |";
					int[] lods = lodParentHolder.lods;
					for (int j = 0; j < lods.Length; j++)
					{
						empty = empty + " " + lods[j] + " |";
					}
					Console.Log(empty);
				}
			}
		}
	}
}
