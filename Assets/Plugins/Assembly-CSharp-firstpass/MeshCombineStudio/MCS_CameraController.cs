using UnityEngine;

namespace MeshCombineStudio
{
	public class MCS_CameraController : MonoBehaviour
	{
		public float speed = 10f;

		public float mouseMoveSpeed = 1f;

		public float shiftMulti = 3f;

		public float controlMulti = 0.5f;

		private Vector3 oldMousePosition;

		private GameObject cameraMountGO;

		private GameObject cameraChildGO;

		private Transform cameraMountT;

		private Transform cameraChildT;

		private Transform t;

		private void Awake()
		{
			t = base.transform;
			CreateParents();
		}

		private void CreateParents()
		{
			cameraMountGO = new GameObject("CameraMount");
			cameraChildGO = new GameObject("CameraChild");
			cameraMountT = cameraMountGO.transform;
			cameraChildT = cameraChildGO.transform;
			cameraChildT.SetParent(cameraMountT);
			cameraMountT.position = t.position;
			cameraMountT.rotation = t.rotation;
			t.SetParent(cameraChildT);
		}

		private void Update()
		{
			Vector3 vector = (Input.mousePosition - oldMousePosition) * mouseMoveSpeed * (Time.deltaTime * 60f);
			if (Input.GetMouseButton(1))
			{
				cameraMountT.Rotate(0f, vector.x, 0f, Space.Self);
				cameraChildT.Rotate(0f - vector.y, 0f, 0f, Space.Self);
			}
			oldMousePosition = Input.mousePosition;
			Vector3 zero = Vector3.zero;
			if (Input.GetKey(KeyCode.W))
			{
				zero.z = speed;
			}
			else if (Input.GetKey(KeyCode.S))
			{
				zero.z = 0f - speed;
			}
			else if (Input.GetKey(KeyCode.A))
			{
				zero.x = 0f - speed;
			}
			else if (Input.GetKey(KeyCode.D))
			{
				zero.x = speed;
			}
			else if (Input.GetKey(KeyCode.Q))
			{
				zero.y = 0f - speed;
			}
			else if (Input.GetKey(KeyCode.E))
			{
				zero.y = speed;
			}
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				zero *= shiftMulti;
			}
			else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				zero *= controlMulti;
			}
			zero *= Time.deltaTime * 60f;
			Quaternion identity = Quaternion.identity;
			identity.eulerAngles = new Vector3(cameraChildT.eulerAngles.x, cameraMountT.eulerAngles.y, 0f);
			zero = identity * zero;
			cameraMountT.position += zero;
		}
	}
}
