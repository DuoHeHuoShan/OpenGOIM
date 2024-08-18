using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[ExecuteInEditMode]
	public class CurvyMetadataBase : MonoBehaviour
	{
		private CurvySplineSegment mCP;

		public CurvySplineSegment ControlPoint
		{
			get
			{
				return mCP;
			}
		}

		public CurvySpline Spline
		{
			get
			{
				return (!mCP) ? null : mCP.Spline;
			}
		}

		protected virtual void Awake()
		{
			mCP = GetComponent<CurvySplineSegment>();
		}

		public T GetPreviousData<T>(bool autoCreate = true, bool segmentsOnly = true, bool useFollowUp = false) where T : MonoBehaviour, ICurvyMetadata
		{
			if ((bool)ControlPoint)
			{
				CurvySplineSegment previousControlPoint = ControlPoint.GetPreviousControlPoint(segmentsOnly, useFollowUp);
				if ((bool)previousControlPoint)
				{
					return previousControlPoint.GetMetadata<T>(autoCreate);
				}
			}
			return (T)null;
		}

		public T GetNextData<T>(bool autoCreate = true, bool segmentsOnly = true, bool useFollowUp = false) where T : MonoBehaviour, ICurvyMetadata
		{
			if ((bool)ControlPoint)
			{
				CurvySplineSegment nextControlPoint = ControlPoint.GetNextControlPoint(segmentsOnly, useFollowUp);
				if ((bool)nextControlPoint)
				{
					return nextControlPoint.GetMetadata<T>(autoCreate);
				}
			}
			return (T)null;
		}

		public void SetDirty()
		{
			if ((bool)ControlPoint)
			{
				ControlPoint.SetDirty();
			}
		}
	}
}
