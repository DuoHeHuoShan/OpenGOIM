using UnityEngine;

namespace FluffyUnderware.DevTools
{
	[ExecuteInEditMode]
	public class DuplicateEditorMesh : MonoBehaviour
	{
		private MeshFilter mFilter;

		public MeshFilter Filter
		{
			get
			{
				if (mFilter == null)
				{
					mFilter = GetComponent<MeshFilter>();
				}
				return mFilter;
			}
		}

		protected virtual void Awake()
		{
			if (Application.isPlaying)
			{
				return;
			}
			MeshFilter filter = Filter;
			if (!filter || !(filter.sharedMesh != null))
			{
				return;
			}
			DuplicateEditorMesh[] array = Object.FindObjectsOfType<DuplicateEditorMesh>();
			DuplicateEditorMesh[] array2 = array;
			foreach (DuplicateEditorMesh duplicateEditorMesh in array2)
			{
				if (duplicateEditorMesh != this)
				{
					MeshFilter filter2 = duplicateEditorMesh.Filter;
					if ((bool)filter2 && filter2.sharedMesh == filter.sharedMesh)
					{
						Mesh mesh = new Mesh();
						mesh.name = filter2.sharedMesh.name;
						filter.mesh = mesh;
					}
				}
			}
		}
	}
}
