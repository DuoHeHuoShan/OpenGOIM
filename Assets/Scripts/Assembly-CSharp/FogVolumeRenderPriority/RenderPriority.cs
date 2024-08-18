using UnityEngine;

namespace FogVolumeRenderPriority
{
	[ExecuteInEditMode]
	public class RenderPriority : MonoBehaviour
	{
		public int DrawOrder;

		public bool UpdateRealTime;

		private void OnEnable()
		{
			base.gameObject.GetComponent<Renderer>().sortingOrder = DrawOrder;
		}

		private void Update()
		{
			if (UpdateRealTime)
			{
				base.gameObject.GetComponent<Renderer>().sortingOrder = DrawOrder;
			}
		}
	}
}
