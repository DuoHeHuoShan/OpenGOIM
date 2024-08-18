using UnityEngine;

public class GroundCol : MonoBehaviour
{
	public enum SoundMaterial
	{
		rock = 0,
		wood = 1,
		metal = 2,
		plastic = 3,
		furniture = 4,
		snow = 5,
		cardboard = 6,
		none = 7,
		snake = 8,
		solidmetal = 9
	}

	public Color groundCol = new Color(0.7f, 0.6f, 0.3f);

	public SoundMaterial material;
}
