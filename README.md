PubNub client for Unity3D
=================

This is still pretty rough, but it's fairly easy to use.

Note though, that for some reason it doesn't work correctly in the editor.
It does work in builds, but if you can fix it so it works in the editor I would be grateful.

Attach Pubnub.cs to an empty game object to setup the PubNub client in the inspector.
The object will persist across scenes, so you only need one.
It also has a static wrapper, so you can use it from any script.

C# example publish:

	Hashtable message = new Hashtable();
	message.Add("example","Hello, this is a message!");

	pubnub.publish("channel-name", message);


C# example subscribe:

	pubnub.subscribe("channel-name",(Hashtable response)=>{
		Debug.Log( pubnub.jsonEncode( response ) );
	});


If you place the source files into /Standard Assets/ or another [special directory](http://docs.unity3d.com/Documentation/ScriptReference/index.Script_compilation_28Advanced29.html), you can also use it from Unity JS:

	pubnub.publish("channel-name", {"test":"FromJS"});


I got the json.cs file from this repo: https://github.com/imersia/pubnub-unity3d

This lib obviously still needs improvement, so if you make any please let me know :)