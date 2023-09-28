using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomColorGenerator : MonoBehaviour
{
	public int textureWidth = 128;  // Set the width of your texture
	public int textureHeight = 128; // Set the height of your texture
	public Image targetImage;       // Assign your UI Image component in the inspector
	

	private void Start()
	{
		//StartCoroutine(Changecolors());
	}
	[SerializeField] bool Stillenerate;
	IEnumerator Changecolors()
	{
		yield return new WaitForEndOfFrame();
		GenerateImage();
		if (Stillenerate)
		{
			StartCoroutine(Changecolors());
		}
	}
	[SerializeField] Color[] colors = new Color[Screen.width * Screen.height];
	int index = 0;
	void GenerateImage()
	{
		textureWidth = Screen.width;
		textureHeight = Screen.height;
		Texture2D texture = new Texture2D(textureWidth, textureHeight);
		//for (int x = 0; x < textureWidth; x++)
		//{
		//	for (int y = 0; y < textureHeight; y++)
		//	{
		//		float random1 = Random.value;
		//		float random2 = Random.value;
		//		float random3 = Random.value;
		//		if (index < 100000)
		//		{
		//			random1 = random2 = random3 = 1;
		//		}
		//		else
		//		{
		//			random1 = random3 * random2;
		//		}
		//		//Color randomColor = new Color(random1, random2, random3);
		//		//texture.SetPixel(x, y, randomColor);
		//		index++;
		//	}
		//	index++;
		//}
		index = Random.Range(0,Screen.width*Screen.height);
		float random1 = Random.value;
		float random2 = Random.value;
		float random3 = Random.value;
		colors[index] = new Color(random1, random2, random3);
		texture.SetPixels(colors);
		texture.Apply();
		Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
		this.GetComponent<Image>().sprite = sprite;
	}
	private void Update()
	{
		GenerateImage();
		if (Input.touchCount > 0)
		{
			if (Input.GetTouch(0).phase == TouchPhase.Began)
			{
				//GenerateImage();
			}
		}
	}
}
