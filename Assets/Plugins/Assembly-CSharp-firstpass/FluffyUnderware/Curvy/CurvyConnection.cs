using System.Collections.Generic;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[ExecuteInEditMode]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/curvyconnection")]
	public class CurvyConnection : MonoBehaviour
	{
		[SerializeField]
		[Hide]
		private List<CurvySplineSegment> m_ControlPoints = new List<CurvySplineSegment>();

		private TTransform mTTransform;

		public List<CurvySplineSegment> ControlPoints
		{
			get
			{
				return m_ControlPoints;
			}
		}

		public int Count
		{
			get
			{
				return m_ControlPoints.Count;
			}
		}

		public CurvySplineSegment this[int idx]
		{
			get
			{
				return m_ControlPoints[idx];
			}
		}

		public TTransform TTransform
		{
			get
			{
				if (!mTTransform)
				{
					mTTransform = new TTransform(base.transform);
				}
				return mTTransform;
			}
		}

		private void OnDestroy()
		{
			if (1 == 0)
			{
				return;
			}
			foreach (CurvySplineSegment controlPoint in ControlPoints)
			{
				controlPoint.Connection = null;
				controlPoint.Disconnect();
			}
			ControlPoints.Clear();
		}

		public void Update()
		{
			bool flag = TTransform.position != base.transform.position;
			bool flag2 = TTransform.rotation != base.transform.rotation;
			if (flag || flag2)
			{
				SynchronizeINTERNAL(base.transform);
			}
		}

		public static CurvyConnection Create(params CurvySplineSegment[] controlPoints)
		{
			CurvyConnection curvyConnection = DTSingleton<CurvyGlobalManager>.Instance.AddChildGameObject<CurvyConnection>("Connection");
			if (!curvyConnection)
			{
				return null;
			}
			if (controlPoints.Length > 0)
			{
				curvyConnection.transform.position = controlPoints[0].transform.position;
				curvyConnection.TTransform.FromTransform(curvyConnection.transform);
				curvyConnection.AddControlPoints(controlPoints);
			}
			return curvyConnection;
		}

		public void AddControlPoints(params CurvySplineSegment[] controlPoints)
		{
			foreach (CurvySplineSegment controlPoint in controlPoints)
			{
				addControlPoint(controlPoint);
			}
			AutoSetFollowUp();
		}

		public void AutoSetFollowUp()
		{
			if (Count == 2 && ControlPoints[0].position == ControlPoints[1].position && ControlPoints[0].ConnectionSyncPosition && ControlPoints[1].ConnectionSyncPosition)
			{
				if (ControlPoints[0].FollowUp == null && ControlPoints[0].CanHaveFollowUp)
				{
					ControlPoints[0].SetFollowUp(ControlPoints[1]);
				}
				if (ControlPoints[1].FollowUp == null && ControlPoints[1].CanHaveFollowUp)
				{
					ControlPoints[1].SetFollowUp(ControlPoints[0]);
				}
			}
		}

		public void RemoveControlPoint(CurvySplineSegment controlPoint, bool destroySelfIfEmpty = true)
		{
			controlPoint.Connection = null;
			ControlPoints.Remove(controlPoint);
			if (ControlPoints.Count == 0 && destroySelfIfEmpty)
			{
				Delete();
			}
		}

		public void Delete()
		{
			if (Application.isPlaying)
			{
				Object.Destroy(base.gameObject);
			}
		}

		public List<CurvySplineSegment> OtherControlPoints(CurvySplineSegment source)
		{
			List<CurvySplineSegment> list = new List<CurvySplineSegment>(ControlPoints);
			list.Remove(source);
			return list;
		}

		private void addControlPoint(CurvySplineSegment controlPoint)
		{
			if (!controlPoint.Connection)
			{
				m_ControlPoints.Add(controlPoint);
				controlPoint.Connection = this;
			}
		}

		private void synchronize()
		{
			for (int i = 0; i < ControlPoints.Count; i++)
			{
				if (ControlPoints[i].ConnectionSyncPosition)
				{
					ControlPoints[i].transform.position = base.transform.position;
				}
				if (ControlPoints[i].ConnectionSyncRotation)
				{
					ControlPoints[i].transform.rotation = base.transform.rotation;
				}
				ControlPoints[i].RefreshTransform(false);
			}
			TTransform.FromTransform(base.transform);
		}

		public void SynchronizeINTERNAL(Transform tform)
		{
			if (tform != base.transform)
			{
				base.transform.position = tform.position;
				base.transform.rotation = tform.rotation;
			}
			synchronize();
		}
	}
}
