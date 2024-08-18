namespace FluffyUnderware.DevTools
{
	public interface IPool
	{
		string Identifier { get; set; }

		PoolSettings Settings { get; }

		int Count { get; }

		void Clear();

		void Reset();

		void Update();
	}
}
