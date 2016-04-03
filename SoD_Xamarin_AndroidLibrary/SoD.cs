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
		public bool registered;
		public int uniqueDeviceID;
		public SoD (string host,int port, AndroidDeviceType type)
		{	
			if (this.socket != null) {
				this.socket.Disconnect ();
				this.socket.Dispose ();
			}
			this.socket = new SocketIO (host, port);
			this.deviceType = type;
			//Set listeners
			socket.On("assignUniqueID", (data) => {               //call this lambda when a message named "MessageReceived"
				Console.WriteLine (data ["uniqueDeviceID"]);     //is emitted from the server
				/*var obj = (JObject)JsonConvert.DeserializeObject(data);
				Type type = typeof(int);
				var i1 = System.Convert.ChangeType(obj["id"].ToString(), type);*/
				this.uniqueDeviceID =  Int32.Parse(data["uniqueDeviceID"].ToString());
			});
			register ();
		}
		public async void register(){
			if (!this.socket.Connected) {
				var connectionStatus = await socket.ConnectAsync ();
				if (connectionStatus == SocketIO.ConnectionStatus.Connected) {
					switch (this.deviceType) {
					case AndroidDeviceType.Glass:
						registerAsGoogleGlass ();
						break;
					case AndroidDeviceType.Watch:
						registerAsAndroidWatch ();
						break;
					default:
						Console.WriteLine ("Unkonwn type: " + this.deviceType);
						break;
					}

				} else {
					Console.WriteLine ("Websocket failed to connect to the server");
				}
			} else {
				Console.WriteLine ("Socket is connected. No need to register.");
			}

		}
		public void disconnect(){
			if (this.socket.Connected && this.socket!=null) {
				this.registered = false;
				this.socket.Disconnect();
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
			registered = true;
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
			this.registered = true;
		}
		public void sendUpdate(int personID,float heartbeat){
			if (this.socket.Connected&&this.registered) {
				Console.WriteLine (personID+" - "+heartbeat);
				socket.Emit ("ERPersonUpdate", new Object[]{new ERPersonUpdateEncap(){
						heartbeat = heartbeat,
						personID = personID
					}
				});
			}
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

