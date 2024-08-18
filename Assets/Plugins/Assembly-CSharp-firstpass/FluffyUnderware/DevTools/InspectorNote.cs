using UnityEngine;

namespace FluffyUnderware.DevTools
{
	public class InspectorNote : MonoBehaviour
	{
		[TextArea(5, 20)]
		[SerializeField]
		private string m_Note;
	}
}
