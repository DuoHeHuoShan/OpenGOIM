using System.Collections.Generic;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace FluffyUnderware.Curvy.Controllers
{
	[RequireComponent(typeof(Text))]
	[AddComponentMenu("Curvy/Controller/UI Text Spline Controller")]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/uitextsplinecontroller")]
	public class UITextSplineController : SplineController, IMeshModifier
	{
		protected class GlyphQuad
		{
			public UIVertex[] V = new UIVertex[4];

			public Rect Rect;

			public Vector3 Center
			{
				get
				{
					return Rect.center;
				}
			}

			public void Load(List<UIVertex> verts, int index)
			{
				V[0] = verts[index];
				V[1] = verts[index + 1];
				V[2] = verts[index + 2];
				V[3] = verts[index + 3];
				calcRect();
			}

			public void LoadTris(List<UIVertex> verts, int index)
			{
				V[0] = verts[index];
				V[1] = verts[index + 1];
				V[2] = verts[index + 2];
				V[3] = verts[index + 4];
				calcRect();
			}

			public void calcRect()
			{
				Rect = new Rect(V[0].position.x, V[2].position.y, V[2].position.x - V[0].position.x, V[0].position.y - V[2].position.y);
			}

			public void Save(List<UIVertex> verts, int index)
			{
				verts[index] = V[0];
				verts[index + 1] = V[1];
				verts[index + 2] = V[2];
				verts[index + 3] = V[3];
			}

			public void Save(VertexHelper vh)
			{
				vh.AddUIVertexQuad(V);
			}

			public void Transpose(Vector3 v)
			{
				for (int i = 0; i < 4; i++)
				{
					V[i].position += v;
				}
			}

			public void Rotate(Quaternion rotation)
			{
				for (int i = 0; i < 4; i++)
				{
					V[i].position = V[i].position.RotateAround(Center, rotation);
				}
			}
		}

		protected class GlyphPlain
		{
			public Vector3[] V = new Vector3[4];

			public Rect Rect;

			public Vector3 Center
			{
				get
				{
					return Rect.center;
				}
			}

			public void Load(ref Vector3[] verts, int index)
			{
				V[0] = verts[index];
				V[1] = verts[index + 1];
				V[2] = verts[index + 2];
				V[3] = verts[index + 3];
				calcRect();
			}

			public void calcRect()
			{
				Rect = new Rect(V[0].x, V[2].y, V[2].x - V[0].x, V[0].y - V[2].y);
			}

			public void Save(ref Vector3[] verts, int index)
			{
				verts[index] = V[0];
				verts[index + 1] = V[1];
				verts[index + 2] = V[2];
				verts[index + 3] = V[3];
			}

			public void Transpose(Vector3 v)
			{
				for (int i = 0; i < 4; i++)
				{
					V[i] += v;
				}
			}

			public void Rotate(Quaternion rotation)
			{
				for (int i = 0; i < 4; i++)
				{
					V[i] = V[i].RotateAround(Center, rotation);
				}
			}
		}

		private Graphic m_Graphic;

		private RectTransform mRect;

		private Text mText;

		protected Text Text
		{
			get
			{
				if (mText == null)
				{
					mText = GetComponent<Text>();
				}
				return mText;
			}
		}

		protected RectTransform Rect
		{
			get
			{
				if (mRect == null)
				{
					mRect = GetComponent<RectTransform>();
				}
				return mRect;
			}
		}

		protected Graphic graphic
		{
			get
			{
				if (m_Graphic == null)
				{
					m_Graphic = GetComponent<Graphic>();
				}
				return m_Graphic;
			}
		}

		public override void Refresh()
		{
			base.Refresh();
			graphic.SetVerticesDirty();
		}

		protected override void OnRefreshSpline(CurvySplineEventArgs e)
		{
			base.OnRefreshSpline(e);
			graphic.SetVerticesDirty();
		}

		public virtual void ModifyVertices(List<UIVertex> verts)
		{
			if (base.enabled && base.gameObject.activeInHierarchy)
			{
				GlyphQuad glyphQuad = new GlyphQuad();
				for (int i = 0; i < Text.text.Length; i++)
				{
					glyphQuad.Load(verts, i * 4);
					float worldUnitDistance = AbsolutePosition + glyphQuad.Rect.center.x;
					float tf = AbsoluteToRelative(worldUnitDistance);
					Vector3 interpolatedSourcePosition = GetInterpolatedSourcePosition(tf);
					Vector3 tangent = GetTangent(tf);
					Vector3 v = interpolatedSourcePosition - Rect.localPosition - glyphQuad.Center;
					glyphQuad.Transpose(new Vector3(0f, glyphQuad.Center.y, 0f));
					glyphQuad.Rotate(Quaternion.AngleAxis(Mathf.Atan2(tangent.x, 0f - tangent.y) * 57.29578f - 90f, Vector3.forward));
					glyphQuad.Transpose(v);
					glyphQuad.Save(verts, i * 4);
				}
			}
		}

		public void ModifyMesh(Mesh verts)
		{
			if (base.enabled && base.gameObject.activeInHierarchy && IsInitialized)
			{
				Vector3[] verts2 = verts.vertices;
				GlyphPlain glyphPlain = new GlyphPlain();
				for (int i = 0; i < Text.text.Length; i++)
				{
					glyphPlain.Load(ref verts2, i * 4);
					float worldUnitDistance = AbsolutePosition + glyphPlain.Rect.center.x;
					float tf = AbsoluteToRelative(worldUnitDistance);
					Vector3 interpolatedSourcePosition = GetInterpolatedSourcePosition(tf);
					Vector3 tangent = GetTangent(tf);
					Vector3 v = interpolatedSourcePosition - Rect.localPosition - glyphPlain.Center;
					glyphPlain.Transpose(new Vector3(0f, glyphPlain.Center.y, 0f));
					glyphPlain.Rotate(Quaternion.AngleAxis(Mathf.Atan2(tangent.x, 0f - tangent.y) * 57.29578f - 90f, Vector3.forward));
					glyphPlain.Transpose(v);
					glyphPlain.Save(ref verts2, i * 4);
				}
				verts.vertices = verts2;
			}
		}

		public void ModifyMesh(VertexHelper vh)
		{
			if (base.enabled && base.gameObject.activeInHierarchy && IsInitialized)
			{
				List<UIVertex> list = new List<UIVertex>();
				GlyphQuad glyphQuad = new GlyphQuad();
				vh.GetUIVertexStream(list);
				vh.Clear();
				for (int i = 0; i < Text.text.Length; i++)
				{
					glyphQuad.LoadTris(list, i * 6);
					float worldUnitDistance = AbsolutePosition + glyphQuad.Rect.center.x;
					float tf = AbsoluteToRelative(worldUnitDistance);
					Vector3 interpolatedSourcePosition = GetInterpolatedSourcePosition(tf);
					Vector3 tangent = GetTangent(tf);
					Vector3 v = interpolatedSourcePosition - Rect.localPosition - glyphQuad.Center;
					glyphQuad.Transpose(new Vector3(0f, glyphQuad.Center.y, 0f));
					glyphQuad.Rotate(Quaternion.AngleAxis(Mathf.Atan2(tangent.x, 0f - tangent.y) * 57.29578f - 90f, Vector3.forward));
					glyphQuad.Transpose(v);
					glyphQuad.Save(vh);
				}
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (graphic != null)
			{
				graphic.SetVerticesDirty();
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (graphic != null)
			{
				graphic.SetVerticesDirty();
			}
		}
	}
}
