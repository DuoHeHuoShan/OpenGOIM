namespace FluffyUnderware.DevTools
{
	public class GroupAttribute : DTAttribute, IDTGroupParsingAttribute, IDTGroupRenderAttribute
	{
		public bool Expanded = true;

		public bool Invisible;

		public string Label;

		public string Tooltip;

		public string HelpURL;

		private string mPath;

		public string Path
		{
			get
			{
				return mPath;
			}
			protected set
			{
				PathIsAbsolute = !string.IsNullOrEmpty(value) && value.StartsWith("@");
				if (PathIsAbsolute)
				{
					mPath = value.Substring(1);
					if (string.IsNullOrEmpty(mPath))
					{
						mPath = null;
					}
				}
				else
				{
					mPath = value;
				}
			}
		}

		public bool PathIsAbsolute { get; private set; }

		public GroupAttribute(string pathAndName)
			: base(15)
		{
			Path = pathAndName;
		}
	}
}
