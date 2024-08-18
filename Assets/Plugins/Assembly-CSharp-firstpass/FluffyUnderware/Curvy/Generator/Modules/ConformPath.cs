using System;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Modifier/Conform Path", ModuleName = "Conform Path", Description = "Projects a path")]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/cgconformpath")]
	public class ConformPath : CGModule, IOnRequestPath, IOnRequestProcessing
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, Name = "Path", ModifiesData = true)]
		public CGModuleInputSlot InPath = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGPath))]
		public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();

		[SerializeField]
		[VectorEx("", "")]
		private Vector3 m_Direction = new Vector3(0f, -1f, 0f);

		[SerializeField]
		private float m_MaxDistance = 100f;

		[SerializeField]
		private float m_Offset;

		[SerializeField]
		private bool m_Warp;

		[SerializeField]
		private LayerMask m_LayerMask;

		public Vector3 Direction
		{
			get
			{
				return m_Direction;
			}
			set
			{
				if (m_Direction != value)
				{
					m_Direction = value;
				}
				base.Dirty = true;
			}
		}

		public float MaxDistance
		{
			get
			{
				return m_MaxDistance;
			}
			set
			{
				if (m_MaxDistance != value)
				{
					m_MaxDistance = value;
				}
				base.Dirty = true;
			}
		}

		public float Offset
		{
			get
			{
				return m_Offset;
			}
			set
			{
				if (m_Offset != value)
				{
					m_Offset = value;
				}
				base.Dirty = true;
			}
		}

		public bool Warp
		{
			get
			{
				return m_Warp;
			}
			set
			{
				if (m_Warp != value)
				{
					m_Warp = value;
				}
				base.Dirty = true;
			}
		}

		public LayerMask LayerMask
		{
			get
			{
				return m_LayerMask;
			}
			set
			{
				if ((int)m_LayerMask != (int)value)
				{
					m_LayerMask = value;
				}
				base.Dirty = true;
			}
		}

		public float PathLength
		{
			get
			{
				if (OutPath.HasData)
				{
					return OutPath.GetData<CGPath>().Length;
				}
				return (!IsConfigured) ? 0f : InPath.SourceSlot().OnRequestPathModule.PathLength;
			}
		}

		public bool PathIsClosed
		{
			get
			{
				return IsConfigured && InPath.SourceSlot().OnRequestPathModule.PathIsClosed;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.LabelWidth = 80f;
		}

		public override void Reset()
		{
			base.Reset();
			Direction = new Vector3(0f, -1f, 0f);
			MaxDistance = 100f;
			Offset = 0f;
			Warp = false;
			LayerMask = 0;
		}

		public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
		{
			CGDataRequestRasterization requestParameter = GetRequestParameter<CGDataRequestRasterization>(ref requests);
			if (!requestParameter)
			{
				return null;
			}
			CGPath data = InPath.GetData<CGPath>(requests);
			return new CGData[1] { Conform(base.Generator.transform, data, LayerMask, Direction, Offset, MaxDistance, Warp) };
		}

		public static CGPath Conform(Transform refTransform, CGPath path, LayerMask layers, Vector3 dir, float offset, float rayLength, bool warp)
		{
			if (dir != Vector3.zero && rayLength > 0f)
			{
				if (warp)
				{
					float num = float.MaxValue;
					for (int i = 0; i < path.Count; i++)
					{
						Ray ray = new Ray(refTransform.TransformPoint(path.Position[i]), dir);
						RaycastHit hitInfo;
						if (Physics.Raycast(ray, out hitInfo, rayLength, layers) && hitInfo.distance < num)
						{
							num = hitInfo.distance;
						}
					}
					if (num != float.MaxValue)
					{
						for (int j = 0; j < path.Count; j++)
						{
							path.Position[j] += dir * (num + offset);
						}
					}
					path.Touch();
				}
				else
				{
					int count = path.Count;
					for (int k = 0; k < count; k++)
					{
						Ray ray2 = new Ray(refTransform.TransformPoint(path.Position[k]), dir);
						RaycastHit hitInfo2;
						if (Physics.Raycast(ray2, out hitInfo2, rayLength, layers))
						{
							path.Position[k] += dir * (hitInfo2.distance + offset);
						}
					}
				}
			}
			return path;
		}
	}
}
