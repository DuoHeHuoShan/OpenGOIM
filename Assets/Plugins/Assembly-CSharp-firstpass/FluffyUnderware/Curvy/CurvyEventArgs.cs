using System.ComponentModel;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	public class CurvyEventArgs : CancelEventArgs
	{
		public MonoBehaviour Sender;

		public object Data;
	}
}
