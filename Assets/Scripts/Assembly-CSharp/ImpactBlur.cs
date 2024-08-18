using UnityEngine;

public class ImpactBlur : MonoBehaviour
{
	public GameObject[] sprites;

	public float shrinkTime = 0.2f;

	private MeshRenderer ren;

	private float[] times;

	private float[] sizes;

	private void Start()
	{
		times = new float[sprites.Length];
		sizes = new float[sprites.Length];
		for (int i = 0; i < times.Length; i++)
		{
			times[i] = 0f;
			sizes[i] = 0f;
		}
	}

	private void Update()
	{
		for (int i = 0; i < sprites.Length; i++)
		{
			if (sprites[i].GetComponent<MeshRenderer>().enabled)
			{
				times[i] -= Time.deltaTime;
				float num = (1f - times[i] / shrinkTime * (times[i] / shrinkTime)) * sizes[i];
				sprites[i].transform.localScale = new Vector3(num, num, num);
				sprites[i].GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Tint", times[i] / shrinkTime);
				if (times[i] <= 0f || sprites[i].transform.localScale.x < 0.03f)
				{
					times[i] = 0f;
					sprites[i].GetComponent<MeshRenderer>().enabled = false;
					sprites[i].transform.localScale = new Vector3(1f, 1f, 1f);
				}
			}
		}
	}

	public void Impact(Vector2 pos, float size)
	{
		for (int i = 0; i < sprites.Length; i++)
		{
			if (!sprites[i].GetComponent<MeshRenderer>().enabled)
			{
				sprites[i].GetComponent<MeshRenderer>().enabled = true;
				sprites[i].GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Tint", 1f);
				sprites[i].transform.position = new Vector3(pos.x, pos.y, -7f);
				times[i] = shrinkTime;
				sizes[i] = size;
				sprites[i].transform.localScale = new Vector3(0.1f * size, 0.1f * size, 0.1f * size);
				break;
			}
		}
	}
}
