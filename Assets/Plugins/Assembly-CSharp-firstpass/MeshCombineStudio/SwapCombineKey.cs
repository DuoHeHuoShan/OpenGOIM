using System.Collections.Generic;
using UnityEngine;

namespace MeshCombineStudio
{
	public class SwapCombineKey : MonoBehaviour
	{
		public static SwapCombineKey instance;

		public List<MeshCombiner> meshCombinerList = new List<MeshCombiner>();

		private MeshCombiner meshCombiner;

		private GUIStyle textStyle;

		private void Awake()
		{
			instance = this;
			meshCombiner = GetComponent<MeshCombiner>();
			meshCombinerList.Add(meshCombiner);
		}

		private void OnDestroy()
		{
			instance = null;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				meshCombiner.SwapCombine();
			}
		}

		private void OnGUI()
		{
			if (textStyle == null)
			{
				textStyle = new GUIStyle("label");
				textStyle.fontStyle = FontStyle.Bold;
				textStyle.fontSize = 16;
			}
			textStyle.normal.textColor = ((!this.meshCombiner.combinedActive) ? Color.red : Color.green);
			GUI.Label(new Rect(10f, 45 + meshCombinerList.Count * 22, 200f, 30f), "Toggle with 'Tab' key.", textStyle);
			for (int i = 0; i < meshCombinerList.Count; i++)
			{
				MeshCombiner meshCombiner = meshCombinerList[i];
				if (meshCombiner.combinedActive)
				{
					GUI.Label(new Rect(10f, 30 + i * 22, 300f, 30f), meshCombiner.gameObject.name + " is Enabled.", textStyle);
				}
				else
				{
					GUI.Label(new Rect(10f, 30 + i * 22, 300f, 30f), meshCombiner.gameObject.name + " is Disabled.", textStyle);
				}
			}
		}
	}
}
