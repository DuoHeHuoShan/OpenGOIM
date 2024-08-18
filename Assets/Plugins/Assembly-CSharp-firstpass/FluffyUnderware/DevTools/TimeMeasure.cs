using System;
using System.Diagnostics;

namespace FluffyUnderware.DevTools
{
	public class TimeMeasure : Ring<long>
	{
		public Stopwatch mWatch = new Stopwatch();

		public double LastTicks
		{
			get
			{
				return base[base.Count - 1];
			}
		}

		public double LastMS
		{
			get
			{
				return LastTicks / 10000.0;
			}
		}

		public double AverageMS
		{
			get
			{
				long num = 0L;
				for (int i = 0; i < base.Count; i++)
				{
					num += base[i];
				}
				return DTMath.FixNaN((double)num / 10000.0 / (double)base.Count);
			}
		}

		public double MinimumMS
		{
			get
			{
				long num = long.MaxValue;
				for (int i = 0; i < base.Count; i++)
				{
					num = Math.Min(num, base[i]);
				}
				return DTMath.FixNaN((double)num / 10000.0);
			}
		}

		public double MaximumMS
		{
			get
			{
				long num = long.MinValue;
				for (int i = 0; i < base.Count; i++)
				{
					num = Math.Max(num, base[i]);
				}
				return DTMath.FixNaN((double)num / 10000.0);
			}
		}

		public double AverageTicks
		{
			get
			{
				long num = 0L;
				for (int i = 0; i < base.Count; i++)
				{
					num += base[i];
				}
				return (double)num / (double)base.Count;
			}
		}

		public double MinimumTicks
		{
			get
			{
				long num = long.MaxValue;
				for (int i = 0; i < base.Count; i++)
				{
					num = Math.Min(num, base[i]);
				}
				return num;
			}
		}

		public double MaximumTicks
		{
			get
			{
				long num = 0L;
				for (int i = 0; i < base.Count; i++)
				{
					num = Math.Max(num, base[i]);
				}
				return num;
			}
		}

		public TimeMeasure(int size)
			: base(size)
		{
		}

		public void Start()
		{
			mWatch.Start();
		}

		public void Stop()
		{
			mWatch.Stop();
			Add(mWatch.ElapsedTicks);
			mWatch.Reset();
		}

		public void Pause()
		{
			mWatch.Stop();
		}
	}
}
