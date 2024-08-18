using System;
using FluffyUnderware.Curvy.Utils;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Create/Path Line Renderer", ModuleName = "Create Path Line Renderer", Description = "Feeds a Line Renderer with a Path")]
	public class CreatePathLineRenderer : CGModule
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) })]
		public CGModuleInputSlot InPath = new CGModuleInputSlot();

		private LineRenderer mLineRenderer;

		public LineRenderer LineRenderer
		{
			get
			{
				if (mLineRenderer == null)
				{
					mLineRenderer = GetComponent<LineRenderer>();
				}
				return mLineRenderer;
			}
		}

		public override bool IsConfigured
		{
			get
			{
				return base.IsConfigured;
			}
		}

		public override bool IsInitialized
		{
			get
			{
				return base.IsInitialized;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			createLR();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
		}

		public override void Reset()
		{
			base.Reset();
		}

		public override void Refresh()
		{
			base.Refresh();
			CGPath data = InPath.GetData<CGPath>(new CGDataRequestParameter[0]);
			if (data != null)
			{
				LineRenderer.positionCount = data.Position.Length;
				LineRenderer.SetPositions(data.Position);
			}
			else
			{
				LineRenderer.positionCount = 0;
			}
		}

		private void createLR()
		{
			if (LineRenderer == null)
			{
				mLineRenderer = base.gameObject.AddComponent<LineRenderer>();
				mLineRenderer.useWorldSpace = false;
				mLineRenderer.textureMode = LineTextureMode.Tile;
				mLineRenderer.sharedMaterial = CurvyUtility.GetDefaultMaterial();
			}
		}
	}
}
