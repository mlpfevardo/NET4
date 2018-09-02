using UnityEngine;
using System.Collections;

public class MMK_Cell : MonoBehaviour {

	//check if we are selecting this one
	public bool IsSelected = false;

	public GameObject SelectedObj; 

	// Use this for initialization
	void Start () {
		if (!IsSelected)
			UnSelectCell ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//disable the selection on this cells
	public void UnSelectCell()
	{
		//disable selected obj
		if (SelectedObj != null)
			SelectedObj.SetActive (false);
		
		//be sure to be below the other cells
		transform.SetAsFirstSibling ();
	}

	public void SelectCell()
	{
		//unselect all the other cells
		foreach (MMK_Cell vCurCell in transform.parent.GetComponentsInChildren<MMK_Cell>())
			vCurCell.UnSelectCell ();

		//be sure to be below the other cells
		transform.SetAsLastSibling();

		//enable selected obj
		if (SelectedObj != null)
			SelectedObj.SetActive (true);
	}
}
