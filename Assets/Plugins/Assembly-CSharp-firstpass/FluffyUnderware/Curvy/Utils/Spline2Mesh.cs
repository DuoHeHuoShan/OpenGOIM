using System;
using System.Collections.Generic;
using FluffyUnderware.Curvy.ThirdParty.LibTessDotNet;
using UnityEngine;

namespace FluffyUnderware.Curvy.Utils
{
	public class Spline2Mesh
	{
		public List<SplinePolyLine> Lines = new List<SplinePolyLine>();

		public WindingRule Winding;

		public Vector2 UVTiling = Vector2.one;

		public Vector2 UVOffset = Vector2.zero;

		public bool SuppressUVMapping;

		public bool UV2;

		public string MeshName = string.Empty;

		public bool VertexLineOnly;

		private bool mUseMeshBounds;

		private Vector2 mNewBounds;

		private Tess mTess;

		private Mesh mMesh;

		[Obsolete("Use Lines instead!")]
		public SplinePolyLine Outline;

		[Obsolete("Use Lines instead!")]
		public List<SplinePolyLine> Holes = new List<SplinePolyLine>();

		public string Error { get; private set; }

		public bool Apply(out Mesh result)
		{
			mTess = null;
			mMesh = null;
			Error = string.Empty;
			if (triangulate() && buildMesh())
			{
				if (!SuppressUVMapping && !VertexLineOnly)
				{
					uvmap();
				}
				result = mMesh;
				return true;
			}
			if ((bool)mMesh)
			{
				mMesh.RecalculateNormals();
			}
			result = mMesh;
			return false;
		}

		public void SetBounds(bool useMeshBounds, Vector2 newSize)
		{
			mUseMeshBounds = useMeshBounds;
			mNewBounds = newSize;
		}

		private bool triangulate()
		{
			if (Lines.Count == 0)
			{
				Error = "Missing splines to triangulate";
				return false;
			}
			if (VertexLineOnly)
			{
				return true;
			}
			mTess = new Tess();
			for (int i = 0; i < Lines.Count; i++)
			{
				if (Lines[i].Spline == null)
				{
					Error = "Missing Spline";
					return false;
				}
				if (!polyLineIsValid(Lines[i]))
				{
					Error = Lines[i].Spline.name + ": Angle must be >0";
					return false;
				}
				Vector3[] vertices = Lines[i].GetVertices();
				if (vertices.Length < 3)
				{
					Error = Lines[i].Spline.name + ": At least 3 Vertices needed!";
					return false;
				}
				mTess.AddContour(UnityLibTessUtility.ToContourVertex(vertices, true), Lines[i].Orientation);
			}
			try
			{
				mTess.Tessellate(Winding, ElementType.Polygons, 3);
				return true;
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}
			return false;
		}

		private bool polyLineIsValid(SplinePolyLine pl)
		{
			return (pl != null && pl.VertexMode == SplinePolyLine.VertexCalculation.ByApproximation) || !Mathf.Approximately(0f, pl.Angle);
		}

		private bool buildMesh()
		{
			mMesh = new Mesh();
			mMesh.name = MeshName;
			if (VertexLineOnly && Lines.Count > 0 && Lines[0] != null)
			{
				Vector3[] vertices = Lines[0].GetVertices();
				for (int i = 0; i < vertices.Length; i++)
				{
					vertices[i].z = 0f;
				}
				mMesh.vertices = vertices;
			}
			else
			{
				mMesh.vertices = UnityLibTessUtility.FromContourVertex(mTess.Vertices);
				mMesh.triangles = mTess.Elements;
			}
			mMesh.RecalculateBounds();
			return true;
		}

		private void uvmap()
		{
			Bounds bounds = mMesh.bounds;
			Vector2 vector = bounds.size;
			if (!mUseMeshBounds)
			{
				vector = mNewBounds;
			}
			Vector3[] vertices = mMesh.vertices;
			Vector2[] array = new Vector2[vertices.Length];
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < vertices.Length; i++)
			{
				float num3 = UVOffset.x + (vertices[i].x - bounds.min.x) / vector.x;
				float num4 = UVOffset.y + (vertices[i].y - bounds.min.y) / vector.y;
				num3 *= UVTiling.x;
				num4 *= UVTiling.y;
				num = Mathf.Max(num3, num);
				num2 = Mathf.Max(num4, num2);
				array[i] = new Vector2(num3, num4);
			}
			mMesh.uv = array;
			Vector2[] array2 = new Vector2[0];
			if (UV2)
			{
				array2 = new Vector2[array.Length];
				for (int j = 0; j < vertices.Length; j++)
				{
					array2[j] = new Vector2(array[j].x / num, array[j].y / num2);
				}
			}
			mMesh.uv2 = array2;
			mMesh.RecalculateNormals();
		}
	}
}
