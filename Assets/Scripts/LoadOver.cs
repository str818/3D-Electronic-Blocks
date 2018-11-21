using UnityEngine;
using System.Collections;

public class LoadOver : MonoBehaviour {

	public GameObject Panel;
	public GameObject Button;
	public Animation loadoverAnimation;

	void Start () {
		loadoverAnimation = transform.GetComponent<Animation> ();
		loadoverAnimation.Play ();
	}

	void Update () {
		if (!loadoverAnimation.IsPlaying("LoadOver")) {
			Panel.SetActive (false);
			Button.SetActive (true);
			gameObject.SetActive (false);
		}
	}

}
