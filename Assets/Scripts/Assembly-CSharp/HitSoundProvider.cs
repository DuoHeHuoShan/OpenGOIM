using System.Collections.Generic;
using UnityEngine;

public class HitSoundProvider : MonoBehaviour
{
	public List<AudioClip> rockhits;

	public List<AudioClip> woodhits;

	public List<AudioClip> metalhits;

	public List<AudioClip> plastichits;

	public List<AudioClip> furniturehits;

	public List<AudioClip> snowhits;

	public List<AudioClip> cardboardhits;

	public List<AudioClip> snakehits;

	public List<AudioClip> solidmetalhits;

	public List<AudioClip> rockhardhits;

	public List<AudioClip> woodhardhits;

	public List<AudioClip> metalhardhits;

	public List<AudioClip> plastichardhits;

	public List<AudioClip> furniturehardhits;

	public List<AudioClip> snowhardhits;

	public List<AudioClip> cardboardhardhits;

	public List<AudioClip> snakehardhits;

	public List<AudioClip> solidmetalhardhits;

	public List<AudioClip> rockscrapes;

	public List<AudioClip> woodscrapes;

	public List<AudioClip> metalscrapes;

	public List<AudioClip> plasticscrapes;

	public List<AudioClip> furniturescrapes;

	public List<AudioClip> snowscrapes;

	public List<AudioClip> cardboardscrapes;

	public List<AudioClip> snakescrapes;

	public List<AudioClip> solidmetalscrapes;

	public AudioClip GetHit(GroundCol.SoundMaterial mat)
	{
		switch (mat)
		{
		case GroundCol.SoundMaterial.rock:
			return rockhits[Random.Range(0, rockhits.Count)];
		case GroundCol.SoundMaterial.wood:
			return woodhits[Random.Range(0, woodhits.Count)];
		case GroundCol.SoundMaterial.metal:
			return metalhits[Random.Range(0, metalhits.Count)];
		case GroundCol.SoundMaterial.plastic:
			return plastichits[Random.Range(0, plastichits.Count)];
		case GroundCol.SoundMaterial.furniture:
			return furniturehits[Random.Range(0, furniturehits.Count)];
		case GroundCol.SoundMaterial.snow:
			return snowhits[Random.Range(0, snowhits.Count)];
		case GroundCol.SoundMaterial.cardboard:
			return cardboardhits[Random.Range(0, cardboardhits.Count)];
		case GroundCol.SoundMaterial.snake:
			return snakehits[Random.Range(0, snakehits.Count)];
		case GroundCol.SoundMaterial.solidmetal:
			return solidmetalhits[Random.Range(0, solidmetalhits.Count)];
		case GroundCol.SoundMaterial.none:
			return null;
		default:
			return rockhits[Random.Range(0, rockhits.Count)];
		}
	}

	public AudioClip GetHardHit(GroundCol.SoundMaterial mat)
	{
		switch (mat)
		{
		case GroundCol.SoundMaterial.rock:
			return rockhardhits[Random.Range(0, rockhardhits.Count)];
		case GroundCol.SoundMaterial.wood:
			return woodhardhits[Random.Range(0, woodhardhits.Count)];
		case GroundCol.SoundMaterial.metal:
			return metalhardhits[Random.Range(0, metalhardhits.Count)];
		case GroundCol.SoundMaterial.plastic:
			return plastichardhits[Random.Range(0, plastichardhits.Count)];
		case GroundCol.SoundMaterial.furniture:
			return furniturehardhits[Random.Range(0, furniturehardhits.Count)];
		case GroundCol.SoundMaterial.snow:
			return snowhardhits[Random.Range(0, snowhardhits.Count)];
		case GroundCol.SoundMaterial.cardboard:
			return cardboardhardhits[Random.Range(0, cardboardhardhits.Count)];
		case GroundCol.SoundMaterial.snake:
			return snakehardhits[Random.Range(0, snakehardhits.Count)];
		case GroundCol.SoundMaterial.solidmetal:
			return solidmetalhardhits[Random.Range(0, solidmetalhardhits.Count)];
		case GroundCol.SoundMaterial.none:
			return null;
		default:
			return rockhardhits[Random.Range(0, rockhardhits.Count)];
		}
	}

	public AudioClip GetScrape(GroundCol.SoundMaterial mat)
	{
		switch (mat)
		{
		case GroundCol.SoundMaterial.rock:
			return rockscrapes[Random.Range(0, rockscrapes.Count)];
		case GroundCol.SoundMaterial.wood:
			return woodscrapes[Random.Range(0, woodscrapes.Count)];
		case GroundCol.SoundMaterial.metal:
			return metalscrapes[Random.Range(0, metalscrapes.Count)];
		case GroundCol.SoundMaterial.plastic:
			return plasticscrapes[Random.Range(0, plasticscrapes.Count)];
		case GroundCol.SoundMaterial.furniture:
			return furniturescrapes[Random.Range(0, furniturescrapes.Count)];
		case GroundCol.SoundMaterial.snow:
			return snowscrapes[Random.Range(0, snowscrapes.Count)];
		case GroundCol.SoundMaterial.cardboard:
			return cardboardscrapes[Random.Range(0, cardboardscrapes.Count)];
		case GroundCol.SoundMaterial.snake:
			return snakescrapes[Random.Range(0, snakescrapes.Count)];
		case GroundCol.SoundMaterial.solidmetal:
			return solidmetalscrapes[Random.Range(0, solidmetalscrapes.Count)];
		case GroundCol.SoundMaterial.none:
			return null;
		default:
			return rockscrapes[Random.Range(0, rockscrapes.Count)];
		}
	}
}
