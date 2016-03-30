using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

using Java.Util.Concurrent;
using SoD_Xamarin_AndroidLibrary;


namespace DDWatch
{
	[Activity (Label = "DDWatch", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity,ISensorEventListener
	{
		//int count = 1;
		public SoD SoD;
		SensorManager sensor_manager;
		Sensor sensor;
		string tag = "ERWear";
		static readonly object _syncLock = new object();
		Button button;
		//private ScheduledExecutorService mScheduler; 
		string serverIP = "192.168.0.106";

		//GEO location
		Location _currentLocation;
		LocationManager _locationManager;

		string _locationProvider;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Marker: Socket 		


			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// MARK: Setup sensor + Location
			sensor_manager = (SensorManager)GetSystemService (Context.SensorService);
			sensor =  sensor_manager.GetDefaultSensor (SensorType.HeartRate);
			InitializeLocationManager();

			var v = FindViewById<WatchViewStub> (Resource.Id.watch_view_stub);
			button = FindViewById<Button> (Resource.Id.myButton);



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
					button.Text = "Check Notification!";
				};
			};

		}
		// Mark: Setup Location
		void InitializeLocationManager(){
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
		}
		//private ScheduledThreadPoolExecutor mScheduler;
		TimerExampleState s;
		protected override void OnResume ()
		{
			base.OnResume ();
			//Reconnect SoD
			this.SoD = new SoD (serverIP,3000,AndroidDeviceType.Watch);
			s = new TimerExampleState();
			TimerCallback timerCallback = new TimerCallback (readHRData);
			s = new TimerExampleState();
			Timer timer = new Timer (timerCallback, s,1000 , 1000);
			s.tmr = timer;

			/*if (sensor_manager.RegisterListener (this, sensor, SensorDelay.Normal)) {
				Log.Info (tag, "Successfully registered for heartrate events");
				Console.WriteLine ("Successfully registered for the heartrate events");
			} else {
				Log.Info (tag, "something is wrong with reading Heartrate sensor");
				Console.WriteLine ("something is wrong with reading heartrate sensors");
			}*/

			/*while (s.tmr != null)
				Thread.Sleep(0);
			Console.WriteLine("Timer example done.");*/
		}



		void readHRData(Object state){
			//TimerExampleState s = (TimerExampleState) state;
			//s.counter++;
			//Console.WriteLine("{0} Checking Status {1}.",DateTime.Now.TimeOfDay, s.counter);
			TimerExampleState s = (TimerExampleState) state;
			s.counter++;
			Console.WriteLine("{0} Checking Status {1}.",DateTime.Now.TimeOfDay, s.counter);
			if (s.counter == 5) {
				SoD.sendUpdate (117,currentHearBeat);
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
				Console.WriteLine ("5");

			}
			if (s.counter == 10) {
				Log.Info (tag, "Unregister HeartRate sensor");
				Console.WriteLine ("Unregister HeartRate sensor");
				sensor_manager.UnregisterListener (this);
				Log.Info (tag, "______");
				Console.WriteLine ("______");
				// reset counter 
				s.counter = 0;
				Console.WriteLine ("10");
				//s.tmr.Dispose();
				//s.tmr = null;
			}



			/*
			if(sensor!=null){
				if (sensor_manager.RegisterListener (this, sensor, SensorDelay.Normal)) {
					Log.Info (tag, "Successfully registered for heartrate events");
					Console.WriteLine ("Successfully registered for the heartrate events");
					Thread.Sleep (5000);	// hang on for 5 seconds 
					//Console.WriteLine("{0} Checking Status {1}.\n",DateTime.Now.TimeOfDay, s.counter);
					Log.Info (tag, "Unregister HeartRate sensor");
					Console.WriteLine ("Unregister HeartRate sensor");
					sensor_manager.UnregisterListener (this);
					Log.Info (tag, "______");
					Console.WriteLine ("______");
				} else {
					Log.Info (tag, "something is wrong with reading Heartrate sensor");
					Console.WriteLine ("something is wrong with reading heartrate sensors");
				}
			}*/
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			sensor_manager.UnregisterListener (this);
			Console.WriteLine ("Unregistered for sensor events");
			Log.Info (tag, "Unregistered for sensor events");
			s.tmr.Dispose();
			s.tmr = null;
			/*if (Log.IsLoggable (TAG, LogPriority.Debug)) {
				Log.Debug (TAG, "Unregistered for sensor events");
			}*/
		}

		private float[] array = new float[0];
		public float currentHearBeat = 0;
		public void OnSensorChanged(SensorEvent e)
		{
			/*lock (_syncLock)
			{*/
				Log.Info (tag, e.Values [0] + " - " +(int)e.Timestamp);
				Console.WriteLine (String.Join("-",array) + " - " + (int)e.Timestamp);
				if (e.Values [0] != currentHearBeat) {
					currentHearBeat = e.Values [0];
					button.Text = currentHearBeat.ToString();
					//TODO: Send to SoD
					SoD.sendUpdate (117,90);
				}
			//}
		}

		public void OnAccuracyChanged(Sensor sensor,int accuracy)
		{

		}

		public void OnAccuracyChanged(Sensor sensor, SensorStatus status)
		{

		}
		

	}
	class TimerExampleState {
		public int counter = 0;
		public Timer tmr;
	}
}




