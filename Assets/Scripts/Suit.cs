using UnityEngine;
using System.Collections;

public class Suit : MonoBehaviour {

	private int count;
	private GameObject[] buttons;

	void Start () {
		/*count = transform.childCount;
		buttons = new GameObject[count];
		for(int i = 0; i < count; i++) {
			buttons [i] = transform.GetChild (i) as GameObject;
		}*/
		suitLayout ();
	}

	//调整布局
	public void suitLayout() {
		count = transform.childCount;
		buttons = new GameObject[count];
		for(int i = 0; i < count; i++) {
			buttons [i] = transform.GetChild (i).gameObject;
		}
		switch(count) {
		case 5:
			buttons [3].transform.localPosition = new Vector3 (-295.0f, -210.0f, 0.0f);
			buttons [4].transform.localPosition = new Vector3 (295.0f, -210.0f, 0.0f);
			break;
		case 6:
			buttons [3].transform.localPosition = new Vector3 (-600.0f, -210.0f, 0.0f);
			buttons [4].transform.localPosition = new Vector3 (0.0f, -210.0f, 0.0f);
			buttons [5].transform.localPosition = new Vector3 (600.0f, -210.0f, 0.0f);
			break;
		}
	}

}
