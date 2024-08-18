namespace FluffyUnderware.DevTools
{
	public class ArrayExAttribute : DTAttribute, IDTFieldParsingAttribute
	{
		public bool Draggable = true;

		public bool ShowHeader = true;

		public bool ShowAdd = true;

		public bool ShowDelete = true;

		public bool DropTarget = true;

		public ArrayExAttribute()
			: base(35)
		{
		}
	}
}
