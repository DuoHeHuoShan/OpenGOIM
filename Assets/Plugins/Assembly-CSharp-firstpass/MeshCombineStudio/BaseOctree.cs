using UnityEngine;

namespace MeshCombineStudio
{
	public class BaseOctree
	{
		public class Cell
		{
			public Cell mainParent;

			public Cell parent;

			public Cell[] cells;

			public bool[] cellsUsed;

			public Bounds bounds;

			public int cellIndex;

			public int cellCount;

			public int level;

			public int maxLevels;

			public Cell()
			{
			}

			public Cell(Vector3 position, Vector3 size, int maxLevels)
			{
				bounds = new Bounds(position, size);
				this.maxLevels = maxLevels;
			}

			public Cell(Cell parent, int cellIndex, Bounds bounds)
			{
				if (parent != null)
				{
					maxLevels = parent.maxLevels;
					mainParent = parent.mainParent;
					level = parent.level + 1;
				}
				this.parent = parent;
				this.cellIndex = cellIndex;
				this.bounds = bounds;
			}

			public void SetCell(Cell parent, int cellIndex, Bounds bounds)
			{
				if (parent != null)
				{
					maxLevels = parent.maxLevels;
					mainParent = parent.mainParent;
					level = parent.level + 1;
				}
				this.parent = parent;
				this.cellIndex = cellIndex;
				this.bounds = bounds;
			}

			protected int AddCell<T, U>(ref T[] cells, Vector3 position, out bool maxCellCreated) where T : Cell, new() where U : Cell, new()
			{
				Vector3 vector = position - this.bounds.min;
				int num = (int)(vector.x / this.bounds.extents.x);
				int num2 = (int)(vector.y / this.bounds.extents.y);
				int num3 = (int)(vector.z / this.bounds.extents.z);
				int num4 = num + num2 * 4 + num3 * 2;
				if (cells == null)
				{
					cells = new T[8];
				}
				if (cellsUsed == null)
				{
					cellsUsed = new bool[8];
				}
				if (!cellsUsed[num4])
				{
					Bounds bounds = new Bounds(new Vector3(this.bounds.min.x + this.bounds.extents.x * ((float)num + 0.5f), this.bounds.min.y + this.bounds.extents.y * ((float)num2 + 0.5f), this.bounds.min.z + this.bounds.extents.z * ((float)num3 + 0.5f)), this.bounds.extents);
					if (level == maxLevels - 1)
					{
						cells[num4] = new U() as T;
						cells[num4].SetCell(this, num4, bounds);
						maxCellCreated = true;
					}
					else
					{
						maxCellCreated = false;
						cells[num4] = new T();
						cells[num4].SetCell(this, num4, bounds);
					}
					cellsUsed[num4] = true;
					cellCount++;
				}
				else
				{
					maxCellCreated = false;
				}
				return num4;
			}

			public void RemoveCell(int index)
			{
				cells[index] = null;
				cellsUsed[index] = false;
				cellCount--;
				if (cellCount == 0 && parent != null)
				{
					parent.RemoveCell(cellIndex);
				}
			}

			public bool InsideBounds(Vector3 position)
			{
				position -= bounds.min;
				if (position.x >= bounds.size.x || position.y >= bounds.size.y || position.z >= bounds.size.z || position.x <= 0f || position.y <= 0f || position.z <= 0f)
				{
					return false;
				}
				return true;
			}

			public void Reset(ref Cell[] cells)
			{
				cells = null;
				cellsUsed = null;
			}
		}
	}
}
