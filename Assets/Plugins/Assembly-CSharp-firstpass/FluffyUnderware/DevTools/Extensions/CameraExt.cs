using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class CameraExt
	{
		private static Plane[] camPlanes;

		private static Vector3 camPos;

		private static Vector3 camForward;

		private static float fov;

		private static float screenW;

		private static float screenH;

		public static bool BoundsInView(this Camera c, Bounds bounds)
		{
			if (camPos != c.transform.position || camForward != c.transform.forward || screenW != (float)Screen.width || screenH != (float)Screen.height || fov != c.fieldOfView)
			{
				camPos = c.transform.position;
				camForward = c.transform.forward;
				screenW = Screen.width;
				screenH = Screen.height;
				fov = c.fieldOfView;
				camPlanes = GeometryUtility.CalculateFrustumPlanes(c);
			}
			return GeometryUtility.TestPlanesAABB(camPlanes, bounds);
		}

		public static bool BoundsPartiallyInView(this Camera c, Bounds bounds)
		{
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(c);
			Vector3 zero = Vector3.zero;
			Vector3 center = bounds.center;
			Vector3 extents = bounds.extents;
			zero.Set(center.x - extents.x, center.y + extents.y, center.z - extents.z);
			if (GeometryUtility.TestPlanesAABB(planes, new Bounds(zero, new Vector3(0.1f, 0.1f, 0.1f))))
			{
				return true;
			}
			zero.Set(center.x + extents.x, center.y + extents.y, center.z - extents.z);
			if (GeometryUtility.TestPlanesAABB(planes, new Bounds(zero, new Vector3(0.1f, 0.1f, 0.1f))))
			{
				return true;
			}
			zero.Set(center.x - extents.x, center.y - extents.y, center.z - extents.z);
			if (GeometryUtility.TestPlanesAABB(planes, new Bounds(zero, new Vector3(0.1f, 0.1f, 0.1f))))
			{
				return true;
			}
			zero.Set(center.x + extents.x, center.y - extents.y, center.z - extents.z);
			if (GeometryUtility.TestPlanesAABB(planes, new Bounds(zero, new Vector3(0.1f, 0.1f, 0.1f))))
			{
				return true;
			}
			zero.Set(center.x - extents.x, center.y + extents.y, center.z + extents.z);
			if (GeometryUtility.TestPlanesAABB(planes, new Bounds(zero, new Vector3(0.1f, 0.1f, 0.1f))))
			{
				return true;
			}
			zero.Set(center.x + extents.x, center.y + extents.y, center.z + extents.z);
			if (GeometryUtility.TestPlanesAABB(planes, new Bounds(zero, new Vector3(0.1f, 0.1f, 0.1f))))
			{
				return true;
			}
			zero.Set(center.x - extents.x, center.y - extents.y, center.z + extents.z);
			if (GeometryUtility.TestPlanesAABB(planes, new Bounds(zero, new Vector3(0.1f, 0.1f, 0.1f))))
			{
				return true;
			}
			zero.Set(center.x + extents.x, center.y - extents.y, center.z + extents.z);
			if (GeometryUtility.TestPlanesAABB(planes, new Bounds(zero, new Vector3(0.1f, 0.1f, 0.1f))))
			{
				return true;
			}
			return false;
		}
	}
}
