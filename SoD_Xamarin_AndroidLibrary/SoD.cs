using System;
using Xamarin.Socket.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace SoD_Xamarin_AndroidLibrary
{
	public class SoD
	{
		public SocketIO socket;	
		public AndroidDeviceType deviceType;
		public SoD (string host,int port, AndroidDeviceType type)
		{	
			socket = new SocketIO (host, port);
			this.deviceType = type;
			register ();
		}
		public async void register(){
			var connectionStatus = await socket.ConnectAsync ();

			if (connectionStatus == SocketIO.ConnectionStatus.Connected) {
				switch (this.deviceType) {
				case AndroidDeviceType.Glass:
					registerAsGoogleGlass ();
					break;
				case AndroidDeviceType.Watch:
					registerAsAndroidWatch();
					break;
				default:
					Console.WriteLine ("Unkonwn type: " + this.deviceType);
					break;
				}

			} else {
				Console.WriteLine ("Websocket failed to connect to the server");
			}
		}
		public void test(){
			//var list = new object [] { 1, "randomString", 3.4f, new Foo () { Bar = "baz"} };
			//Foo aFoo = new Foo (){ Bar = "baz" };

		}
		public void registerAsAndroidWatch(){
			socket.Emit ("registerDevice", new Object[] {new AndroidWatch () {
					name = "Android Watch",
					deviceType = "AndroidWatch",
					width = 0.1,
					height=0.1,
					depth = 0.1,
					stationary = false
				} });
		}
		public void registerAsGoogleGlass(){
			socket.Emit ("registerDevice", new Object[] {new GoogleGlass () {
					name = "GoogleGlass",
					deviceType = "Glass",
					width = 0.1,
					height=0.1,
					depth = 0.1,
					stationary = false
				} });
		}
		public void sendUpdate(int personID,float heartbeat){
			Console.WriteLine (personID+" - "+heartbeat);
			socket.Emit ("ERPersonUpdate", new Object[]{new ERPersonUpdateEncap(){
					heartbeat = heartbeat,
					personID = personID
			}
			});
		}
	}

	[JsonObject(MemberSerialization.OptIn)]
	public class ERPersonUpdateEncap
	{
		[JsonProperty]
		public float heartbeat { get; set;} 
		[JsonProperty]
		public int personID { get; set;} 

	}
	[JsonObject(MemberSerialization.OptIn)]
	public class AndroidWatch: AndroidWear
	{
		[JsonProperty]
		public string name { get; set;} 
		[JsonProperty]
		public string deviceType { get; set;} 
		[JsonProperty]
		public double width { get; set;} 
		[JsonProperty]
		public double height { get; set;} 
		[JsonProperty]
		public double depth { get; set;} 
		[JsonProperty]
		public bool stationary {get;set;}
	}

	[JsonObject(MemberSerialization.OptIn)]
	public class GoogleGlass: AndroidWear
	{
		[JsonProperty]
		public string name { get; set;} 
		[JsonProperty]
		public string deviceType { get; set;} 
		[JsonProperty]
		public double width { get; set;} 
		[JsonProperty]
		public double height { get; set;} 
		[JsonProperty]
		public double depth { get; set;} 
		[JsonProperty]
		public bool stationary {get;set;}
	}
	public interface AndroidWear{
		[JsonProperty]
		string name { get; set;} 
		[JsonProperty]
		string deviceType { get; set;} 
		[JsonProperty]
		double width { get; set;} 
		[JsonProperty]
		double height { get; set;} 
		[JsonProperty]
		double depth { get; set;} 
		[JsonProperty]
		bool stationary {get;set;}
	}
	public enum AndroidDeviceType{
		Glass,Watch
	}
}

