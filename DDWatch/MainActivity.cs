using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.Wearable.Views;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Hardware;
using Android.Support.V4.View;
using Android.Locations;

// Google play API
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Wearable;
//using Android.Gms;
using Android.Gms.Common.Data;
using Android.Gms.Common;
using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;


using Java.Util.Concurrent;
using SoD_Xamarin_AndroidLibrary;


namespace DDWatch
{
	[Activity (Label = "DDWatch", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity,ISensorEventListener,GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener//,ILocationListener
	{
		//int count = 1;
		public SoD SoD;
		SensorManager sensor_manager;
		Sensor sensor;
		string tag = "ERWear";
		static readonly object _syncLock = new object();
		Button button;
		//private ScheduledExecutorService mScheduler; 
		string serverIP = "192.168.0.109";


		// START: Location Services 
		public void OnConnected (Bundle bundle)
		{

		}
		public void OnDisconnected (Bundle bundle)
		{

		}
		public void OnConnectionFailed (Bundle bundle)
		{

		}
		public void OnConnectionSuspended (int p0)
		{
			//DataLayerListenerService.LOGD (Tag, "OnConnectionSuspended(): Connection to Google API clinet was suspended");
		}

		public void OnConnectionFailed (Android.Gms.Common.ConnectionResult result)
		{
			//DataLayerListenerService.LOGD (Tag, "OnConnectionFailed(): Failed to connect, with result: " + result);
		}
		//GEO location
		//Location _currentLocation;
		//LocationManager _locationManager;

		//string _locationProvider;
	
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);


			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// MARK: Setup sensor + Location
			sensor_manager = (SensorManager)GetSystemService (Context.SensorService);
			sensor =  sensor_manager.GetDefaultSensor (SensorType.HeartRate);
			//InitializeLocationManager();

			// UI Delegate
			var v = FindViewById<WatchViewStub> (Resource.Id.watch_view_stub);
			button = FindViewById<Button> (Resource.Id.myButton);
			//this.SoD = new SoD (serverIP,3000,AndroidDeviceType.Watch);
			v.LayoutInflated += delegate {

				// Get our button from the layout resource,
				// and attach an event to it
				button = FindViewById<Button> (Resource.Id.myButton);
				button.Click += delegate {
					/*SoD.test();
					var notification = new NotificationCompat.Builder (this)
						.SetContentTitle ("Button tapped")
						.SetContentText ("Button tapped " + count++ + " times!")
						.SetSmallIcon (Android.Resource.Drawable.StatNotifyVoicemail)
						.SetGroup ("group_key_demo").Build ();

					var manager = NotificationManagerCompat.From (this);
					manager.Notify (1, notification);*/
					//this.SoD = new SoD (serverIP,3000,AndroidDeviceType.Watch);
					button.Text = "Check Notification!";
				};
			};
			//this.Window.AddFlags (WindowManagerFlags.KeepScreenOn);

		}
		// Mark: Setup Location
		/*void InitializeLocationManager(){
			_locationManager = (LocationManager) GetSystemService(LocationService);
			Criteria criteriaForLocationService = new Criteria
			{
				Accuracy = Accuracy.Fine
			};
			IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

			if (acceptableLocationProviders.Any())
			{
				_locationProvider = acceptableLocationProviders.First();
			}
			else
			{
				_locationProvider = string.Empty;
			}
			Log.Debug(tag, "Using " + _locationProvider + ".");
		}*/
		//private ScheduledThreadPoolExecutor mScheduler;
		TimerExampleState s;
		protected override void OnResume ()
		{
			base.OnResume ();
			LOG ("starting up !");
			//this.SoD.register ();
			// Location
			//InitializeLocationManager();
			//_locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
			// SoD

			this.SoD = new SoD (serverIP,3000,AndroidDeviceType.Watch);
			// Pulse Thread
			if (s == null) {
				s = new TimerExampleState();
				TimerCallback timerCallback = new TimerCallback (readHRData);
				Timer timer = new Timer (timerCallback, s,1000 , 1000);
				s.tmr = timer;
			}
		}



		void readHRData(Object state){
			TimerExampleState s = (TimerExampleState) state;
			s.counter++;
			Console.WriteLine("{0} Checking Status {1}.",DateTime.Now.TimeOfDay, s.counter);
			// get data of interest
			if (s.counter == 3) {
				//Console.WriteLine("disposing of timer...");

				if (sensor_manager.RegisterListener (this, sensor, SensorDelay.Normal)) {
					Log.Info (tag, "Successfully registered for heartrate events");
					Console.WriteLine ("Successfully registered for the heartrate events");
					//Thread.Sleep (5000);	// hang on for 5 seconds 
					//Console.WriteLine("{0} Checking Status {1}.\n",DateTime.Now.TimeOfDay, s.counter);
				} else {
					Log.Info (tag, "something is wrong with reading Heartrate sensor");
					Console.WriteLine ("something is wrong with reading heartrate sensors");
				}
				SoD.sendUpdate (117,currentHearBeat);
			} else if (s.counter == 10) {
				Log.Info (tag, "Unregister HeartRate sensor");
				Console.WriteLine ("Unregister HeartRate sensor");
				sensor_manager.UnregisterListener (this);
				Log.Info (tag, "______");
				Console.WriteLine ("______");
				// reset counter 
				s.counter = 0;
				Console.WriteLine ("10");
			} else if (s.counter > 3 && s.counter < 10) {
				SoD.sendUpdate (117,currentHearBeat);
			}
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			// Thread
			s.tmr.Dispose();
			s.tmr = null;
			// SOD
            
			this.SoD.disconnect();
			// Location
			//_locationManager.RemoveUpdates(this);
			// Heartrate
			sensor_manager.UnregisterListener (this);
			Console.WriteLine ("Unregistered for sensor events");
			Log.Info (tag, "Unregistered for sensor events");
		}

		private float[] array = new float[0];
		public float currentHearBeat = 0;
		public void OnSensorChanged(SensorEvent e)
		{
			lock (_syncLock) {
				Log.Info (tag, e.Values [0] + " - " + (int)e.Timestamp);
				Console.WriteLine (String.Join ("-", array) + " - " + (int)e.Timestamp);
				if (e.Values [0] != currentHearBeat) {
					currentHearBeat = e.Values [0];
					button.Text = currentHearBeat.ToString ();

				}
			}
		}
		public void OnAccuracyChanged(Sensor sensor,int accuracy)
		{
			
		}

		public void OnAccuracyChanged(Sensor sensor, SensorStatus status)
		{

		}
		// END: sensor delegate



		// MARK: Location delegates
		/*public async void OnLocationChanged(Location location) {
			_currentLocation = location;
			if (_currentLocation == null)
			{
				//_locationText.Text = "Unable to determine your location. Try again in a short while.";
				LOG ("Unable to determine your location. Try again in a short while.");
			}
			else
			{
				LOG (string.Format("{0:f6},{1:f6}", _currentLocation.Latitude, _currentLocation.Longitude));
				//_locationText.Text = string.Format("{0:f6},{1:f6}", _currentLocation.Latitude, _currentLocation.Longitude);
				Address address = await ReverseGeocodeCurrentLocation();
				syncAddressSoD(address);
			}
		}

		void syncAddressSoD(Address address)
		{
			if (address != null)
			{
				StringBuilder deviceAddress = new StringBuilder();
				for (int i = 0; i < address.MaxAddressLineIndex; i++)
				{
					deviceAddress.AppendLine(address.GetAddressLine(i));
				}
				// Remove the last comma from the end of the address.
				this.LOG (deviceAddress.ToString());
			}
			else
			{
				//_addressText.Text = "Unable to determine the address. Try again in a few minutes.";
				this.LOG("Unable to determine the address. Try again in a few minutes.");
			}
		}
		async Task<Address> ReverseGeocodeCurrentLocation()
		{
			Geocoder geocoder = new Geocoder(this);
			IList<Address> addressList =
				await geocoder.GetFromLocationAsync(_currentLocation.Latitude, _currentLocation.Longitude, 10);

			Address address = addressList.FirstOrDefault();
			return address;
		}


		public void OnProviderDisabled(string provider) {}

		public void OnProviderEnabled(string provider) {}

		public void OnStatusChanged(string provider, Availability status, Bundle extras) {}
	*/
		public void LOG(string  logstring){
			Log.Debug(tag, logstring);
			Console.WriteLine (logstring);
		}
	}
	class TimerExampleState {
		public int counter = 0;
		public Timer tmr;
	}

}




