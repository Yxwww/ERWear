using System;
using System.Drawing;
//using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using Xamarin.Socket.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Hardware;
using Android.OS;
using Android.Glass.App;
using Android.Glass.Widget;
using Android.Provider;
using Android.Util;
using Gesture = Android.Glass.Touchpad.Gesture;
using GestureDetector = Android.Glass.Touchpad.GestureDetector;
using SoD_Xamarin_AndroidLibrary;
using Android.Locations;

using Java.IO;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;


namespace HelloGlassWorld
{
	[Activity (Label = "ERGlass", Icon = "@drawable/Icon", MainLauncher = true, Enabled = true)]
	[IntentFilter (new String[]{ "com.google.android.glass.action.VOICE_TRIGGER" })]
	[MetaData ("com.google.android.glass.VoiceTrigger", Resource = "@xml/voicetriggerstart")]
	public class MainActivity : Activity , GestureDetector.IBaseListener,ILocationListener
	{
		public string serverIP = "192.168.0.136";
        SoD sod;
        string tag = "ERWear";
        private GestureDetector mGestureDetector;
        private bool mIsBound = false;
        //private CameraDemoLocalService cameraDemoLocalService = null;

        private string lat, lng;

        // Camera 
        Preview mPreview;
        Camera mCamera;
        int numberOfCameras;
        int cameraCurrentlyLocked;

        // The first rear facing camera
        int defaultCameraId;


        //Location 
        LocationManager locMgr;
        string locationProvider;
        Criteria locationCriteria = new Criteria();


		// The project requires the Google Glass Component from
		// https://components.xamarin.com/view/googleglass
		// so make sure you add that in to compile succesfully.
		protected override void OnCreate (Bundle bundle)
		{
            // Camera
            // Hide the window title and go fullscreen.
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.AddFlags(WindowManagerFlags.Fullscreen);

			base.OnCreate (bundle);


            sod = new SoD(serverIP, 3000, AndroidDeviceType.Glass);
            sod.socket.On("remoteNavigation", (data) =>
            {               //call this lambda when a message named "MessageReceived"
                System.Console.WriteLine(data["payload"]["lat"]);     //is emitted from the server
                /*var obj = (JObject)JsonConvert.DeserializeObject(data);
                Type type = typeof(int);
                var i1 = System.Convert.ChangeType(obj["id"].ToString(), type);*/
                //this.uniqueDeviceID = Int32.Parse(data["uniqueDeviceID"].ToString());
                Intent intent = new Intent(Intent.ActionView);
                intent.SetData(Uri.Parse("google.navigation:q=" + data["payload"]["lat"].ToString() + "," + data["payload"]["lng"].ToString()));
                StartActivity(intent);
            });
            lat = "";
            lng = "";
            sod.socket.On("broadcastPOI", (data) =>
            {
                var POICard = new CardBuilder(this, CardBuilder.Layout.Alert)
                .SetText("POI Update.")
                .SetFootnote("Tap to read more")
                .SetIcon(Resource.Drawable.gpsLocation);
                RunOnUiThread(() => SetContentView(POICard.View));
                
            });
            /*var b = new CardBuilder (this, CardBuilder.Layout.Text);
            b.SetText ("");
            b.SetFootnote ("Let's get hacking!");
            b.SetTimestamp ("now");
            */
            var b = new CardBuilder(this, CardBuilder.Layout.Author)
    .SetText("Nearest responder: 3.2Km away")
    .SetHeading("Frank Castle")
    .SetSubheading("ID: John-117")
    .SetFootnote("EOC: Connected")
    .SetTimestamp("Hr: 78");
            // camara
            if (IsThereAnAppToTakePictures())
            {
                //view.SetText("Ready to start recording.");
                CreateDirectoryForPictures();
            }

            SetContentView(b.View);

            // Gesture
            this.mGestureDetector = new GestureDetector(this);
            this.mGestureDetector.SetBaseListener(this);


            //lcoation 
            locMgr = GetSystemService(Context.LocationService) as LocationManager;

            

            // Create our Preview view and set it as the content of our activity.
            //mPreview = new Preview(this);
            //SetContentView(mPreview);

            // Find the total number of cameras available
            numberOfCameras = Camera.NumberOfCameras;

            // Find the ID of the default camera
            Camera.CameraInfo cameraInfo = new Camera.CameraInfo();
            for (int i = 0; i < numberOfCameras; i++)
            {
                Camera.GetCameraInfo(i, cameraInfo);
                if (cameraInfo.Facing == CameraFacing.Back)
                {
                    defaultCameraId = i;
                }
            }
		}

        public bool OnGesture(Gesture gesture)
        {
            System.Console.WriteLine(gesture);
            if (gesture == Gesture.SwipeRight)
            {
                // do something on right (forward) swipe
                /*var b = new CardBuilder(this, CardBuilder.Layout.Text);
                b.SetText("A view dude");
                b.SetFootnote("WUBALUBADUBDUB");
                b.SetTimestamp("now");
                SetContentView(b.View);
                return true;*/
               
            }
            else if (gesture == Gesture.SwipeLeft)
            {
                // do something on left (backwards) swipe
                TakeVideo(this,null);
                //TakeAPicture(this, null);
                return true;
            }
            else if (gesture == Gesture.SwipeDown)
            {
                // do something on the down swipe
                Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                return true;
            }
            else if (gesture == Gesture.SwipeUp)
            {

                Intent intent = new Intent(Intent.ActionView);
                intent.SetData(Uri.Parse("google.navigation:q=51.080018013294136,-114.12513971328735"));
                StartActivity(intent);
                // do something on the up swipe
                return true;
            }
            else if (gesture == Gesture.Tap)
            {
                // do something on left (backwards) swipe
                //TakeVideo(this,null);
                var b = new CardBuilder(this, CardBuilder.Layout.Text)
                .SetText("Fire incident happened 1 Km northeast. Victims number unkonwn.")
                .SetFootnote("Long Press to Dismiss")
                .SetTimestamp("Hr: 78");
                SetContentView(b.View);
                return true;
            }
            else if (gesture == Gesture.LongPress) {
                var b = new CardBuilder(this, CardBuilder.Layout.Author)
                .SetText("Nearest responder: 3.2Km away")
                .SetHeading("Frank Castle")
                .SetSubheading("ID: John-117")
                .SetFootnote("EOC: Connected")
                .SetTimestamp("Hr: 75");
                SetContentView(b.View);
                return true;
            }
           

            return false;
        }

        public void setContentView(View view) {
            SetContentView(view);
        }
        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            if (this.mGestureDetector != null)
            {
                return this.mGestureDetector.OnMotionEvent(e);
            }

            return false;
        }
        
        public override void Finish()
        {
            base.Finish();
            //sod.disconnect();
        }
        protected override void OnResume()
        {
            base.OnResume();
            
            /*if (sod == null)
            {
                sod = new SoD(serverIP, 3000, AndroidDeviceType.Glass);
            }
            else if (!sod.socket.Connected)
            {
                sod = new SoD(serverIP, 3000, AndroidDeviceType.Glass);
            }*/
            sod = new SoD(serverIP, 3000, AndroidDeviceType.Glass);
            // Camera 
            // Open the default i.e. the first rear facing camera.
            //mCamera = Camera.Open();
            //cameraCurrentlyLocked = defaultCameraId;
            //mPreview.PreviewCamera = mCamera;
           
            // Location
            //LocationService locationManager = () 
            //this.get
            /*string Provider = LocationManager.GpsProvider;

            if (locMgr.IsProviderEnabled(Provider))
            {
                locMgr.RequestLocationUpdates(Provider, 2000, 1, this);
            }
            else
            {
                Log.Info(tag, Provider + " is not available. Does the device have location services enabled?");
            }*/
            locationCriteria.Accuracy = Accuracy.Coarse;
            locationCriteria.PowerRequirement = Power.Medium;

            locationProvider = locMgr.GetBestProvider(locationCriteria, true);

            if (locationProvider != null)
            {
                locMgr.RequestLocationUpdates(locationProvider, 2000, 1, this);
            }
            else
            {
                Log.Info(tag, "No location providers available");
            }
            /*Criteria locationCriteria = new Criteria();

            locationCriteria.Accuracy = Accuracy.Coarse;
            locationCriteria.PowerRequirement = Power.Medium;

            locationProvider = locMgr.GetBestProvider(locationCriteria, true);

            if (locationProvider != null)
            {
                locMgr.RequestLocationUpdates(locationProvider, 2000, 1, this);
            }
            else
            {
                Log.Info(tag, "No location providers available");
            }*/
            
        }
        
        // Location delegate
        public void OnLocationChanged(Location location){
            System.Console.WriteLine(location.Latitude + " - " + location.Longitude);
            if (sod.socket!=null)
            {
                if(sod.socket.Connected)
                    sod.socket.Emit("updateERLocation", location.Latitude + "," + location.Longitude);
            }
            //sod.socket.Emit("updateERLocation", location.Latitude + "," + location.Longitude);
        }
        public void OnProviderDisabled(string provider)
        {
        
        }
        public void OnProviderEnabled(string provider)
        {
        
        }
        public void OnStatusChanged(string provider, Availability avail, Bundle bundle)
        {
        
        }
        //END: location delegate

        /*public static String getMapUrl(double latitude, double longitude, double currentLat, double currentLon, int width, int height)
        {
            try
            {
                /*String raw = "https://maps.googleapis.com/maps/api/staticmap?sensor=false&size=" + width + "x" + height +
                    "&style=feature:all|element:all|saturation:-100|lightness:-25|gamma:0.5|visibility:simplified" +
                    "&style=feature:roads|element:geometry&style=feature:landscape|element:geometry|lightness:-25" +
                    "&markers=icon:" + URLEncode.encode("http://mirror-api.appspot.com/glass/images/map_dot.png",
                    "UTF-8") + "|shadow:false|" + currentLat + "," + "" + currentLon + "&markers=color:0xF7594A|" + latitude + "," + longitude;
                return raw.Replace("|", "%7C");
                 * 
            }
            catch (UnsupportedEncodingException e)
            {
                return null;
            }
        }*/
        protected override void OnPause()
        {
            base.OnPause();
            //Camera
            // Because the Camera object is a shared resource, it's very
            // important to release it when the activity is paused.
            if (mCamera != null)
            {
                //mPreview.PreviewCamera = null;
                mCamera.Release();
                mCamera = null;
            }

            locMgr.RemoveUpdates(this);
            if (this.sod.socket != null)
            {
                if (this.sod.socket.Connected)
                    sod.disconnect();
            }
            
        }

        public void LOG(string logstring)
        {
            
            System.Console.WriteLine(logstring);
        }



        // Camera API 
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            // Inflate our menu which can gather user input for switching camera
            //MenuInflater.Inflate(Resource.Menu.camera_menu, menu);
            return true;
        }

        // Camera Intent
        private File _dir;
        private File _file;
        private ImageView _imageView;

        private void CreateDirectoryForPictures()
        {
            _dir = new File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures), "ERWearPictures");
            if (!_dir.Exists())
            {
                _dir.Mkdirs();
            }
        }
        private bool IsThereAnAppToTakePictures()
        {
            //Intent intent = new Intent(MediaStore.ActionImageCapture);
            Intent intent = new Intent(MediaStore.ActionVideoCapture);
            IList<ResolveInfo> availableActivities = PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        private void TakeVideo(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(MediaStore.ActionVideoCapture);
            intent.PutExtra(MediaStore.ExtraDurationLimit, 10);
            
            _file = new File(_dir, String.Format("myVideo_{0}.mp4", Guid.NewGuid()));

            intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(_file));

            StartActivityForResult(intent, 0);
        }
        private void TakeAPicture(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);

            _file = new File(_dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));

            intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(_file));

            StartActivityForResult(intent, 0);
        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            /*System.Console.WriteLine(resultCode.ToString());
            System.Net.WebClient Client = new System.Net.WebClient();
            Client.Headers.Add("Content-Type", "binary/octet-stream");
            byte[] result = Client.UploadFile(serverIP, "POST", _file.Path);//("localhost/FolderName/upload.php", "POST", path);
            string s = System.Text.Encoding.UTF8.GetString(result, 0, result.Length);
            */

            // make it available in the gallery
            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Uri contentUri = Uri.FromFile(_file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);
            //upload 
            sod.socket.Emit("showMediaAll","");
            
            // display in ImageView. We will resize the bitmap to fit the display
            // Loading the full sized image will consume to much memory 
            // and cause the application to crash.
            /*int height = _imageView.Height;
            int width = Resources.DisplayMetrics.WidthPixels;*/
            
            /*using (Bitmap bitmap = _file.Path.LoadAndResizeBitmap(width, height))
            {
                //_imageView.RecycleBitmap();
                //_imageView.SetImageBitmap(bitmap);
            }*/
            //using (Bitmap bitmap = _file.Path.)
           
        }


        //UPLOADYO

        public static class Http
        {
            public static byte[] Post(string uri, NameValueCollection pairs)
            {
                byte[] response = null;
                using (WebClient client = new WebClient())
                {
                    response = client.UploadValues(uri, pairs);
                }
                return response;
            }
        }
	}
}


