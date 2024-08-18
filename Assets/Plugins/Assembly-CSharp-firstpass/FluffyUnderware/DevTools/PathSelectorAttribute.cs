using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public class PathSelectorAttribute : DTPropertyAttribute
	{
		public enum DialogMode
		{
			OpenFile = 0,
			OpenFolder = 1,
			CreateFile = 2
		}

		public readonly DialogMode Mode;

		public string Title;

		public string Directory;

		public string Extension;

		public string DefaultName;

		public PathSelectorAttribute(DialogMode mode = DialogMode.OpenFile)
			: base(string.Empty, string.Empty)
		{
			Mode = mode;
			Directory = Application.dataPath;
		}
	}
}
