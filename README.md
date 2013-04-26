PubNub client for Unity3D
=================

This is still pretty rough, and I'm not happy with the interface to it yet.
But it's fairly easy to use, and doesn't involve System.Threading;

Attach Pubnub.cs to an empty game object to setup the PubNub client in the inspector.
The object will persist across scenes, so you only need one.
It also has a static wrapper, so you can use it from any script.

C# example publish:

	Hashtable message = new Hashtable();
	message.Add("example","Hello, this is a message!");

	pubnub.publish("channel-name", message);


C# example subscribe:
	
	AsyncResponse callback = delegate(Hashtable response) {
		Debug.Log( pubnub.jsonEncode( response ) );
	};

	pubnub.subscribe("channel-name",callback);


If you place the source files into /Standard Assets/ or another [special directory](http://docs.unity3d.com/Documentation/ScriptReference/index.Script_compilation_28Advanced29.html), you can also use it from Unity JS:

	pubnub.publish("channel-name", {"test":"FromJS"});


I got the json.cs file from this repo: https://github.com/imersia/pubnub-unity3d

This lib still needs improvement, if you make any please let me know :)