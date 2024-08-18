using System;
using UnityEngine;

namespace FluffyUnderware.DevTools
{
	[Serializable]
	public class PoolSettings
	{
		[SerializeField]
		private bool m_Prewarm;

		[SerializeField]
		private bool m_AutoCreate = true;

		[SerializeField]
		private bool m_AutoEnableDisable = true;

		[Positive]
		[SerializeField]
		private int m_MinItems;

		[Positive]
		[SerializeField]
		private int m_Threshold;

		[Positive]
		[SerializeField]
		private float m_Speed = 1f;

		public bool Debug;

		public bool Prewarm
		{
			get
			{
				return m_Prewarm;
			}
			set
			{
				if (m_Prewarm != value)
				{
					m_Prewarm = value;
				}
			}
		}

		public bool AutoCreate
		{
			get
			{
				return m_AutoCreate;
			}
			set
			{
				if (m_AutoCreate != value)
				{
					m_AutoCreate = value;
				}
			}
		}

		public bool AutoEnableDisable
		{
			get
			{
				return m_AutoEnableDisable;
			}
			set
			{
				if (m_AutoEnableDisable != value)
				{
					m_AutoEnableDisable = value;
				}
			}
		}

		public int MinItems
		{
			get
			{
				return m_MinItems;
			}
			set
			{
				int num = Mathf.Max(0, value);
				if (m_MinItems != num)
				{
					m_MinItems = num;
				}
			}
		}

		public int Threshold
		{
			get
			{
				return m_Threshold;
			}
			set
			{
				int num = Mathf.Max(MinItems, value);
				if (m_Threshold != num)
				{
					m_Threshold = num;
				}
			}
		}

		public float Speed
		{
			get
			{
				return m_Speed;
			}
			set
			{
				float num = Mathf.Max(0f, value);
				if (m_Speed != num)
				{
					m_Speed = num;
				}
			}
		}

		public PoolSettings()
		{
		}

		public PoolSettings(PoolSettings src)
		{
			Prewarm = src.Prewarm;
			AutoCreate = src.AutoCreate;
			MinItems = src.MinItems;
			Threshold = src.Threshold;
			Speed = src.Speed;
			Debug = src.Debug;
		}

		public void OnValidate()
		{
			MinItems = m_MinItems;
			Threshold = m_Threshold;
			Speed = m_Speed;
		}
	}
}
