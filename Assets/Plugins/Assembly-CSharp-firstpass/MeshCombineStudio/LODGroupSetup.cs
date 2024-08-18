using System.Collections.Generic;
using UnityEngine;

namespace MeshCombineStudio
{
	public class LODGroupSetup : MonoBehaviour
	{
		public MeshCombiner meshCombiner;

		public LODGroup lodGroup;

		public int lodGroupParentIndex;

		public int lodCount;

		private LODGroup[] lodGroups;

		public void Init(MeshCombiner meshCombiner, int lodGroupParentIndex)
		{
			this.meshCombiner = meshCombiner;
			this.lodGroupParentIndex = lodGroupParentIndex;
			lodCount = lodGroupParentIndex + 1;
			if (lodGroup == null)
			{
				lodGroup = base.gameObject.AddComponent<LODGroup>();
			}
			GetSetup();
		}

		private void GetSetup()
		{
			LOD[] array = new LOD[lodGroupParentIndex + 1];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = default(LOD);
				array[i].screenRelativeTransitionHeight = meshCombiner.lodGroupsSettings[lodGroupParentIndex].lodSettings[i].screenRelativeTransitionHeight;
			}
			lodGroup.SetLODs(array);
		}

		public void ApplySetup()
		{
			LOD[] lODs = lodGroup.GetLODs();
			if (lodGroups == null)
			{
				lodGroups = GetComponentsInChildren<LODGroup>();
			}
			if (lODs.Length != lodCount)
			{
				return;
			}
			bool flag = false;
			if (lodGroupParentIndex == 0)
			{
				if (lODs[0].screenRelativeTransitionHeight != 0f)
				{
					if (lodGroups == null || lodGroups.Length == 1)
					{
						AddLODGroupsToChildren();
					}
				}
				else
				{
					if (lodGroup != null && lodGroups.Length != 1)
					{
						RemoveLODGroupFromChildren();
					}
					flag = true;
				}
			}
			if (meshCombiner != null)
			{
				for (int i = 0; i < lODs.Length; i++)
				{
					meshCombiner.lodGroupsSettings[lodGroupParentIndex].lodSettings[i].screenRelativeTransitionHeight = lODs[i].screenRelativeTransitionHeight;
				}
			}
			if (flag)
			{
				return;
			}
			for (int j = 0; j < lodGroups.Length; j++)
			{
				LOD[] lODs2 = lodGroups[j].GetLODs();
				for (int k = 0; k < lODs2.Length; k++)
				{
					lODs2[k].screenRelativeTransitionHeight = lODs[k].screenRelativeTransitionHeight;
				}
				lodGroups[j].SetLODs(lODs2);
			}
			if (meshCombiner != null)
			{
				lodGroup.size = meshCombiner.cellSize;
			}
		}

		public void AddLODGroupsToChildren()
		{
			Transform transform = base.transform;
			List<LODGroup> list = new List<LODGroup>();
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				Debug.Log(child.name);
				LODGroup lODGroup = child.GetComponent<LODGroup>();
				if (lODGroup == null)
				{
					lODGroup = child.gameObject.AddComponent<LODGroup>();
					lODGroup.SetLODs(new LOD[1]
					{
						new LOD(0f, child.GetComponentsInChildren<MeshRenderer>())
					});
				}
				list.Add(lODGroup);
			}
			lodGroups = list.ToArray();
		}

		public void RemoveLODGroupFromChildren()
		{
			Transform transform = base.transform;
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				LODGroup component = child.GetComponent<LODGroup>();
				if (component != null)
				{
					Object.DestroyImmediate(component);
				}
			}
			lodGroups = null;
		}
	}
}
