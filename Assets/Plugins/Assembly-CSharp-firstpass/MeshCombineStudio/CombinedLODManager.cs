using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeshCombineStudio
{
	public class CombinedLODManager : MonoBehaviour
	{
		public enum LodMode
		{
			Automatic = 0,
			DebugLod = 1
		}

		public enum LodDistanceMode
		{
			Automatic = 0,
			Manual = 1
		}

		[Serializable]
		public class LOD
		{
			public Transform searchParent;

			public Sphere3 sphere = new Sphere3();

			public LOD()
			{
			}

			public LOD(Transform searchParent)
			{
				this.searchParent = searchParent;
			}
		}

		public class Cell : BaseOctree.Cell
		{
			public new Cell[] cells;

			private AABB3 box;

			public Cell()
			{
			}

			public Cell(Vector3 position, Vector3 size, int maxLevels)
				: base(position, size, maxLevels)
			{
			}

			public void AddMeshRenderer(MeshRenderer mr, Vector3 position, int lodLevel, int lodLevels)
			{
				if (InsideBounds(position))
				{
					AddMeshRendererInternal(mr, position, lodLevel, lodLevels);
				}
			}

			private void AddMeshRendererInternal(MeshRenderer mr, Vector3 position, int lodLevel, int lodLevels)
			{
				if (level == maxLevels)
				{
					MaxCell maxCell = (MaxCell)this;
					if (maxCell.mrList == null)
					{
						maxCell.mrList = new List<MeshRenderer>[lodLevels];
					}
					List<MeshRenderer>[] mrList = maxCell.mrList;
					if (mrList[lodLevel] == null)
					{
						mrList[lodLevel] = new List<MeshRenderer>();
					}
					mrList[lodLevel].Add(mr);
					maxCell.currentLod = -1;
				}
				else
				{
					bool maxCellCreated;
					int num = AddCell<Cell, MaxCell>(ref cells, position, out maxCellCreated);
					cells[num].box = new AABB3(cells[num].bounds.min, cells[num].bounds.max);
					cells[num].AddMeshRendererInternal(mr, position, lodLevel, lodLevels);
				}
			}

			public void AutoLodInternal(LOD[] lods, float lodCulledDistance)
			{
				if (level == maxLevels)
				{
					MaxCell maxCell = (MaxCell)this;
					if (lodCulledDistance != -1f)
					{
						float sqrMagnitude = (bounds.center - lods[0].sphere.center).sqrMagnitude;
						if (sqrMagnitude > lodCulledDistance * lodCulledDistance)
						{
							if (maxCell.currentLod == -1)
							{
								return;
							}
							for (int i = 0; i < lods.Length; i++)
							{
								for (int j = 0; j < maxCell.mrList[i].Count; j++)
								{
									maxCell.mrList[i][j].enabled = false;
								}
							}
							maxCell.currentLod = -1;
							return;
						}
					}
					for (int k = 0; k < lods.Length; k++)
					{
						if (k < lods.Length - 1 && !Mathw.IntersectAABB3Sphere3(box, lods[k].sphere))
						{
							continue;
						}
						if (maxCell.currentLod == k)
						{
							break;
						}
						for (int l = 0; l < lods.Length; l++)
						{
							bool enabled = l == k;
							for (int m = 0; m < maxCell.mrList[l].Count; m++)
							{
								maxCell.mrList[l][m].enabled = enabled;
							}
						}
						maxCell.currentLod = k;
						break;
					}
					return;
				}
				for (int n = 0; n < 8; n++)
				{
					if (cellsUsed[n])
					{
						cells[n].AutoLodInternal(lods, lodCulledDistance);
					}
				}
			}

			public void LodInternal(LOD[] lods, int lodLevel)
			{
				if (level == maxLevels)
				{
					MaxCell maxCell = (MaxCell)this;
					if (maxCell.currentLod == lodLevel)
					{
						return;
					}
					for (int i = 0; i < lods.Length; i++)
					{
						bool enabled = i == lodLevel;
						for (int j = 0; j < maxCell.mrList[i].Count; j++)
						{
							maxCell.mrList[i][j].enabled = enabled;
						}
					}
					maxCell.currentLod = lodLevel;
					return;
				}
				for (int k = 0; k < 8; k++)
				{
					if (cellsUsed[k])
					{
						cells[k].LodInternal(lods, lodLevel);
					}
				}
			}

			public void DrawGizmos(LOD[] lods)
			{
				for (int i = 0; i < lods.Length; i++)
				{
					switch (i)
					{
					case 0:
						Gizmos.color = Color.red;
						break;
					case 1:
						Gizmos.color = Color.green;
						break;
					case 2:
						Gizmos.color = Color.yellow;
						break;
					case 3:
						Gizmos.color = Color.blue;
						break;
					}
					Gizmos.DrawWireSphere(lods[i].sphere.center, lods[i].sphere.radius);
				}
				DrawGizmosInternal();
			}

			public void DrawGizmosInternal()
			{
				if (level == maxLevels)
				{
					MaxCell maxCell = (MaxCell)this;
					if (maxCell.currentLod == 0)
					{
						Gizmos.color = Color.red;
					}
					else if (maxCell.currentLod == 1)
					{
						Gizmos.color = Color.green;
					}
					else if (maxCell.currentLod == 2)
					{
						Gizmos.color = Color.yellow;
					}
					else if (maxCell.currentLod == 3)
					{
						Gizmos.color = Color.blue;
					}
					Gizmos.DrawWireCube(bounds.center, bounds.size - new Vector3(0.25f, 0.25f, 0.25f));
					Gizmos.color = Color.white;
					return;
				}
				for (int i = 0; i < 8; i++)
				{
					if (cellsUsed[i])
					{
						cells[i].DrawGizmosInternal();
					}
				}
			}
		}

		public class MaxCell : Cell
		{
			public List<MeshRenderer>[] mrList;

			public int currentLod;
		}

		public bool drawGizmos = true;

		public LOD[] lods;

		public float[] distances;

		public LodDistanceMode lodDistanceMode;

		public LodMode lodMode;

		public int showLod;

		public bool lodCulled;

		public float lodCullDistance = 500f;

		public Vector3 octreeCenter = Vector3.zero;

		public Vector3 octreeSize = new Vector3(256f, 256f, 256f);

		public int maxLevels = 4;

		public bool search = true;

		private Cell octree;

		private Transform cameraMainT;

		private void Awake()
		{
			cameraMainT = Camera.main.transform;
		}

		private void InitOctree()
		{
			octree = new Cell(octreeCenter, octreeSize, maxLevels);
		}

		private void Start()
		{
			if (search)
			{
				search = false;
				InitOctree();
				Search();
			}
		}

		private void Update()
		{
			if (octree.cellsUsed != null)
			{
				Lod(lodMode);
			}
		}

		public void UpdateLods(MeshCombiner meshCombiner, int lodAmount)
		{
			if (lods != null && lods.Length == lodAmount)
			{
				return;
			}
			lods = new LOD[lodAmount];
			float[] array = new float[lodAmount];
			for (int i = 0; i < lods.Length; i++)
			{
				lods[i] = new LOD();
				if (lodDistanceMode == LodDistanceMode.Automatic)
				{
					array[i] = meshCombiner.cellSize * i;
				}
				else if (distances != null && i < distances.Length)
				{
					array[i] = distances[i];
				}
			}
			distances = array;
		}

		public void UpdateDistances(MeshCombiner meshCombiner)
		{
			if (lodDistanceMode == LodDistanceMode.Automatic)
			{
				for (int i = 0; i < distances.Length; i++)
				{
					distances[i] = meshCombiner.cellSize * i;
				}
			}
		}

		public void Search()
		{
			for (int i = 0; i < lods.Length; i++)
			{
				lods[i].searchParent.gameObject.SetActive(true);
				MeshRenderer[] componentsInChildren = lods[i].searchParent.GetComponentsInChildren<MeshRenderer>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					octree.AddMeshRenderer(componentsInChildren[j], componentsInChildren[j].transform.position, i, lods.Length);
				}
			}
		}

		public void ResetOctree()
		{
			if (octree == null)
			{
				return;
			}
			octree.cells = null;
			octree.cellsUsed = null;
			for (int i = 0; i < lods.Length; i++)
			{
				if (lods[i].searchParent != null)
				{
					UnityEngine.Object.Destroy(lods[i].searchParent.gameObject);
				}
			}
		}

		public void Lod(LodMode lodMode)
		{
			Vector3 position = cameraMainT.position;
			for (int i = 0; i < lods.Length - 1; i++)
			{
				lods[i].sphere.center = position;
				lods[i].sphere.radius = distances[i + 1];
			}
			if (lodMode == LodMode.Automatic)
			{
				octree.AutoLodInternal(lods, (!lodCulled) ? (-1f) : lodCullDistance);
			}
			else
			{
				octree.LodInternal(lods, showLod);
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (drawGizmos && octree != null && octree.cells != null)
			{
				octree.DrawGizmos(lods);
			}
		}
	}
}
