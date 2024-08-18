using System;
using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.Curvy.Components
{
	[Serializable]
	public class GLSlotData
	{
		[SerializeField]
		public CurvySplineBase Spline;

		public Color LineColor = CurvyGlobalManager.DefaultGizmoColor;

		public List<Vector3[]> VertexData = new List<Vector3[]>();

		public void GetVertexData()
		{
			VertexData.Clear();
			List<CurvySpline> list = new List<CurvySpline>();
			if (Spline is CurvySplineGroup)
			{
				list.AddRange(((CurvySplineGroup)Spline).Splines);
			}
			else
			{
				list.Add((CurvySpline)Spline);
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsInitialized)
				{
					VertexData.Add(list[i].GetApproximation(Space.World));
				}
			}
		}

		public void Render(Material mat)
		{
			for (int i = 0; i < VertexData.Count; i++)
			{
				if (VertexData[i].Length > 0)
				{
					mat.SetPass(0);
					GL.Begin(1);
					GL.Color(LineColor);
					for (int j = 1; j < VertexData[i].Length; j++)
					{
						GL.Vertex(VertexData[i][j - 1]);
						GL.Vertex(VertexData[i][j]);
					}
					GL.End();
				}
			}
		}
	}
}
