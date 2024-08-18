using System;
using System.Threading;

namespace FluffyUnderware.DevTools
{
	public class ThreadPoolWorker : IDisposable
	{
		private class QueuedCallback
		{
			public WaitCallback Callback;

			public object State;
		}

		private int _remainingWorkItems = 1;

		private ManualResetEvent _done = new ManualResetEvent(false);

		public void QueueWorkItem(WaitCallback callback)
		{
			QueueWorkItem(callback, null);
		}

		public void QueueWorkItem(Action act)
		{
			QueueWorkItem(act, null);
		}

		public void QueueWorkItem(WaitCallback callback, object state)
		{
			ThrowIfDisposed();
			QueuedCallback queuedCallback = new QueuedCallback();
			queuedCallback.Callback = callback;
			queuedCallback.State = state;
			lock (_done)
			{
				_remainingWorkItems++;
			}
			ThreadPool.QueueUserWorkItem(HandleWorkItem, queuedCallback);
		}

		public void QueueWorkItem(Action act, object state)
		{
			ThrowIfDisposed();
			QueuedCallback queuedCallback = new QueuedCallback();
			queuedCallback.Callback = delegate
			{
				act();
			};
			queuedCallback.State = state;
			lock (_done)
			{
				_remainingWorkItems++;
			}
			ThreadPool.QueueUserWorkItem(HandleWorkItem, queuedCallback);
		}

		public bool WaitAll()
		{
			return WaitAll(-1, false);
		}

		public bool WaitAll(TimeSpan timeout, bool exitContext)
		{
			return WaitAll((int)timeout.TotalMilliseconds, exitContext);
		}

		public bool WaitAll(int millisecondsTimeout, bool exitContext)
		{
			ThrowIfDisposed();
			DoneWorkItem();
			bool flag = _done.WaitOne(millisecondsTimeout, exitContext);
			lock (_done)
			{
				if (flag)
				{
					_remainingWorkItems = 1;
					_done.Reset();
				}
				else
				{
					_remainingWorkItems++;
				}
			}
			return flag;
		}

		private void HandleWorkItem(object state)
		{
			QueuedCallback queuedCallback = (QueuedCallback)state;
			try
			{
				queuedCallback.Callback(queuedCallback.State);
			}
			finally
			{
				DoneWorkItem();
			}
		}

		private void DoneWorkItem()
		{
			lock (_done)
			{
				_remainingWorkItems--;
				if (_remainingWorkItems == 0)
				{
					_done.Set();
				}
			}
		}

		private void ThrowIfDisposed()
		{
			if (_done == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		public void Dispose()
		{
			if (_done != null)
			{
				((IDisposable)_done).Dispose();
				_done = null;
			}
		}
	}
}
