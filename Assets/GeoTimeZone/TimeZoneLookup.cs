using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Linq;

namespace GeoTimeZone
{
	public class TimeZoneLookup : MonoBehaviour
	{
		public UnityEvent OnReady;
		[HideInInspector]
		public bool dataIsReady = false;

		string tzPath, tzlPath;



		List<string> tzlData;
		List<string> tzData;

		public TimeZoneResult GetTimeZone(double latitude, double longitude)
		{
			TimeZoneResult res;

			Debug.LogFormat ("Getting Timezone for {0} {1}", latitude, longitude);

			var geohash = Geohash.Encode(latitude, longitude, 5);

			var lineNumber = GetTzDataLineNumbers(geohash);

			var timeZones = GetTzsFromData(lineNumber).ToList();

			if (timeZones.Count == 1) {
				res = new TimeZoneResult { Result = timeZones [0] };
			} else if (timeZones.Count > 1) {
				res = new TimeZoneResult { Result = timeZones [0], AlternativeResults = timeZones.Skip (1).ToList () };
			} else {

				var offsetHours = CalculateOffsetHoursFromLongitude (longitude);
				res = new TimeZoneResult { Result = GetTimeZoneId (offsetHours) };
			}

			Debug.LogFormat ("Result: {0}", res.Result);

			return res;
		}

		private IEnumerable<string> GetTzsFromData(IEnumerable<int> lineNumbers)
		{
			return lineNumbers.OrderBy(x => x).Select(x => tzlData[x - 1]);
		}

		private int CalculateOffsetHoursFromLongitude(double longitude)
		{
			var dir = longitude < 0 ? -1 : 1;
			var posNo = System.Math.Sqrt(System.Math.Pow(longitude, 2));
			if (posNo <= 7.5)
				return 0;

			posNo -= 7.5;
			var offset = posNo / 15;
			if (posNo % 15 > 0)
				offset++;

			return dir * (int)System.Math.Floor(offset);
		}

		private string GetTimeZoneId(int offsetHours)
		{
			if (offsetHours == 0) {
				return "UTC";
			}

			string reversed = (offsetHours >= 0 ? "-" : "+") + System.Math.Abs(offsetHours);
			return "Etc/GMT" + reversed;
		}

		private IEnumerable<int> GetTzDataLineNumbers(string geohash)
		{
			int seeked = SeekTimeZoneFile(geohash);
			if (seeked == 0) {
				return new List<int> ();
			}

			int min = seeked, max = seeked;
			string seekedGeohash = tzData[seeked].Substring(0, 5);

			while (true)
			{
				string prevGeohash = tzData[min - 1].Substring(0, 5);
				if (seekedGeohash == prevGeohash)
					min--;
				else
					break;
			}

			while (true)
			{
				string nextGeohash = tzData[max + 1].Substring(0, 5);
				if (seekedGeohash == nextGeohash)
					max++;
				else
					break;
			}

			List<int> lineNumbers = new List<int>();
			for (int i = min; i <= max; i++)
			{
				string str = tzData[i].Substring (5);
				Debug.Log ("DEBUG STRING: " + str);
				var lineNumber = int.Parse(str);
				lineNumbers.Add(lineNumber);
			}

			return lineNumbers;
		}

		private int SeekTimeZoneFile(string hash)
		{
			int min = 1;
			int max = tzData.Count;
			bool converged = false;

			while (true)
			{
				int mid = ((max - min) / 2) + min;
				string midLine = tzData[mid];

				for (int i = 0; i < hash.Length; i++)
				{
					if (midLine[i] == '-')
					{
						return mid;
					}

					if (midLine[i] > hash[i])
					{
						max = mid == max ? min : mid;
						break;
					}
					if (midLine[i] < hash[i])
					{
						min = mid == min ? max : mid;
						break;
					}

					if (i == 4)
					{
						return mid;
					}

					if (min == mid)
					{
						min = max;
						break;
					}
				}

				if (min == max)
				{
					if (converged)
						break;

					converged = true;
				}
			}
			return 0;
		}

		/*
		 * DATA LOADING
		 * 
		 * Coroutine is used to check status and call event without tying up the ui
		 * Actual data loading is via threads
		 * Much larger data sets could be used (like real shapefile) without blocking
		 */

		public void LoadData() {
			StartCoroutine (LoadDataCoroutine ());
		}

		IEnumerator LoadDataCoroutine() {

			tzPath = System.IO.Path.Combine (Application.streamingAssetsPath, "GeoTimeZone-TZ.dat");
			tzlPath = System.IO.Path.Combine (Application.streamingAssetsPath, "GeoTimeZone-TZL.dat");

			Thread loadThread = new Thread(new ThreadStart(LoadDataThread));
			loadThread.Start ();

			while (!dataIsReady) {
				yield return null;
			}

			Debug.LogFormat ("TZL Data Contains {0} Lines", tzlData.Count);
			Debug.LogFormat ("TZ Data Contains {0} Lines", tzData.Count);



			if (OnReady != null) {
				OnReady.Invoke ();
			}

		}

		void LoadDataThread() {
			tzlData = new List<string> (System.IO.File.ReadAllLines (tzlPath));
			tzData = new List<string> (System.IO.File.ReadAllLines (tzPath));


			dataIsReady = true;
		}


	}
}
