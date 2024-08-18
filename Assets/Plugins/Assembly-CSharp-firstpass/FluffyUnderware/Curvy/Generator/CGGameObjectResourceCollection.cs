using System;
using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGGameObjectResourceCollection : ICGResourceCollection
	{
		public List<Transform> Items = new List<Transform>();

		public List<string> PoolNames = new List<string>();

		public int Count
		{
			get
			{
				return Items.Count;
			}
		}

		public Component[] ItemsArray
		{
			get
			{
				return Items.ToArray();
			}
		}
	}
}
