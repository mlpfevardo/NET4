using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class countdown : MonoBehaviour {

	public float timeLeft = 86400f;
	private Text text;

	void Start()
	{
		//get the Text component first
		text = GetComponent<Text> ();
	}

	string ConvertWithZero(float fValue)
	{
		string vValue = fValue.ToString ();
		if (vValue.Length == 1)
			vValue = "0" + vValue;

		return vValue;
	}

	void Update()
	{
		if (timeLeft > 0) {
			timeLeft -= Time.deltaTime;

			float vhour = Mathf.Floor(timeLeft / 60 / 60);
			float vmin = Mathf.Floor(Mathf.Floor(timeLeft-(vhour*60*60))/60);
			float vsec = (Mathf.Floor(timeLeft-((vhour*60*60)+(vmin*60))));

			text.text = ConvertWithZero (vhour) + ":" + ConvertWithZero (vmin) + ":" + ConvertWithZero (vsec); //+ ":" + (vsec).ToString("00");
		}
	}
}
