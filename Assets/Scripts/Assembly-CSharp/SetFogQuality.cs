using UnityEngine;

public class SetFogQuality : MonoBehaviour
{
	private FogVolume fog;

	private FogVolumeRenderer fogrenderer;

	public int[] sampleAmounts = new int[3] { 8, 6, 4 };

	private int[] stratusIterations = new int[6] { 20, 30, 30, 30, 40, 40 };

	private int[] cumulusIterations = new int[6] { 15, 25, 30, 35, 50, 60 };

	private int[] spaceCloudIterations = new int[6] { 10, 20, 20, 30, 40, 40 };

	private int[] defaultIterations = new int[7] { 10, 15, 15, 15, 25, 25, 30 };

	private void Start()
	{
	}

	public void SetQuality(int quality)
	{
		if (quality < QualitySettings.names.Length / 2)
		{
			fogrenderer._Downsample = sampleAmounts[0];
		}
		else if (quality < QualitySettings.names.Length - 1)
		{
			fogrenderer._Downsample = sampleAmounts[1];
		}
		else
		{
			fogrenderer._Downsample = sampleAmounts[2];
		}
		FogVolume component = GetComponent<FogVolume>();
		if (component != null)
		{
			switch (base.gameObject.name)
			{
			case "Stratus":
				component.Iterations = stratusIterations[quality];
				break;
			case "Cumulus":
				component.Iterations = cumulusIterations[quality];
				break;
			case "SpaceCloud":
				component.Iterations = spaceCloudIterations[quality];
				break;
			default:
				component.Iterations = defaultIterations[quality];
				break;
			}
		}
	}

	private void Update()
	{
	}
}
