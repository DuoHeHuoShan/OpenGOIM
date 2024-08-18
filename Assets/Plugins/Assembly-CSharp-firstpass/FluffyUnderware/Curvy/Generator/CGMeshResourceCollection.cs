using System;
using System.Collections.Generic;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	[Serializable]
	public class CGMeshResourceCollection : ICGResourceCollection
	{
		public List<CGMeshResource> Items = new List<CGMeshResource>();

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
