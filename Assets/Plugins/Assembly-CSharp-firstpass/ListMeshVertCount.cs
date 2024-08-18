using UnityEngine;

[ExecuteInEditMode]
public class ListMeshVertCount : MonoBehaviour
{
	public bool includeInActive;

	public bool listVertCount;

	private void Update()
	{
		if (listVertCount)
		{
			listVertCount = false;
			ListVertCount();
		}
	}

	private void ListVertCount()
	{
		MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>(includeInActive);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Mesh sharedMesh = componentsInChildren[i].sharedMesh;
			if (!(sharedMesh == null))
			{
				num += sharedMesh.vertexCount;
				num2 += sharedMesh.triangles.Length;
			}
		}
		Debug.Log(base.gameObject.name + " Vertices " + num + "  Triangles " + num2);
	}
}
