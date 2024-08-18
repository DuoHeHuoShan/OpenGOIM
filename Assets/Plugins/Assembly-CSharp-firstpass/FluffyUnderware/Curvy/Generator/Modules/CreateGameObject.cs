using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Create/GameObject", ModuleName = "Create GameObject")]
	[HelpURL("http://www.fluffyunderware.com/curvy/doclink/210/cgcreategameobject")]
	public class CreateGameObject : CGModule
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGGameObject) }, Array = true, Name = "GameObject")]
		public CGModuleInputSlot InGameObjectArray = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGSpots) }, Name = "Spots")]
		public CGModuleInputSlot InSpots = new CGModuleInputSlot();

		[SerializeField]
		[CGResourceCollectionManager("GameObject", ShowCount = true)]
		private CGGameObjectResourceCollection m_Resources = new CGGameObjectResourceCollection();

		[Tab("General")]
		[SerializeField]
		private bool m_MakeStatic;

		[SerializeField]
		[Layer("", "")]
		private int m_Layer;

		public int Layer
		{
			get
			{
				return m_Layer;
			}
			set
			{
				int num = Mathf.Clamp(value, 0, 32);
				if (m_Layer != num)
				{
					m_Layer = num;
				}
				base.Dirty = true;
			}
		}

		public bool MakeStatic
		{
			get
			{
				return m_MakeStatic;
			}
			set
			{
				if (m_MakeStatic != value)
				{
					m_MakeStatic = value;
				}
				base.Dirty = true;
			}
		}

		public CGGameObjectResourceCollection GameObjects
		{
			get
			{
				return m_Resources;
			}
		}

		public int GameObjectCount
		{
			get
			{
				return GameObjects.Count;
			}
		}

		public override void Reset()
		{
			base.Reset();
			MakeStatic = false;
			Layer = 0;
			Clear();
		}

		protected override void OnDestroy()
		{
			if (!base.Generator.Destroying)
			{
				DeleteAllPrefabPools();
			}
			base.OnDestroy();
		}

		public override void OnTemplateCreated()
		{
			Clear();
		}

		public void Clear()
		{
			for (int i = 0; i < GameObjects.Count; i++)
			{
				DeleteManagedResource("GameObject", GameObjects.Items[i], GameObjects.PoolNames[i]);
			}
			GameObjects.Items.Clear();
			GameObjects.PoolNames.Clear();
		}

		public override void OnStateChange()
		{
			base.OnStateChange();
			if (!IsConfigured)
			{
				Clear();
			}
		}

		public override void Refresh()
		{
			base.Refresh();
			List<CGGameObject> allData = InGameObjectArray.GetAllData<CGGameObject>(new CGDataRequestParameter[0]);
			CGSpots data = InSpots.GetData<CGSpots>(new CGDataRequestParameter[0]);
			Clear();
			List<IPool> allPrefabPools = GetAllPrefabPools();
			HashSet<string> hashSet = new HashSet<string>();
			if (allData.Count > 0 && data.Count > 0)
			{
				for (int i = 0; i < data.Count; i++)
				{
					CGSpot cGSpot = data.Points[i];
					int index = cGSpot.Index;
					if (index >= 0 && index < allData.Count && allData[index].Object != null)
					{
						string identifier = GetPrefabPool(allData[index].Object).Identifier;
						hashSet.Add(identifier);
						Transform transform = (Transform)AddManagedResource("GameObject", identifier, i);
						transform.gameObject.isStatic = MakeStatic;
						transform.gameObject.layer = Layer;
						transform.localPosition = cGSpot.Position;
						transform.localRotation = cGSpot.Rotation;
						transform.localScale = new Vector3(transform.localScale.x * cGSpot.Scale.x * allData[index].Scale.x, transform.localScale.y * cGSpot.Scale.y * allData[index].Scale.y, transform.localScale.z * cGSpot.Scale.z * allData[index].Scale.z);
						if (allData[index].Matrix != Matrix4x4.identity)
						{
							transform.Translate(allData[index].Translate);
							transform.Rotate(allData[index].Rotate);
						}
						GameObjects.Items.Add(transform);
						GameObjects.PoolNames.Add(identifier);
					}
				}
			}
			foreach (IPool item in allPrefabPools)
			{
				if (!hashSet.Contains(item.Identifier))
				{
					base.Generator.PoolManager.DeletePool(item);
				}
			}
		}
	}
}
