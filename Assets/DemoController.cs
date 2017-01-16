using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DemoController : MonoBehaviour {
	public GeoTimeZone.TimeZoneLookup tzLookup;
	public InputField latInput, lonInput;
	public Text  status, results;
	public GameObject waitWindow;


	// Use this for initialization
	void Start () {
		latInput.text = "31.92";
		lonInput.text = "35.03";
		results.text = "";
		status.text = "";

		tzLookup.LoadData ();
	}

	public void TimezoneDataReady() {
		waitWindow.SetActive (false);

		FindLocation ();
	}

	public void FindLocation() {
		float lat, lon;

		results.text = "";
		status.text = "";
		if (!float.TryParse (latInput.text, out lat)) {
			status.text = "Bad Latitude Input!";
			return;
		}

		if (!float.TryParse (lonInput.text, out lon)) {
			status.text = "Bad Longitude Input!";
			return;
		}

		Debug.LogFormat ("Finding location for {0} {1}", lat, lon);
		status.text = "Please wait...";

		GeoTimeZone.TimeZoneResult tzResult = tzLookup.GetTimeZone (lat, lon);
		string tzId = "";

		if (tzResult != null) {
			tzId = tzResult.Result.ToString ();
		}
		if (tzId != "") {
			results.text = tzId;
			status.text = "";
			Debug.Log (tzId);
		} else {
			status.text = "Unable to get timezone Id!";
			Debug.Log (status.text);
		}



	}
}
