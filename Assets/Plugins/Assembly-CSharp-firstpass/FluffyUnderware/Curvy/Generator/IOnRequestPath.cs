namespace FluffyUnderware.Curvy.Generator
{
	public interface IOnRequestPath : IOnRequestProcessing
	{
		float PathLength { get; }

		bool PathIsClosed { get; }
	}
}
