using System.Collections.Generic;
using FluffyUnderware.Curvy;
using UnityEngine;

public class ShadowRaycaster : MonoBehaviour
{
	private SkinnedMeshRenderer[] skrns;

	private MeshRenderer[] mrns;

	public CurvySpline spline;

	private Vector3[] points;

	private MaterialPropertyBlock propBlock;

	private List<Vector4> vectorOut;

	private void Start()
	{
		skrns = GetComponentsInChildren<SkinnedMeshRenderer>();
		mrns = GetComponentsInChildren<MeshRenderer>();
		points = new Vector3[spline.ControlPointCount];
		for (int i = 0; i < spline.ControlPointCount; i++)
		{
			points[i] = spline.ControlPoints[i].position;
		}
		propBlock = new MaterialPropertyBlock();
		vectorOut = new List<Vector4>(4);
		for (int j = 0; j < 4; j++)
		{
			vectorOut.Add(new Vector4(0f, 0f, 0f, 0f));
		}
	}

	private void LateUpdate()
	{
		SkinnedMeshRenderer[] array = skrns;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			float num5 = 999999f;
			for (int j = 0; j < points.Length; j++)
			{
				float sqrMagnitude = (skinnedMeshRenderer.bounds.center - points[j]).sqrMagnitude;
				if (sqrMagnitude < num5)
				{
					num5 = sqrMagnitude;
					num4 = num3;
					num3 = num2;
					num2 = num;
					num = j;
				}
			}
			skinnedMeshRenderer.GetPropertyBlock(propBlock);
			vectorOut[0] = points[num];
			vectorOut[1] = points[num2];
			vectorOut[2] = points[num3];
			vectorOut[3] = points[num4];
			propBlock.SetVectorArray("_Nearest", vectorOut);
			skinnedMeshRenderer.SetPropertyBlock(propBlock);
		}
		MeshRenderer[] array2 = mrns;
		foreach (MeshRenderer meshRenderer in array2)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			float num5 = 999999f;
			for (int l = 0; l < points.Length; l++)
			{
				float sqrMagnitude = (meshRenderer.bounds.center - points[l]).sqrMagnitude;
				if (sqrMagnitude < num5)
				{
					num5 = sqrMagnitude;
					num4 = num3;
					num3 = num2;
					num2 = num;
					num = l;
				}
			}
			Debug.DrawLine(meshRenderer.bounds.center, points[num], Color.cyan);
			Debug.DrawLine(meshRenderer.bounds.center, points[num2], Color.blue);
			meshRenderer.GetPropertyBlock(propBlock);
			vectorOut[0] = points[num];
			vectorOut[1] = points[num2];
			vectorOut[2] = points[num3];
			vectorOut[3] = points[num4];
			propBlock.SetVectorArray("_Nearest", vectorOut);
			meshRenderer.SetPropertyBlock(propBlock);
		}
	}
}
