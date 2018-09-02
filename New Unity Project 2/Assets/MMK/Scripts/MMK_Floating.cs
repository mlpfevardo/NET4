using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class MMK_Floating : MonoBehaviour {

	//keep in memory the original Color to make sure it fit when se change it.
	[System.Serializable]
	public class MMK_TextColor
	{
		public Text vText;										
		public Color vOriginalColor;
	}

	[System.Serializable]
	public class MMK_ImageColor
	{
		public Image vImage;										
		public Color vOriginalColor;
	}

	private List<MMK_TextColor> vTextList;
	private List<MMK_ImageColor> vImageList;

	void Start()
	{
		//initialize them all
		vTextList = new List<MMK_TextColor> ();
		vImageList = new List<MMK_ImageColor> ();

		//change the color for the images
		foreach (Image vCurImage in GetComponentsInChildren<Image> ()) {
			MMK_ImageColor vImageC = new MMK_ImageColor ();
			vImageC.vOriginalColor = vCurImage.color;
			vImageC.vImage = vCurImage;
			//vCurImage.color = new Color (vCurImage.color.r, vCurImage.color.g, vCurImage.color.b, vNewAlpha);
			vImageList.Add(vImageC);
		}

		//change the color for the images
		foreach (Text vCurText in GetComponentsInChildren<Text> ()) {
			MMK_TextColor vTextC = new MMK_TextColor ();
			vTextC.vOriginalColor = vCurText.color;
			vTextC.vText = vCurText;
			vTextList.Add(vTextC);
		}
	}

	public void ShowHideFloating (bool vChoice)
	{
		//enable it before doing anything because it will be disabled
		if (vChoice) {
			gameObject.SetActive (vChoice);

			//destroy corps
			StartCoroutine (ShowHide (vChoice));
		}

		//enable or disable this gameobject at the end
		gameObject.SetActive (vChoice);
	}

	//show/hide menu smoothly
	IEnumerator ShowHide (bool vChoice)
	{
		float vAlpha = 1f;
		if (!vChoice) {
			//hide it
			while (vAlpha > 0f) {
				vAlpha -= 0.05f; 
				yield return new WaitForSeconds (0.01f);
				ChangeAlpha (vAlpha);
			}

			ChangeAlpha (0f);
		} else {
			//show it
			vAlpha = 0f;

			while (vAlpha < 1f) {
				vAlpha += 0.05f; 
				yield return new WaitForSeconds (0.01f);
				ChangeAlpha (vAlpha);
			}

			ChangeAlpha (1f);
		}
	}

	void ChangeAlpha (float vNewAlpha)
	{
		foreach (MMK_ImageColor vCurImageC in vImageList) 
			vCurImageC.vImage.color = new Color (vCurImageC.vOriginalColor.r, vCurImageC.vOriginalColor.g, vCurImageC.vOriginalColor.b, vCurImageC.vOriginalColor.a*(vNewAlpha/1f));

		foreach (MMK_TextColor vCurTextC in vTextList) 
			vCurTextC.vText.color = new Color (vCurTextC.vOriginalColor.r, vCurTextC.vOriginalColor.g, vCurTextC.vOriginalColor.b, vCurTextC.vOriginalColor.a*(vNewAlpha/1f));
	}
}
