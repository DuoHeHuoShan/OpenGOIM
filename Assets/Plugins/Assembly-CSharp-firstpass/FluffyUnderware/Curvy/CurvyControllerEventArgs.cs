using UnityEngine;

namespace FluffyUnderware.Curvy
{
	public class CurvyControllerEventArgs : CurvyEventArgs
	{
		public CurvyController Controller;

		public CurvyControllerEventArgs(MonoBehaviour sender, CurvyController controller)
		{
			Sender = sender;
			Controller = controller;
		}
	}
}
