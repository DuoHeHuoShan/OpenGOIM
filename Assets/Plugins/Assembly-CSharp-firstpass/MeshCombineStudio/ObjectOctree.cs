using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshCombineStudio
{
	public class ObjectOctree
	{
		public class LODParent
		{
			public GameObject cellGO;

			public Transform cellT;

			public LODGroup lodGroup;

			public LODLevel[] lodLevels;

			public bool hasChanged;

			public int jobsPending;

			public LODParent(int lodCount)
			{
				lodLevels = new LODLevel[lodCount];
				for (int i = 0; i < lodLevels.Length; i++)
				{
					lodLevels[i] = new LODLevel();
				}
			}

			public void AssignLODGroup(MeshCombiner meshCombiner)
			{
				LOD[] array = new LOD[lodLevels.Length];
				int num = array.Length - 1;
				for (int i = 0; i < lodLevels.Length; i++)
				{
					LODLevel lODLevel = lodLevels[i];
					array[i] = new LOD(meshCombiner.lodGroupsSettings[num].lodSettings[i].screenRelativeTransitionHeight, lODLevel.newMeshRenderers.ToArray());
				}
				lodGroup.SetLODs(array);
				lodGroup.size = meshCombiner.cellSize;
			}

			public void ApplyChanges(MeshCombiner meshCombiner)
			{
				for (int i = 0; i < lodLevels.Length; i++)
				{
					lodLevels[i].ApplyChanges(meshCombiner);
				}
				hasChanged = false;
			}
		}

		public class LODLevel
		{
			public List<CachedGameObject> cachedGOs = new List<CachedGameObject>();

			public List<MeshObjectsHolder> meshObjectsHolders;

			public List<MeshObjectsHolder> changedMeshObjectsHolders;

			public List<MeshRenderer> newMeshRenderers = new List<MeshRenderer>();

			public int vertCount;

			public int objectCount;

			public int GetSortMeshIndex(Material mat, bool shadowCastingModeTwoSided, int lightmapIndex)
			{
				int instanceID = mat.GetInstanceID();
				for (int i = 0; i < meshObjectsHolders.Count; i++)
				{
					MeshObjectsHolder meshObjectsHolder = meshObjectsHolders[i];
					if (!(meshObjectsHolder.mat == null) && meshObjectsHolder.mat.GetInstanceID() == instanceID && meshObjectsHolder.shadowCastingModeTwoSided == shadowCastingModeTwoSided && meshObjectsHolder.lightmapIndex == lightmapIndex)
					{
						return i;
					}
				}
				return -1;
			}

			public void ApplyChanges(MeshCombiner meshCombiner)
			{
				for (int i = 0; i < changedMeshObjectsHolders.Count; i++)
				{
					MeshObjectsHolder meshObjectsHolder = changedMeshObjectsHolders[i];
					meshObjectsHolder.hasChanged = false;
				}
				changedMeshObjectsHolders.Clear();
			}
		}

		public class MaxCell : Cell
		{
			public static int maxCellCount;

			public LODParent[] lodParents;

			public List<LODParent> changedLodParents;

			public bool hasChanged;

			public void ApplyChanges(MeshCombiner meshCombiner)
			{
				for (int i = 0; i < changedLodParents.Count; i++)
				{
					changedLodParents[i].ApplyChanges(meshCombiner);
				}
				changedLodParents.Clear();
				hasChanged = false;
			}
		}

		public class Cell : BaseOctree.Cell
		{
			public new Cell[] cells;

			public Cell()
			{
			}

			public Cell(Vector3 position, Vector3 size, int maxLevels)
				: base(position, size, maxLevels)
			{
			}

			public CachedGameObject AddObject(MeshCombiner meshCombiner, CachedGameObject cachedGO, int lodParentIndex, int lodLevel, bool isChangeMode = false)
			{
				Vector3 position = ((meshCombiner.searchOptions.objectCenter != MeshCombiner.ObjectCenter.TransformPosition) ? cachedGO.mr.bounds.center : cachedGO.t.position);
				if (InsideBounds(position))
				{
					AddObjectInternal(meshCombiner, cachedGO, position, lodParentIndex, lodLevel, isChangeMode);
					return cachedGO;
				}
				return null;
			}

			private void AddObjectInternal(MeshCombiner meshCombiner, CachedGameObject cachedGO, Vector3 position, int lodParentIndex, int lodLevel, bool isChangeMode)
			{
				if (level == maxLevels)
				{
					MaxCell maxCell = (MaxCell)this;
					if (maxCell.lodParents == null)
					{
						maxCell.lodParents = new LODParent[10];
					}
					if (maxCell.lodParents[lodParentIndex] == null)
					{
						maxCell.lodParents[lodParentIndex] = new LODParent(lodParentIndex + 1);
					}
					LODParent lODParent = maxCell.lodParents[lodParentIndex];
					LODLevel lODLevel = lODParent.lodLevels[lodLevel];
					lODLevel.cachedGOs.Add(cachedGO);
					if (isChangeMode && SortObject(meshCombiner, lODLevel, cachedGO))
					{
						if (!maxCell.hasChanged)
						{
							maxCell.hasChanged = true;
							if (meshCombiner.changedCells == null)
							{
								meshCombiner.changedCells = new List<MaxCell>();
							}
							meshCombiner.changedCells.Add(maxCell);
						}
						if (!lODParent.hasChanged)
						{
							lODParent.hasChanged = true;
							maxCell.changedLodParents.Add(lODParent);
						}
					}
					lODLevel.objectCount++;
					lODLevel.vertCount += cachedGO.mesh.vertexCount;
				}
				else
				{
					bool maxCellCreated;
					int num = AddCell<Cell, MaxCell>(ref cells, position, out maxCellCreated);
					if (maxCellCreated)
					{
						MaxCell.maxCellCount++;
					}
					cells[num].AddObjectInternal(meshCombiner, cachedGO, position, lodParentIndex, lodLevel, isChangeMode);
				}
			}

			public void SortObjects(MeshCombiner meshCombiner)
			{
				if (level == maxLevels)
				{
					MaxCell maxCell = (MaxCell)this;
					LODParent[] lodParents = maxCell.lodParents;
					foreach (LODParent lODParent in lodParents)
					{
						if (lODParent == null)
						{
							continue;
						}
						for (int j = 0; j < lODParent.lodLevels.Length; j++)
						{
							LODLevel lODLevel = lODParent.lodLevels[j];
							if (lODLevel == null || lODLevel.cachedGOs.Count == 0)
							{
								return;
							}
							for (int k = 0; k < lODLevel.cachedGOs.Count; k++)
							{
								CachedGameObject cachedGO = lODLevel.cachedGOs[k];
								if (!SortObject(meshCombiner, lODLevel, cachedGO))
								{
									Methods.ListRemoveAt(lODLevel.cachedGOs, k--);
								}
							}
						}
					}
					return;
				}
				for (int l = 0; l < 8; l++)
				{
					if (cellsUsed[l])
					{
						cells[l].SortObjects(meshCombiner);
					}
				}
			}

			public bool SortObject(MeshCombiner meshCombiner, LODLevel lod, CachedGameObject cachedGO, bool isChangeMode = false)
			{
				if (cachedGO.mr == null)
				{
					return false;
				}
				if (lod.meshObjectsHolders == null)
				{
					lod.meshObjectsHolders = new List<MeshObjectsHolder>();
				}
				Material[] sharedMaterials = cachedGO.mr.sharedMaterials;
				int num = Mathf.Min(cachedGO.mesh.subMeshCount, sharedMaterials.Length);
				for (int i = 0; i < num; i++)
				{
					Material material = sharedMaterials[i];
					if (!(material == null))
					{
						bool shadowCastingModeTwoSided = cachedGO.mr.shadowCastingMode == ShadowCastingMode.TwoSided;
						int lightmapIndex = ((!meshCombiner.validCopyBakedLighting) ? (-1) : cachedGO.mr.lightmapIndex);
						int sortMeshIndex = lod.GetSortMeshIndex(material, shadowCastingModeTwoSided, lightmapIndex);
						MeshObjectsHolder meshObjectsHolder;
						if (sortMeshIndex == -1)
						{
							meshObjectsHolder = new MeshObjectsHolder(cachedGO, material, i, shadowCastingModeTwoSided, lightmapIndex);
							lod.meshObjectsHolders.Add(meshObjectsHolder);
						}
						else
						{
							meshObjectsHolder = lod.meshObjectsHolders[sortMeshIndex];
							meshObjectsHolder.meshObjects.Add(new MeshObject(cachedGO, i));
						}
						if (isChangeMode && !meshObjectsHolder.hasChanged)
						{
							meshObjectsHolder.hasChanged = true;
							lod.changedMeshObjectsHolders.Add(meshObjectsHolder);
						}
					}
				}
				return true;
			}

			public void CombineMeshes(MeshCombiner meshCombiner, int lodParentIndex)
			{
				if (level == maxLevels)
				{
					MaxCell maxCell = (MaxCell)this;
					LODParent lODParent = maxCell.lodParents[lodParentIndex];
					if (lODParent == null)
					{
						return;
					}
					lODParent.cellGO = new GameObject((!meshCombiner.useCells) ? "Combined Objects" : ("Cell " + bounds.center));
					lODParent.cellT = lODParent.cellGO.transform;
					lODParent.cellT.parent = meshCombiner.lodParentHolders[lodParentIndex].t;
					lODParent.lodGroup = lODParent.cellGO.AddComponent<LODGroup>();
					LODGroup lodGroup = lODParent.lodGroup;
					Vector3 center = bounds.center;
					lODParent.cellT.position = center;
					lodGroup.localReferencePoint = center;
					LODLevel[] lodLevels = lODParent.lodLevels;
					for (int i = 0; i < lodLevels.Length; i++)
					{
						LODLevel lODLevel = lODParent.lodLevels[i];
						if (lODLevel == null || lODLevel.meshObjectsHolders == null)
						{
							break;
						}
						Transform transform = null;
						if (lodParentIndex > 0)
						{
							GameObject gameObject = new GameObject("LOD" + i);
							transform = gameObject.transform;
							transform.parent = lODParent.cellT;
						}
						for (int j = 0; j < lODLevel.meshObjectsHolders.Count; j++)
						{
							MeshObjectsHolder meshObjectsHolder = lODLevel.meshObjectsHolders[j];
							meshObjectsHolder.lodParent = lODParent;
							meshObjectsHolder.lodLevel = i;
							MeshCombineJobManager.instance.AddJob(meshCombiner, meshObjectsHolder, (lodParentIndex <= 0) ? lODParent.cellT : transform, bounds.center);
						}
					}
					return;
				}
				for (int k = 0; k < 8; k++)
				{
					if (cellsUsed[k])
					{
						cells[k].CombineMeshes(meshCombiner, lodParentIndex);
					}
				}
			}

			public void Draw(MeshCombiner meshCombiner, bool onlyMaxLevel, bool drawLevel0)
			{
				if (!onlyMaxLevel || level == maxLevels || (drawLevel0 && level == 0))
				{
					Gizmos.DrawWireCube(base.bounds.center, base.bounds.size);
					if (level == maxLevels && meshCombiner.drawMeshBounds)
					{
						MaxCell maxCell = (MaxCell)this;
						LODParent[] lodParents = maxCell.lodParents;
						for (int i = 0; i < lodParents.Length; i++)
						{
							if (lodParents[i] == null)
							{
								continue;
							}
							LODLevel[] lodLevels = lodParents[i].lodLevels;
							Gizmos.color = ((!meshCombiner.activeOriginal) ? Color.green : Color.blue);
							for (int j = 0; j < lodLevels.Length; j++)
							{
								for (int k = 0; k < lodLevels[j].cachedGOs.Count; k++)
								{
									if (!(lodLevels[j].cachedGOs[k].mr == null))
									{
										Bounds bounds = lodLevels[j].cachedGOs[k].mr.bounds;
										Gizmos.DrawWireCube(bounds.center, bounds.size);
									}
								}
							}
							Gizmos.color = Color.white;
						}
						return;
					}
				}
				if (cells == null || cellsUsed == null)
				{
					return;
				}
				for (int l = 0; l < 8; l++)
				{
					if (cellsUsed[l])
					{
						cells[l].Draw(meshCombiner, onlyMaxLevel, drawLevel0);
					}
				}
			}
		}
	}
}
