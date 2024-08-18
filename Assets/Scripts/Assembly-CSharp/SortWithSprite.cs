using UnityEngine;

public class SortWithSprite : MonoBehaviour
{
	private SkinnedMeshRenderer ren;

	private void Start()
	{
		ren = GetComponent<SkinnedMeshRenderer>();
		ren.sortingLayerName = "Arms";
		ren.sortingLayerID = SortingLayer.NameToID("Arms");
		ren.sortingOrder = 0;
	}

	private void Update()
	{
	}
}
