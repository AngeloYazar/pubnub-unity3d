/*
PubNub client by Angelo Yazar, based PubNub's lua client: https://github.com/pubnub/pubnub-api/tree/master/lua-corona
Attach this script to an empty game object to setup pubnub, it will persist across scenes.

>IMPORTANT< 
For some reason, subscribing to more than one channel locks up pubnub in the editor, but works fine in builds.
If you can figure out why, please let me know!
*/

using UnityEngine;
using System;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using JSONEncoderDecoder;

public delegate void AsyncResponse( Hashtable response );

public class Pubnub : MonoBehaviour {
	public static bool initialized = false;
	public string subscribe_key = "demo";
	public string publish_key = "demo";
	public string secret_key;
	public bool ssl = false;
	public bool debugging = true;
	
	private string origin = "pubsub.pubnub.com/";
	private bool secure = false;
	private HMACSHA256 sha256;
	private string uuid = Guid.NewGuid().ToString();
	
	public Hashtable subscriptions;
	private List<IEnumerator> pendingRoutines = new List<IEnumerator>();
	
	private void QueueCoroutine( IEnumerator co ) {
		pendingRoutines.Add( co );
	}
	
	void debug(string message) {
		if(debugging) {
			Debug.Log ("PubNub: " + message);
		}
	}
	
	public string encode(string str) {
		return Uri.EscapeDataString(str);
	}
	
	public void init() {
		System.Net.ServicePointManager.DefaultConnectionLimit = 1000;
		secure = secret_key.Length > 0;
		subscriptions = new Hashtable();
		origin = (ssl?"https://":"http://") + origin;
		
		if(secure) {
			sha256 = new HMACSHA256( System.Text.Encoding.UTF8.GetBytes(secret_key) );
		}
		pubnub.init(this);
		initialized = true;
	}

	private void _request(string[] request, AsyncResponse callback = null, string query = "") {
		WebClient client = new WebClient ();
		string url = origin + String.Join("/", request) + "?" + query;
		debug( url );
		client.Headers.Add("V","1.0");
		client.Headers.Add("User-Agent","Unity3D");
		client.DownloadStringCompleted += (s,e) => {
			if( callback != null ) {
				Hashtable response = new Hashtable();
				if(e.Cancelled != false || e.Error != null) {
					response.Add("error", true);
				}
				else {
					response.Add("message", (ArrayList)JSON.JsonDecode((string)e.Result));
				}
				callback( response );
			}
			client.Dispose();
		};
			
		client.DownloadStringAsync(new Uri( url ));
	}
	
	public void publish(string channel, object messageObject, AsyncResponse callback = null) {
		string message = JSON.JsonEncode( messageObject );
		string signature = "0";
		
		if(secure) {
			string[] plaintext = {publish_key,subscribe_key,secret_key,channel,message};
			byte[] hash = sha256.ComputeHash( System.Text.Encoding.UTF8.GetBytes(String.Join("/",plaintext)));
			signature = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
		}	
		
		string[] request = {"publish",publish_key,subscribe_key,signature,encode(channel),"0",encode(message)};
		_request(request, callback);
	}
	
	public void subscribe(string channel, AsyncResponse callback, AsyncResponse onConnected = null, AsyncResponse onError = null) {
		if( !subscriptions.ContainsKey(channel) ) {
			subscriptions.Add(channel, new Hashtable());
		}
		
		Hashtable subscription = (Hashtable)subscriptions[channel];
		if(subscription["connected"] != null) {
			debug( "Already connected to " + channel );
			return;
		}
		
		subscription["onError"] = onError;
		subscription["onConnected"] = onConnected;
		subscription["callback"] = callback;
		subscription["timetoken"] = "0";
		subscription.Add("connected",true);
		subscription.Remove("first");
		
		string[] request = {"subscribe",subscribe_key,encode(channel),"0",(string)subscription["timetoken"]};
		StartCoroutine( substabizel(channel, request, "uuid="+uuid) );
	}
	
	private IEnumerator substabizel(string channel,string[] request, string query, YieldInstruction delay = null) {
		yield return delay;
		Hashtable subscription = (Hashtable)subscriptions[channel];
		
		if(!subscription.ContainsKey("connected")) { return false; }
		
		AsyncResponse onComplete = delegate(Hashtable response) {
			if(!subscription.ContainsKey("connected")) { return; }
			
			if(subscription["onConnected"] != null && !subscription.ContainsKey("first")) {
				subscription.Add("first",true);
				((AsyncResponse)subscription["onConnected"])(response);
			}
			
			if(response["error"]!=null) {
				AsyncResponse timeCallback = delegate(Hashtable timeResponse) {
					if(response["error"] != null) {
						if(subscription["onError"] != null) {
							((AsyncResponse)subscription["onError"])(timeResponse);
						}
						QueueCoroutine( substabizel(channel, request, query, new WaitForSeconds(1.0f)) );
					}
					else {
						QueueCoroutine( substabizel(channel, request, query, new WaitForSeconds(0.1f)) );
					}
				};
				time(timeCallback);
			}
			else {
				ArrayList parsedResponse = (ArrayList)((ArrayList)response["message"]);
				subscription["timetoken"] = (string)parsedResponse[1];
				ArrayList messages = (ArrayList)parsedResponse[0];
				foreach(object message in messages) {
					((AsyncResponse)subscription["callback"])((Hashtable)message);
				}
				QueueCoroutine( substabizel(channel, request, query, new WaitForSeconds(0.033f)) );
			}
		};
		request[4] = (string)subscription["timetoken"];
		_request(request, onComplete, query);
	}
	
	public void unsubscribe(string channel) {
		if( !subscriptions.ContainsKey(channel) ) { return; }
		Hashtable subscription = (Hashtable)subscriptions[channel];
		subscription.Remove("connected");
		subscription.Remove("first");
	}
	
	public void presence(string channel, AsyncResponse callback, AsyncResponse onConnected = null, AsyncResponse onError = null) {
		channel += "-pnpres";
		subscribe(channel,callback,onConnected,onError);
	}
	
	public void here_now(string channel, AsyncResponse callback) {
		string[] request = {"v2","presence","sub-key",subscribe_key,"channel",encode(channel)};
		_request(request,callback);
	}
	
	public void history(string channel, AsyncResponse callback, int limit = 10 ) {
		string[] request = {"history",subscribe_key,encode(channel),"0",limit.ToString()};
		_request(request,callback);
	}
	
	public void detailedHistory(string channel, AsyncResponse callback, long start = -1, long stop = -1, long count = 10, bool reverse = false ) {
		string[] request = {"v2","history","sub-key",subscribe_key,"channel",encode(channel)};
		string query = "reverse=" + reverse.ToString() + "&count=" + count.ToString();
		if( start >= 0 ) {
			query += "&" + "start=" + start.ToString();
		}
		if( stop >= 0 ) {
			query += "&" + "stop=" + stop.ToString();
		}
		_request(request,callback,query);
	}
	
	public void time(AsyncResponse callback) {
		string[] request = { "time", "0" };
		_request(request,callback);
	}
	
	void Awake() {
		if(!initialized) {
			DontDestroyOnLoad(gameObject);
			init();
		}
		else {
			Destroy(gameObject);
		}
	}
	
	void Update() {
		if( pendingRoutines.Count > 0 ) {
			foreach( IEnumerator co in pendingRoutines ) {
				StartCoroutine( co );
			}
			pendingRoutines.Clear();
		}
	}
}

/*This is just so you can use pubnub.method(); from anywhere.*/
public static class pubnub {
	private static Pubnub instance;
	public static void init(Pubnub inst) { instance = inst; }
	public static void publish(string channel, object messageObject, AsyncResponse callback = null) {
		instance.publish(channel,messageObject,callback);
	}
	public static void subscribe(string channel, AsyncResponse callback, AsyncResponse onConnected = null, AsyncResponse onError = null) {
		instance.subscribe(channel,callback,onConnected,onError);
	}
	public static void unsubscribe(string channel) {
		instance.unsubscribe(channel);
	}
	public static void presence(string channel, AsyncResponse callback, AsyncResponse onConnected = null, AsyncResponse onError = null) {
		instance.presence(channel,callback,onConnected,onError);
	}
	public static void here_now(string channel, AsyncResponse callback) {
		instance.here_now(channel,callback);
	}
	public static void history(string channel, AsyncResponse callback, int limit = 10 ) {
		instance.history(channel,callback,limit);
	}
	public static void detailedHistory(string channel, AsyncResponse callback, long start = -1, long stop = -1, long count = 10, bool reverse = false ) {
		instance.detailedHistory(channel,callback,start,stop,count,reverse);
	}
	public static string jsonEncode( object data ) {
		return JSON.JsonEncode( data );
	}
	public static object jsonDecode( string data ) {
		return JSON.JsonDecode( data );
	}
	public static string uuid() {
		return Guid.NewGuid().ToString();
	}
}