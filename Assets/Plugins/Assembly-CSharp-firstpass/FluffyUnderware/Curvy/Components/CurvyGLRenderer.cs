using System.Collections.Generic;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Components
{
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/curvyglrenderer")]
	[AddComponentMenu("Curvy/Misc/Curvy GL Renderer")]
	public class CurvyGLRenderer : MonoBehaviour
	{
		[ArrayEx(ShowAdd = false, Draggable = false)]
		public List<GLSlotData> Splines = new List<GLSlotData>();

		private Material lineMaterial;

		private void CreateLineMaterial()
		{
			if (!lineMaterial)
			{
				lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
				lineMaterial.hideFlags = HideFlags.HideAndDontSave;
				lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
			}
		}

		private void OnPostRender()
		{
			sanitize();
			CreateLineMaterial();
			for (int num = Splines.Count - 1; num >= 0; num--)
			{
				Splines[num].Spline.OnRefresh.AddListenerOnce(OnSplineRefresh);
				if (Splines[num].VertexData.Count == 0)
				{
					Splines[num].GetVertexData();
				}
				Splines[num].Render(lineMaterial);
			}
		}

		private void sanitize()
		{
			for (int num = Splines.Count - 1; num >= 0; num--)
			{
				if (Splines[num] == null || Splines[num].Spline == null)
				{
					Splines.RemoveAt(num);
				}
			}
		}

		private void OnSplineRefresh(CurvySplineEventArgs e)
		{
			GLSlotData slot = getSlot((CurvySplineBase)e.Sender);
			if (slot == null)
			{
				((CurvySplineBase)e.Sender).OnRefresh.RemoveListener(OnSplineRefresh);
			}
			else
			{
				slot.VertexData.Clear();
			}
		}

		private GLSlotData getSlot(CurvySplineBase spline)
		{
			if ((bool)spline)
			{
				foreach (GLSlotData spline2 in Splines)
				{
					if (spline2.Spline == spline)
					{
						return spline2;
					}
				}
			}
			return null;
		}

		public void Add(CurvySplineBase spline)
		{
			if (spline != null)
			{
				Splines.Add(new GLSlotData
				{
					Spline = spline
				});
			}
		}

		public void Remove(CurvySplineBase spline)
		{
			for (int num = Splines.Count - 1; num >= 0; num--)
			{
				if (Splines[num].Spline == spline)
				{
					Splines.RemoveAt(num);
				}
			}
		}
	}
}
