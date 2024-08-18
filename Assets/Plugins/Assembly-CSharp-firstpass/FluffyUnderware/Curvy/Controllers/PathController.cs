using System.Collections;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Controllers
{
	[AddComponentMenu("Curvy/Controller/CG Path Controller", 7)]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/pathcontroller")]
	public class PathController : CurvyController
	{
		[Section("General", true, false, 100, Sort = 0)]
		[SerializeField]
		[CGDataReferenceSelector(typeof(CGPath), Label = "Path/Slot")]
		private CGDataReference m_Path = new CGDataReference();

		private float mKeepDistanceAt;

		public CGDataReference Path
		{
			get
			{
				return m_Path;
			}
			set
			{
				if (m_Path == value)
				{
					return;
				}
				if (m_Path != null)
				{
					UnbindEvents();
				}
				m_Path = value;
				if (m_Path != null)
				{
					if (!mIsPrepared)
					{
						Prepare();
					}
					else
					{
						BindEvents();
					}
				}
			}
		}

		public CGPath PathData
		{
			get
			{
				return (!Path.HasValue) ? null : Path.GetData<CGPath>();
			}
		}

		public override bool IsConfigured
		{
			get
			{
				return Path != null && !Path.IsEmpty;
			}
		}

		public override bool DependenciesInitialized
		{
			get
			{
				return IsConfigured && Path.HasValue;
			}
		}

		public override float Length
		{
			get
			{
				return (PathData == null) ? 0f : PathData.Length;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			BindEvents();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			UnbindEvents();
		}

		private IEnumerator Start()
		{
			if (IsConfigured)
			{
				while (!DependenciesInitialized)
				{
					yield return 0;
				}
				Prepare();
			}
		}

		protected override void Reset()
		{
			base.Reset();
			Path = null;
		}

		public override void Prepare()
		{
			BindEvents();
			base.Prepare();
		}

		protected override float RelativeToAbsolute(float relativeDistance)
		{
			return PathData.FToDistance(relativeDistance);
		}

		protected override float AbsoluteToRelative(float worldUnitDistance)
		{
			return PathData.DistanceToF(worldUnitDistance);
		}

		protected override Vector3 GetInterpolatedSourcePosition(float tf)
		{
			return (base.Space != 0) ? PathData.InterpolatePosition(tf) : Path.Module.Generator.transform.TransformPoint(PathData.InterpolatePosition(tf));
		}

		protected override void GetInterpolatedSourcePosition(float tf, out Vector3 position, out Vector3 tangent, out Vector3 up)
		{
			PathData.Interpolate(tf, out position, out tangent, out up);
			if (base.Space == Space.World)
			{
				position = Path.Module.Generator.transform.TransformPoint(position);
				tangent = Path.Module.Generator.transform.TransformDirection(tangent);
				up = Path.Module.Generator.transform.TransformDirection(up);
			}
		}

		protected override Vector3 GetTangent(float tf)
		{
			return (base.Space != 0) ? PathData.InterpolateDirection(tf) : Path.Module.Generator.transform.TransformDirection(PathData.InterpolateDirection(tf));
		}

		protected override Vector3 GetOrientation(float tf)
		{
			return (base.Space != 0) ? PathData.InterpolateUp(tf) : Path.Module.Generator.transform.TransformDirection(PathData.InterpolateUp(tf));
		}

		protected override void Advance(ref float virtualPosition, ref int direction, MoveModeEnum mode, float absSpeed, CurvyClamping clamping)
		{
			if (mode == MoveModeEnum.Relative)
			{
				PathData.Move(ref virtualPosition, ref direction, absSpeed, clamping);
			}
			else
			{
				PathData.MoveBy(ref virtualPosition, ref direction, absSpeed, clamping);
			}
		}

		protected virtual void OnRefreshPath(CurvyCGEventArgs e)
		{
			if (Path == null || e.Module != Path.Module)
			{
				e.Module.OnRefresh.RemoveListener(OnRefreshPath);
			}
			else if (base.Active)
			{
				if (Application.isPlaying)
				{
					if (mKeepDistanceAt > 0f && IsInitialized && base.IsPlaying)
					{
						AbsolutePosition = mKeepDistanceAt;
						mKeepDistanceAt = 0f;
					}
					if (Speed == 0f)
					{
						Refresh();
					}
				}
				else if (IsInitialized)
				{
					Prepare();
				}
			}
			else
			{
				UnbindEvents();
			}
		}

		protected override void BindEvents()
		{
			base.BindEvents();
			if (Path != null && Path.Module != null)
			{
				UnbindEvents();
				Path.Module.OnRefresh.AddListener(OnRefreshPath);
			}
		}

		protected override void UnbindEvents()
		{
			if (Path != null && Path.Module != null)
			{
				Path.Module.OnRefresh.RemoveListener(OnRefreshPath);
			}
		}
	}
}
