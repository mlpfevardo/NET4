using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MMK_Table : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	public MMK_Manager.MMK_ScrollType  vScrollType = MMK_Manager.MMK_ScrollType.None;
	public float vOffSet = 0f;								//add this offset to stop the scroll bar X or Y

	//private variable 
	private Canvas vCanvas; 
	private bool IsMovingMenu = false;
	private Vector3 vOriginalPos = Vector3.zero;
	private Vector3 vOriginalObjPos = Vector3.zero;

	//hold the current position where the slider must 
	private float vStartPos = 0f;
	private GameObject vMainObj;
	private float vDiff = 0f;
	private MMK_Manager vMMKManager;
	private bool vCanBeUsed = false;
	private float vMenuSpeed = 0f;
	private float vPrecisionMove = 0.002f;
	private float vLenght = 0f;
	private bool vIsAbove = false;

	// Use this for initialization
	void Start () {

		vIsAbove = false;

		//add the Button on start 
		gameObject.AddComponent<Button> ();

		//disabled by
		vCanBeUsed = false;

		//get the MMKManager 
		vMMKManager = GameObject.Find ("Canvas").GetComponent<MMK_Manager> ();

		//get the orignal position when the game launch
		vOriginalObjPos = transform.localPosition;

		//get the MKK_Table to be moved
		vMainObj = this.gameObject;
			
		//get teh same menu speed as the Menu
		vMenuSpeed = vMMKManager.vMenuSpeed;
	}

	//go back to the original pos
	public void StopMoving()
	{
		//go back to original position + don't move!
		IsMovingMenu = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		vIsAbove = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		vIsAbove = false;
	}
		
	// Update is called once per frame
	void Update () {

		//check if we just started to click on GUI
		if (Input.GetMouseButtonDown (0) && vIsAbove) {
			vOriginalPos = MMK_Manager.GetMousePosition ();
			vOriginalObjPos = transform.localPosition;
			IsMovingMenu = true;

			//initialise the starting pos once
			if (vStartPos == 0f) {
				//get the starting points where it need to stop sliding
				if (vScrollType == MMK_Manager.MMK_ScrollType.Vertical) {
					vStartPos = vMainObj.transform.localPosition.y;
				} else {
					vStartPos = vMainObj.transform.localPosition.x;
				}
			}
		} 
		//check if we have finished moving it
		else if (Input.GetMouseButtonUp (0)) {
			IsMovingMenu = false;
		}

		//check if were moving in the 
		if (IsMovingMenu)
		{
			//get the current mouse position
			Vector3 vMousePos = MMK_Manager.GetMousePosition ();
			float vCurrentPos = 0f;

			//check if were using horizontal or vertical
			Vector3 vVectorDiff = Vector3.zero;
			if (vScrollType == MMK_Manager.MMK_ScrollType.Horizontal) {
				//get the diff to know where we are going
				vDiff = vMousePos.x - vOriginalPos.x;

				//get the current position
				vCurrentPos = vMainObj.transform.localPosition.x;
				vLenght = vMainObj.GetComponent<RectTransform> ().sizeDelta.x;

				//calculate the diff on X
				vVectorDiff = vOriginalObjPos + new Vector3((vDiff*Screen.width*3f), 0f, 0f);
			} else {
				//get the diff to know where we are going
				vDiff = vMousePos.y - vOriginalPos.y;

				//get the current position
				vCurrentPos = vMainObj.transform.localPosition.y;
				vLenght = vMainObj.GetComponent<RectTransform> ().sizeDelta.y;

				//calculate the diff on y
				vVectorDiff = vOriginalObjPos + new Vector3(0f, (vDiff*Screen.height*3f), 0f);
			}
				
			//Make sure were moving ONLY this one
			if (Mathf.Abs (vDiff) >= vPrecisionMove) {
				vMMKManager.StopMoving ();
			}

			//Debug.Log(vVectorDiff.y +">"+ vStartPos+" && "+vVectorDiff.y+" < "+vEndPos+"+"+vStartPos+"-"+vParentEndPos);

			//Debug.Log (vCurrentPos+" <= "+Screen.height+"-"+vLenght);


			//check if we have to stop scrolling
			if ((vCurrentPos >= vStartPos && vCurrentPos <= Screen.height-vLenght && MMK_Manager.MMK_ScrollType.Vertical == vScrollType) /*||
				(vCurrentPos >= vStartPos && vCurrentPos <= vEndPos && MMK_Manager.MMK_ScrollType.Horizontal == vScrollType)*/) {

				//make sure we cannot go below starting position
				if (vVectorDiff.y < vStartPos && MMK_Manager.MMK_ScrollType.Vertical == vScrollType)
					vVectorDiff.y = vStartPos;
				else if (vVectorDiff.x < vStartPos && MMK_Manager.MMK_ScrollType.Horizontal == vScrollType)
					vVectorDiff.x = vStartPos;

				//make sure we cannot go after end position
				if (vVectorDiff.y > Screen.height - vLenght && MMK_Manager.MMK_ScrollType.Vertical == vScrollType) 
					vVectorDiff.y = Screen.height - vLenght;
				else if (vVectorDiff.x > Screen.width - vLenght && MMK_Manager.MMK_ScrollType.Horizontal == vScrollType) 
					vVectorDiff.x = Screen.width-vLenght;

				//change the position
				vMainObj.transform.localPosition = vVectorDiff;
			}
		}
	}
}
