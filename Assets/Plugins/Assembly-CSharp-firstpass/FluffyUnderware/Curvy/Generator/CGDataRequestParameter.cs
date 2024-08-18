namespace FluffyUnderware.Curvy.Generator
{
	public class CGDataRequestParameter
	{
		public static implicit operator bool(CGDataRequestParameter a)
		{
			return !object.ReferenceEquals(a, null);
		}
	}
}
