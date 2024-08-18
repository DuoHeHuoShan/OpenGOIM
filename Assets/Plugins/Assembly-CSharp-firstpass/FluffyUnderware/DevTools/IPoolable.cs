namespace FluffyUnderware.DevTools
{
	public interface IPoolable
	{
		void OnBeforePush();

		void OnAfterPop();
	}
}
