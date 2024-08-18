using System.Collections.Generic;
using FluffyUnderware.Curvy.Controllers;
using UnityEngine;

namespace FluffyUnderware.Curvy.Components
{
	[AddComponentMenu("Curvy/Misc/Curvy Default Event Handler")]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/defaulteventhandler")]
	public class CurvyDefaultEventHandler : MonoBehaviour
	{
		public static void DebugLogStatic(CurvySplineEventArgs e)
		{
			Debug.Log(string.Format("Sender/Spline/Data: {0}/{1}/{2}", e.Sender, e.Spline, e.Data));
		}

		public static void DebugLogStatic(CurvySplineMoveEventArgs e)
		{
			Debug.Log(string.Format("Segment/TF/Direction: {0}/{1}/{2}", e.ControlPoint, e.TF, e.Direction));
		}

		public static void UseFollowUpStatic(CurvySplineMoveEventArgs e)
		{
			if (e.Sender is SplineController && (bool)e.ControlPoint.FollowUp)
			{
				CurvySplineSegment controlPoint = e.ControlPoint;
				e.Follow(e.ControlPoint.FollowUp, e.ControlPoint.FollowUpHeading);
				SplineController splineController = (SplineController)e.Sender;
				splineController.Spline = e.Spline;
				splineController.RelativePosition = e.TF;
				if ((bool)controlPoint.FollowUp && controlPoint.FollowUp.FollowUp != controlPoint)
				{
					splineController.OnControlPointReached.Invoke(e);
				}
			}
		}

		public void DebugLog(CurvySplineEventArgs e)
		{
			DebugLogStatic(e);
		}

		public static void DebugLog(CurvySplineMoveEventArgs e)
		{
			DebugLogStatic(e);
		}

		public void UseFollowUp(CurvySplineMoveEventArgs e)
		{
			UseFollowUpStatic(e);
		}

		public void UseRandomConnectionStatic(CurvySplineMoveEventArgs e)
		{
			if (!(e.Sender is SplineController) || !e.ControlPoint.Connection)
			{
				return;
			}
			CurvySplineSegment controlPoint = e.ControlPoint;
			List<CurvySplineSegment> list = e.ControlPoint.Connection.OtherControlPoints(controlPoint);
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (e.AngleTo(list[num]) > 90f)
				{
					list.RemoveAt(num);
				}
			}
			int num2 = Random.Range(-1, list.Count);
			if (num2 < 0)
			{
				if ((bool)controlPoint.FollowUp)
				{
					e.Follow(controlPoint.FollowUp, controlPoint.FollowUpHeading);
				}
			}
			else
			{
				e.Follow(list[num2]);
			}
			SplineController splineController = (SplineController)e.Sender;
			splineController.Spline = e.Spline;
			splineController.RelativePosition = e.TF;
		}

		public void UseRandomConnection(CurvySplineMoveEventArgs e)
		{
			UseRandomConnectionStatic(e);
		}
	}
}
