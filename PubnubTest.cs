using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class PubnubTest : MonoBehaviour {
	
	private static  List<string> messages = new List<string>();
	
	void Start () {
	}
	void Update () {
	}
	
	void OnGUI() {
		int k = 1;
		if (GUI.Button (new Rect (Screen.width/2-200,60 + 60*k,400,50), "Subscribe to A") ) {
		    pubnub.subscribe("A",
				(Hashtable response)=>{ // callback
					messages.Insert(0, "A: " + pubnub.jsonEncode( response ) );
				},
				(Hashtable response)=>{ // on connected
					messages.Insert(0, "A: Connected!" );
				},
				(Hashtable response)=>{ // on error
					messages.Insert(0, "A: Error, retrying in 1 second!" );
				});
		}
		k++;
		if (GUI.Button (new Rect (Screen.width/2-200,60 + 60*k,400,50), "Subscribe to B") ) {
			 pubnub.subscribe("B",
				(Hashtable response)=>{ // callback
					messages.Insert(0, "B: " + pubnub.jsonEncode( response ) );
				},
				(Hashtable response)=>{ // on connected
					messages.Insert(0, "B: Connected!" );
				},
				(Hashtable response)=>{ // on error
					messages.Insert(0, "B: Error, retrying in 1 second!" );
				});
		}
		k++;
		if (GUI.Button (new Rect (Screen.width/2-200,60 + 60*k,400,50), "Subscribe to C") ) {
			 pubnub.subscribe("C",
				(Hashtable response)=>{ // callback
					messages.Insert(0, "C: " + pubnub.jsonEncode( response ) );
				},
				(Hashtable response)=>{ // on connected
					messages.Insert(0, "C: Connected!" );
				},
				(Hashtable response)=>{ // on error
					messages.Insert(0, "C: Error, retrying in 1 second!" );
				});
		}
		k++;
		if (GUI.Button (new Rect (Screen.width/2-200,60 + 60*k,400,50), "Publish to A") ) {
			Hashtable message = new Hashtable();
			message.Add("msg",Random.value.ToString());
			pubnub.publish ("A", message);
		}
		k++;
		if (GUI.Button (new Rect (Screen.width/2-200,60 + 60*k,400,50), "Publish to B") ) {
			Hashtable message = new Hashtable();
			message.Add("msg",Random.value.ToString());
			pubnub.publish ("B", message);
		}
		k++;
		if (GUI.Button (new Rect (Screen.width/2-200,60 + 60*k,400,50), "Publish to C") ) {
			Hashtable message = new Hashtable();
			message.Add("msg",Random.value.ToString());
			pubnub.publish ("C", message, (Hashtable response)=>{
				Debug.Log ("SENT");
			});
		}
		
		k = 1;
		List<string> messagesCopy = new List<string>(messages);
		foreach (string message in messagesCopy) {
			GUI.Box (new Rect (20, Screen.height-30 - 40*k, 250, 30), message);
			k = k +1;
		}
	}
}
