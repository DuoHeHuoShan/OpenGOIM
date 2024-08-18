using UnityEngine;

namespace MeshCombineStudio
{
	public class DirectDraw : MonoBehaviour
	{
		private MeshRenderer[] mrs;

		private Mesh[] meshes;

		private Material[] mats;

		private Vector3[] positions;

		private Quaternion[] rotations;

		private void Awake()
		{
			mrs = GetComponentsInChildren<MeshRenderer>(false);
			SetMeshRenderersEnabled(false);
			meshes = new Mesh[mrs.Length];
			mats = new Material[mrs.Length];
			positions = new Vector3[mrs.Length];
			rotations = new Quaternion[mrs.Length];
			for (int i = 0; i < mrs.Length; i++)
			{
				MeshFilter component = mrs[i].GetComponent<MeshFilter>();
				meshes[i] = component.sharedMesh;
				mats[i] = mrs[i].sharedMaterial;
				positions[i] = mrs[i].transform.position;
				rotations[i] = mrs[i].transform.rotation;
			}
		}

		private void SetMeshRenderersEnabled(bool enabled)
		{
			for (int i = 0; i < mrs.Length; i++)
			{
				mrs[i].enabled = enabled;
			}
		}

		private void Update()
		{
			for (int i = 0; i < mrs.Length; i++)
			{
				Graphics.DrawMesh(meshes[i], positions[i], rotations[i], mats[i], 0);
			}
		}
	}
}
