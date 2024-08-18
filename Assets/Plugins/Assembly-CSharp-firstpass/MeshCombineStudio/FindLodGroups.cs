using UnityEngine;

namespace MeshCombineStudio
{
	[ExecuteInEditMode]
	public class FindLodGroups : MonoBehaviour
	{
		public bool find;

		private void Start()
		{
			FindLods();
		}

		private void Update()
		{
			if (find)
			{
				find = false;
				FindLods();
			}
		}

		private void FindLods()
		{
			LODGroup[] componentsInChildren = GetComponentsInChildren<LODGroup>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Debug.Log(componentsInChildren[i].name);
			}
			Debug.Log("---------------------------------------------");
			Debug.Log("LODGroups found " + componentsInChildren.Length);
			Debug.Log("---------------------------------------------");
		}
	}
}
