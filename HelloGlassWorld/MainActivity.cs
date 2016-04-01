using System;
using System.Drawing;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Glass.App;
using Android.Glass.Widget;
using Android.Provider;
using Gesture = Android.Glass.Touchpad.Gesture;
using GestureDetector = Android.Glass.Touchpad.GestureDetector;
using SoD_Xamarin_AndroidLibrary;


using Java.IO;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;


namespace HelloGlassWorld
{
	[Activity (Label = "ERGlass", Icon = "@drawable/Icon", MainLauncher = true, Enabled = true)]
	[IntentFilter (new String[]{ "com.google.android.glass.action.VOICE_TRIGGER" })]
	[MetaData ("com.google.android.glass.VoiceTrigger", Resource = "@xml/voicetriggerstart")]
	public class MainActivity : Activity , GestureDetector.IBaseListener
	{
		public string serverIP = "192.168.0.106";
        SoD sod;
        string tag = "ERWear";
        private GestureDetector mGestureDetector;
        private bool mIsBound = false;
        //private CameraDemoLocalService cameraDemoLocalService = null;
       
		// The project requires the Google Glass Component from
		// https://components.xamarin.com/view/googleglass
		// so make sure you add that in to compile succesfully.
       
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);


            sod = new SoD(serverIP, 3000, AndroidDeviceType.Glass);
            var b = new CardBuilder (this, CardBuilder.Layout.Text);
            b.SetText ("Welcome to Xamarin Google Glass Development");
            b.SetFootnote ("Let's get hacking!");
            b.SetTimestamp ("now");

            // camara
            if (IsThereAnAppToTakePictures())
            {
                b.SetText("Ready to start recording.");
                CreateDirectoryForPictures();
            }

            SetContentView (b.View);

            // Gesture
            this.mGestureDetector = new GestureDetector(this);
            this.mGestureDetector.SetBaseListener(this);
		}

        public bool OnGesture(Gesture gesture)
        {
            System.Console.WriteLine(gesture);
            if (gesture == Gesture.SwipeRight)
            {
                // do something on right (forward) swipe
                var b = new CardBuilder(this, CardBuilder.Layout.Text);
                b.SetText("A view dude");
                b.SetFootnote("WUBALUBADUBDUB");
                b.SetTimestamp("now");
                SetContentView(b.View);
                return true;
            }
            else if (gesture == Gesture.SwipeLeft)
            {
                // do something on left (backwards) swipe
                TakeVideo(this,null);
                return true;
            }
            else if (gesture == Gesture.SwipeDown)
            {
                // do something on the down swipe
                return true;
            }
            else if (gesture == Gesture.SwipeUp)
            {
                // do something on the up swipe
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
            if (sod.socket != null)
            {
                sod = new SoD(serverIP, 3000, AndroidDeviceType.Glass);
            }
           
            
        }
        protected override void OnPause()
        {
            base.OnPause();
            sod.disconnect();
        }

        public void LOG(string logstring)
        {
            
            System.Console.WriteLine(logstring);
        }


        // Camera
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
            
            _file = new File(_dir, String.Format("myPhoto_{0}.mp4", Guid.NewGuid()));

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

            // make it available in the gallery
            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Uri contentUri = Uri.FromFile(_file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);

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
       

        
        // Note - the key part here is to extend Java.Lang.Object in order to properly setup
        // the IntPtr field and Dispose method required by IBaseListener
        public class GestureListener : Java.Lang.Object, GestureDetector.IBaseListener
        {
            public bool OnGesture(Gesture gesture)
            {
                System.Console.WriteLine(gesture);
                if (gesture == Gesture.SwipeRight)
                {
                    // do something on right (forward) swipe
                    return true;
                }
                else if (gesture == Gesture.SwipeLeft)
                {
                    // do something on left (backwards) swipe
                    return true;
                }
                else if (gesture == Gesture.SwipeDown)
                {
                    // do something on the down swipe
                    return true;
                }
                else if (gesture == Gesture.SwipeUp)
                {
                    // do something on the up swipe
                    return true;
                }

                return false;
            }
        }
	}
}


