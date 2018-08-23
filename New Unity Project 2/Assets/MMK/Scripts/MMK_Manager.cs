using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MMK_Manager : MonoBehaviour {

	public enum MMK_ScrollType{Vertical, Horizontal, None};
	public enum MMK_Direction{Left, Right, Top, Bottom};

	[System.Serializable]
	public class MMK_Panel
	{
		public GameObject vPanel;											//keep in memory which panel
		public Sprite vPanelIcon; 											//will show the icons in the Nav
		public string vName; 												//name of this Menu Panel
		public GameObject vIconObj; 
		public List<MMK_Floating> vFloatingMenu; 							//handle here if we show or hide these floating menu when we switch between main panel. 
		public List<MMK_Table> vSubTableList; 								//keep all the childs 
	}

	//Public variable for the client
	public List<MMK_Panel> vPanelList; 										//keep all the menu panel in the right order
	public MMK_ScrollType vScrollType = MMK_ScrollType.Horizontal; 			//check in which direction we are scrolling
	public int vStartingPanel = 0;
	public float vMenuSpeed = 0.5f;

	//Nav
	public bool ShowNavIcons = true;										//construct automatically
	public MMK_Direction NavDirection = MMK_Direction.Bottom; 				//create a panel on the right direction
	public float vNavThicknessPerc = 0.2f;									//will produce the navBar with this Thickness in Percentage. 1f = 100% of the screen.

	//private variable 
	private Canvas vCanvas; 
	private bool IsMovingMenu = false;
	private float PercScreenToMove = 0.1f;
	private Vector3 vOriginalPos = Vector3.zero;
	private static Camera vCam; 
	private GameObject vMainObj;
	private GameObject vNavObj;
	private int vCurrentNbr = 0;
	private float vDiff = 0f;
	private float vElapseTime = 0f;											//used for lerping the menus smoothly
	private string IsGoingTo = "";											//right, left, up, down
	private int vDirFrom = 0;
	private int vDirTo = 0;
	private float vPrecisionMove = 0.006f;
	private List<MMK_Floating> vListFloatingMenu; 							//get all the floating menu here so we know if they are active in a page OR not.
	private float vScreenDimX = 0f;
	private float vScreenDimY = 0f;

	// Use this for initialization
	void Start () {
		//check all the infos provided by the user to make sure there are no error
		ValidateVariables();

		//initialise all the panel components
		InitialiseMMK ();

		//update starting position
		UpdateMenuPos ();

		//make sure all the Icons are updated correctly
		UpdateIconsBtn ();

		//disable all MK_Table by default
		foreach (MMK_Panel vCurPanel in vPanelList)
			foreach (MMK_Table vCurTable in vCurPanel.vSubTableList)
				vCurTable.enabled = false;

		//disable all MMK_Table
		UpdateMMKTable(true);

		//update all the floating menus at once to show the right one
		UpdateFloatingMenus ();

		//only show selected
		HandlingMenu ("Selected");
	}

	public void ShowGameObject(GameObject vObj)
	{
		if (vObj != null)
			vObj.gameObject.SetActive (true);
	}

	public void HideGameObject(GameObject vObj)
	{
		if (vObj != null)
			vObj.gameObject.SetActive (false);
	}
		
	void UpdateMMKTable(bool vChoice)
	{
		//enable/disable MMK_Table
		foreach (MMK_Table vCurTable in vPanelList[vCurrentNbr].vSubTableList) {
			vCurTable.StopMoving ();
			vCurTable.enabled = vChoice;
		}
	}

	void ValidateVariables()
	{
		//make sure the MMK_panel list has valid panel
		List<MMK_Panel> vValidatedList = new List<MMK_Panel> ();
		foreach (MMK_Panel vCurPanel in vPanelList)
			if (vCurPanel.vPanel != null)
				vValidatedList.Add (vCurPanel);

		//then replace the list with the new validated list
		vPanelList = vValidatedList;

		//make sure we cannot have a negative value here OR be higher than the last panel
		if (vStartingPanel >= vPanelList.Count) 
			vStartingPanel = vPanelList.Count - 1;
		else if (vStartingPanel <= 0) 
			vStartingPanel = 0;
		else 
			vStartingPanel -= 1;

		//get the new value
		vCurrentNbr = vStartingPanel;
	}

	//create all the panel together in the right order and positionnate the default menu to be shown correctly
	void InitialiseMMK()
	{
		//check what is the Width of every subPanel
		vCanvas = GetComponent<Canvas>();
		vCam = Camera.main;
		vMainObj = vCanvas.transform.Find ("Main").gameObject;

		//get the Resolution 1 time
		vScreenDimX = vCanvas.GetComponent<RectTransform> ().rect.width;
		vScreenDimY = vCanvas.GetComponent<RectTransform> ().rect.height;

		int vOriginalNbr = vCurrentNbr;

		//initialise the floating list
		vListFloatingMenu = new List<MMK_Floating> ();

		vCurrentNbr = 0;
		foreach (MMK_Panel vCurPanel in vPanelList) {
			//construct it
			vCurPanel.vPanel.GetComponent<RectTransform> ().offsetMin = GetMenuPos();
			vCurPanel.vPanel.GetComponent<RectTransform> ().offsetMax = GetMenuPos();

			//add all the floating menu in this list
			foreach (MMK_Floating vCurFloating in vCurPanel.vFloatingMenu)
				if (!vListFloatingMenu.Contains (vCurFloating))
					vListFloatingMenu.Add (vCurFloating);

			//increase counter
			vCurrentNbr++;
		}

		//go back to the original Nbr
		vCurrentNbr = vOriginalNbr;

		//check if we show the menu icons
		if (ShowNavIcons) {
			//create the NavMenu
			vNavObj = (GameObject)Instantiate(Resources.Load("Fabrik/MMK_Navbar"));
			vNavObj.name = "Navbar";
			vNavObj.transform.parent = vCanvas.transform;

			//Navbar
			Vector2 vNewPosMin = new Vector2(0f, 0f);
			Vector2 vNewPosMax = new Vector2(0f, 0f);

			//Buttons
			Vector2 vBtnPosMin = new Vector2 (0f, 0f);
			Vector2 vBtnPosMax = new Vector2 (0f, 0f);
			Vector2 vPiecesDim = new Vector2 (0f, 0f);

			//get this components for further usage
			RectTransform vTrans = vNavObj.GetComponent<RectTransform> (); 

			//positionate the menu itself on the right side of the screen
			switch (NavDirection) {
			case MMK_Direction.Bottom:
					//navbar
					vTrans.anchorMin = new Vector2 (0f, 0f);
					vTrans.anchorMax = new Vector2 (0f, 0f);
					vNewPosMin = new Vector2 (0f, 0f);
					vNewPosMax = new Vector2 (vScreenDimX, (vScreenDimY * vNavThicknessPerc));

					//buttons
					vBtnPosMin = new Vector2 (0f, 0f);
					vBtnPosMax = new Vector2 (vScreenDimX / vPanelList.Count, (vScreenDimY * vNavThicknessPerc));
					vPiecesDim = new Vector2 (vScreenDimX / vPanelList.Count, 0f);
				break;
				case MMK_Direction.Top:
					//navbar
					vTrans.anchorMin = new Vector2 (0f, 1f);
					vTrans.anchorMax = new Vector2 (0f, 1f);
					vNewPosMin = new Vector2(0f, -(Screen.height*vNavThicknessPerc));
					vNewPosMax = new Vector2(Screen.width, 0f);

					//buttons
					vBtnPosMin = new Vector2(0f, -(Screen.height*vNavThicknessPerc));
					vBtnPosMax = new Vector2(Screen.width/vPanelList.Count, 0f);
					vPiecesDim = new Vector2(Screen.width/vPanelList.Count, 0f);
				break;
				case MMK_Direction.Left: 
					//narbar
					vTrans.anchorMin = new Vector2 (0f, 0f);
					vTrans.anchorMax = new Vector2 (0f, 0f);
					vNewPosMin = new Vector2(0f, 0f);
					vNewPosMax = new Vector2((Screen.width*vNavThicknessPerc), Screen.height);

					//buttons
					vBtnPosMin = new Vector2(0f, 0f);
					vBtnPosMax = new Vector2((Screen.width*vNavThicknessPerc), Screen.height/vPanelList.Count);
					vPiecesDim = new Vector2(0f, Screen.height/vPanelList.Count);
				break;
				case MMK_Direction.Right:
					//narbar
					vTrans.anchorMin = new Vector2 (1f, 0f);
					vTrans.anchorMax = new Vector2 (1f, 0f);
					vNewPosMin = new Vector2(-(Screen.width*vNavThicknessPerc), 0f);
					vNewPosMax = new Vector2(0f, Screen.height);

					//buttons
					vBtnPosMin = new Vector2(-(Screen.width*vNavThicknessPerc), 0f);
					vBtnPosMax = new Vector2(0f, Screen.height/vPanelList.Count);
					vPiecesDim = new Vector2(0f, Screen.height/vPanelList.Count);
				break;
			}

			//repositionnate the brand new menus icons
			vTrans.offsetMin = vNewPosMin;
			vTrans.offsetMax = vNewPosMax;
			vTrans.localScale = new Vector3 (1f, 1f, 1f);

			int cpt = 0;

			//Create the Icons to be clicked and updated
			foreach (MMK_Panel vPanel in vPanelList) {
				//create the NavMenu
				GameObject vNewIconObj = (GameObject)Instantiate(Resources.Load("Fabrik/MMK_NavIcons"));
				vNewIconObj.name = vPanel.vName+"Btn";
				vNewIconObj.transform.parent = vNavObj.transform;

				//apply the same anchor type to the button so they can be constructed/handled easily
				RectTransform vNewTrans = vNewIconObj.GetComponent<RectTransform> ();
				vNewTrans.anchorMin = vTrans.anchorMin;
				vNewTrans.anchorMax = vTrans.anchorMax;
				vNewTrans.localScale = new Vector3 (1f, 1f, 1f);

				vNewTrans.offsetMin = vBtnPosMin+(vPiecesDim*cpt);
				vNewTrans.offsetMax = vBtnPosMax+(vPiecesDim*cpt);

				//add OnClick to make sure when we click on it, it will change all the menu 
				vNewIconObj.GetComponent<Button> ().onClick.AddListener (() => ChangeMenuByBtn (vNewIconObj.GetComponent<Button> ()));

				//show the right Menu name
				vNewIconObj.transform.Find ("Text").GetComponent<Text> ().text = vPanel.vName;

				//show the right menu Sprite
				vNewIconObj.transform.Find ("Image").GetComponent<Image> ().sprite = vPanel.vPanelIcon;

				//save this new icon obj in the List
				vPanel.vIconObj = vNewIconObj;

				//disable all MMK_Table
				UpdateMMKTable(false);

				//increase counter
				cpt++;
			}
		}
	}

	Vector2  GetMenuPos()
	{
		//check if we use the menu for Horizontal view or Vertical view
		if (vScrollType == MMK_ScrollType.Horizontal)
			return new Vector2(vScreenDimX*vCurrentNbr, 0f);
		else 
			return new Vector2(0f, vScreenDimY*vCurrentNbr);
	}

	//get the Mouse Position
	public static Vector3 GetMousePosition()
	{
		Ray ray = vCam.ScreenPointToRay(Input.mousePosition);
		Vector3 mousePos = ray.origin;

		//get the mouse click position
		return new Vector2 (mousePos.x, mousePos.y);
	}

	//updat the menu position by moving the Main Obj
	void UpdateMenuPos()
	{
		Vector2 vNewPos;
		//check if we use the menu for Horizontal view or Vertical view
		if (vScrollType == MMK_ScrollType.Horizontal)
			vNewPos = new Vector2(vScreenDimX*vCurrentNbr, 0f);
		else 
			vNewPos = new Vector2(0f, vScreenDimY*vCurrentNbr);
		
		//doesn,t move the menu at all
		vMainObj.GetComponent<RectTransform> ().offsetMin = -GetMenuPos();
		vMainObj.GetComponent<RectTransform> ().offsetMax = -GetMenuPos();
	}

	//make sure all the Icons are updated correctly
	void UpdateIconsBtn()
	{
		int cpt = 0;
		foreach (MMK_Panel vCurPanel in vPanelList) {

			//get the image component
			Image vCurImage = vCurPanel.vIconObj.GetComponent<Image> ();
			Text vPanelText = vCurPanel.vIconObj.transform.Find ("Text").GetComponent<Text> ();
			Animator vAnimator = vCurPanel.vIconObj.GetComponent<Animator> ();

			//check if it's currently selected
			if (cpt == vCurrentNbr) {
				vCurImage.enabled = true;
				vPanelText.enabled = true;
				vAnimator.SetBool ("Show", true);
			} else {
				vCurImage.enabled = false;
				vPanelText.enabled = false;
				vAnimator.SetBool ("Show", false);
			}

			//increase the counter
			cpt++;
		}
	}

	public void ChangeMenuByBtn(Button vBtn)
	{
		int vMenuNbr = 0;
		int cpt = 0;
		foreach (MMK_Panel vPanel in vPanelList)
		{
			//check if we have the same button name 
			if (vPanel.vName == vBtn.transform.Find("Text").GetComponent<Text>().text)
				vMenuNbr = cpt;

			//increase counter
			cpt++;
		}

		//check in which direction we are going!
		if (vMenuNbr < vCurrentNbr) {
			IsGoingTo = "Decrease";
			vElapseTime = 0f;
			vDirFrom = -1;
			vDirTo = 0;
		} else if (vMenuNbr > vCurrentNbr) {
			IsGoingTo = "Increase";
			vElapseTime = 0f;
			vDirFrom = 1; 
			vDirTo = 0;
		} else {
			//NO change done, go back to current position
			IsGoingTo = "Same";
			vElapseTime = 0f;
			vDirFrom = 0;
			vDirTo = 0;
		}

		//disable last MMK_Table
		UpdateMMKTable(false);

		//change the current Menu Nbr
		vCurrentNbr = vMenuNbr;

		//enable new MMK_Table
		UpdateMMKTable(true);

		//update all the floating menus at once to show the right one
		UpdateFloatingMenus ();

		//make sure all the Icons are updated correctly
		UpdateIconsBtn ();
	}

	//stop moving and go back to original position
	public void StopMoving()
	{
		IsMovingMenu = false;

		//NO change done, go back to current position
		IsGoingTo = "Same";
		vElapseTime = 0f;
		vDirFrom = 0;
		vDirTo = 0;

		//only show the selected
		HandlingMenu ("Selected");
	}

	//show All the menu OR only the selected one to optimize the menu speed
	void HandlingMenu (string vChoice)
	{
		if (vChoice == "All") {
			foreach (MMK_Panel vPanel in vPanelList)
				vPanel.vPanel.SetActive (true);
		} else {
			int cpt = 0;
			foreach (MMK_Panel vPanel in vPanelList) {
				//activate ONLY the selected
				if (cpt == vCurrentNbr)
					vPanel.vPanel.SetActive (true);
				else
					vPanel.vPanel.SetActive (false);

				//increase counter
				cpt++;
			}
		}
	}

	void UpdateFloatingMenus()
	{
		//check all MMK_Floating in the private list and check if they are active in teh current Menu so we show or hide them all
		foreach (MMK_Floating vCurFloating in vListFloatingMenu)
			vCurFloating.ShowHideFloating (vPanelList[vCurrentNbr].vFloatingMenu.Contains(vCurFloating));
	}
	
	// Update is called once per frame
	void Update () {
		//check if we just started to click on GUI
		if (Input.GetMouseButtonDown (0)) {
			vOriginalPos = GetMousePosition ();
			IsMovingMenu = true;
			HandlingMenu ("All");
		} 
		//check if we have finished moving it
		else if (Input.GetMouseButtonUp (0)) {
			IsMovingMenu = false;

			//check if going right
			if (vDiff < 0 && (vDiff/0.5f) < -PercScreenToMove) {
				if (vCurrentNbr < vPanelList.Count - 1) {
					IsGoingTo = "Increase";
					vElapseTime = 0f;
					UpdateMMKTable(false);
					vCurrentNbr++;
					UpdateMMKTable(true);
					vDirFrom = 1; 
					vDirTo = 0;

					//update all the floating menus at once to show the right one
					UpdateFloatingMenus ();
				}
			}
			//check if going left
			else if (vDiff > 0 && (vDiff/0.5f) > PercScreenToMove) {
				if (vCurrentNbr > 0) {
					IsGoingTo = "Decrease";
					vElapseTime = 0f;
					UpdateMMKTable(false);
					vCurrentNbr--;
					UpdateMMKTable(true);
					vDirFrom = -1;
					vDirTo = 0;

					//update all the floating menus at once to show the right one
					UpdateFloatingMenus ();
				}
			} else {
				//NO change done, go back to current position
				IsGoingTo = "Same";
				vElapseTime = 0f;
				vDirFrom = 0;
				vDirTo = 0;
			}

			//make sure all the Icons are updated correctly
			UpdateIconsBtn ();
		}

		//check if were moving in the 
		if (IsMovingMenu && IsGoingTo == "")
		{
			//get the current mouse position
			Vector3 vMousePos = GetMousePosition ();

			//check if were using horizontal or vertical
			Vector2 vVectorDiff;
			if (vScrollType == MMK_ScrollType.Horizontal) {
				//get the diff to know where we are going
				vDiff = vMousePos.x - vOriginalPos.x;

				//calculate the diff on X
				vVectorDiff = new Vector2 (vDiff * vScreenDimX, 0f);
			} else {
				//get the diff to know where we are going
				vDiff = vMousePos.y - vOriginalPos.y;

				//calculate the diff on X
				vVectorDiff = new Vector2 (0f, vDiff * vScreenDimY);
			}

			//disable ALL other components
			if (Mathf.Abs(vDiff) >= vPrecisionMove) {
				foreach (MMK_Table vCurTable in vPanelList[vCurrentNbr].vSubTableList)
					vCurTable.StopMoving ();
			}

			//make doesn't move at all on the FIRST and LAST
			if (!((vDiff > 0 && vCurrentNbr == 0) || (vDiff < 0 && vCurrentNbr == vPanelList.Count - 1))) {
				//update the menu position
				vMainObj.GetComponent<RectTransform>().offsetMin = -GetMenuPos()+vVectorDiff*4;
				vMainObj.GetComponent<RectTransform>().offsetMax = -GetMenuPos()+vVectorDiff*4;
			}
		}

		//check if were moving
		if (IsGoingTo != "")
		{
			//update time
			vElapseTime += Time.deltaTime;
			if (vElapseTime >= vMenuSpeed) {
				//end the moving menu
				IsGoingTo = "";
				vElapseTime = 0f;

				//only show selected
				HandlingMenu ("Selected");

				//redraw the menu correctly
				UpdateMenuPos();

			} else {
				//check if were using horizontal or vertical
				Vector2 vVectorDiff;
				if (vScrollType == MMK_ScrollType.Horizontal)
					vVectorDiff = Vector2.Lerp (new Vector2 (vScreenDimX * (-vCurrentNbr+vDirFrom), 0f)+new Vector2((vDiff)*4*vScreenDimX, 0f), new Vector2 (vScreenDimX * (-vCurrentNbr + vDirTo), 0f), vElapseTime*(1f/vMenuSpeed));
				else
					vVectorDiff = Vector2.Lerp (new Vector2 (0f, vScreenDimY * (-vCurrentNbr+vDirFrom))+new Vector2(0f, vDiff*4*vScreenDimY), new Vector2 (0f, vScreenDimY * (-vCurrentNbr + vDirTo)), vElapseTime*(1f/vMenuSpeed));

				//update the menu position
				vMainObj.GetComponent<RectTransform> ().offsetMin = vVectorDiff;
				vMainObj.GetComponent<RectTransform> ().offsetMax = vVectorDiff;
			}
		}
	}
}
