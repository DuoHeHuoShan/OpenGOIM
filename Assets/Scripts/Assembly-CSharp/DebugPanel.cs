using UnityEngine;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour
{
	public PlayerControl player;

	public Slider velocityToWorldSlider;

	public Text velocityToWorldValue;

	public Slider rawInputSlider;

	public Text rawInputValue;

	public Toggle expInputToggle;

	public Toggle smoothInputToggle;

	public Toggle smoothOutputToggle;

	public Toggle multOutputVelocityAverage;

	public Toggle multOutputTimeDelta;

	public Text sensitivityValue;
}
