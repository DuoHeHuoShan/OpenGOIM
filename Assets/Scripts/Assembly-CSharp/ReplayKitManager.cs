using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplayKitManager : MonoBehaviour
{
	public Button StartStreamButton;

	public Button StopStreamButton;

	public Toggle camToggle;

	public Toggle micToggle;

	public GameObject shareURL;

	public TextMeshProUGUI URLText;

	public bool CamEnabled = true;

	public bool MicEnabled = true;

	private static Vector2 CameraPreviewPositionOffset = new Vector2(150f, 50f);

	private float camDisableTimer;

	private float camEnableTimer;

	private bool setCameraState;

	private bool broadcastCallbackFailed;

	private bool broadcastCallbackSuccess;

	private MobileManager mobileMan;

	private MobileManager.MobileScale scale;

	private float scaleModifier;
}
