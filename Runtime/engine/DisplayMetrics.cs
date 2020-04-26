using System;
using System.Runtime.InteropServices;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.engine {
    [StructLayout(LayoutKind.Sequential)]
    public struct viewMetrics {
        public float insets_top;
        public float insets_bottom;
        public float insets_left;
        public float insets_right;

        public float padding_top;
        public float padding_bottom;
        public float padding_left;
        public float padding_right;
    }

    public static class DisplayMetricsProvider {
        public static Func<DisplayMetrics> provider = () => new PlayerDisplayMetrics();
    }

    public interface DisplayMetrics {
        void OnEnable();
        void OnGUI();
        void Update();
        void onViewMetricsChanged();

        float devicePixelRatio { get; }

        viewMetrics viewMetrics { get; }

        WindowPadding viewPadding { get; }

        WindowPadding viewInsets { get; }
    }

    public class PlayerDisplayMetrics : DisplayMetrics {
        float _devicePixelRatio = 0;
        viewMetrics? _viewMetrics = null;

        public void OnEnable() {
        }

        public void OnGUI() {
        }

        public void Update() {
            
        }

        public void onViewMetricsChanged() {
            //view metrics marks dirty
            this._viewMetrics = null;
        }

        public float devicePixelRatio {
            get {
                if (this._devicePixelRatio > 0) {
                    return this._devicePixelRatio;
                }

#if UNITY_ANDROID
                this._devicePixelRatio = AndroidDevicePixelRatio();
#endif

#if UNITY_WEBGL
                this._devicePixelRatio = UIWidgetsWebGLDevicePixelRatio();
#endif

#if UNITY_IOS
                this._devicePixelRatio = IOSDeviceScaleFactor();
#endif

                if (this._devicePixelRatio <= 0) {
                    this._devicePixelRatio = 1;
                }

                return this._devicePixelRatio;
            }
        }

        public WindowPadding viewPadding {
            get {
                return new WindowPadding(this.viewMetrics.padding_left,
                    this.viewMetrics.padding_top,
                    this.viewMetrics.padding_right,
                    this.viewMetrics.padding_bottom);
            }
        }

        public WindowPadding viewInsets {
            get {
                return new WindowPadding(this.viewMetrics.insets_left,
                    this.viewMetrics.insets_top,
                    this.viewMetrics.insets_right,
                    this.viewMetrics.insets_bottom);
            }
        }

        public viewMetrics viewMetrics {
            get {
                if (this._viewMetrics != null) {
                    return this._viewMetrics.Value;
                }

#if UNITY_ANDROID

                using (AndroidJavaObject listenerInstance = new AndroidJavaClass("com.justzht.unity.lwp.LiveWallpaperListenerManager").CallStatic<AndroidJavaObject>("getInstance"))
                {
                    // AndroidJavaObject getRootWindowInsets = windowManagerInstance.Get<AndroidJavaObject>("getDecorView").Get<AndroidJavaObject>("getRootWindowInsets");
                    float insets_top = listenerInstance.Get<int>("insetsTop");
                    float insets_left = listenerInstance.Get<int>("insetsLeft");
                    float insets_right = listenerInstance.Get<int>("insetsRight");
                    float insets_bottom = listenerInstance.Get<int>("insetsBottom");
                    this._viewMetrics = new viewMetrics
                    {
                        insets_bottom = insets_bottom,
                        insets_left = insets_left,
                        insets_right = insets_right,
                        insets_top = insets_top,
                        padding_left = 0,
                        padding_top = 0,
                        padding_right = 0,
                        padding_bottom = 0
                    };
                }

#elif UNITY_WEBGL
                this._viewMetrics = new viewMetrics {
                    insets_bottom = 0,
                    insets_left = 0,
                    insets_right = 0,
                    insets_top = 0,
                    padding_left = 0,
                    padding_top = 0,
                    padding_right = 0,
                    padding_bottom = 0
                };
#elif UNITY_IOS
                viewMetrics metrics = IOSGetViewportPadding();
                this._viewMetrics = metrics;
#else
                this._viewMetrics = new viewMetrics {
                    insets_bottom = 0,
                    insets_left = 0,
                    insets_right = 0,
                    insets_top = 0,
                    padding_left = 0,
                    padding_top = 0,
                    padding_right = 0,
                    padding_bottom = 0
                };
#endif
                return this._viewMetrics.Value;
            }
        }

#if UNITY_ANDROID
        static float AndroidDevicePixelRatio() {
            using (
                AndroidJavaClass wallpaperManagerClass = new AndroidJavaClass("com.justzht.unity.lwp.LiveWallpaperManager")
            ) {
                using (
                    AndroidJavaObject metricsInstance = new AndroidJavaObject("android.util.DisplayMetrics"),
                    windowManagerInstance = wallpaperManagerClass.CallStatic<AndroidJavaObject>("getInstance").Call<AndroidJavaObject>("getWindowManager"),
                    displayInstance = windowManagerInstance.Call<AndroidJavaObject>("getDefaultDisplay")
                ) {
                    displayInstance.Call("getMetrics", metricsInstance);
                    return metricsInstance.Get<float>("density");
                }
            }
            return 2.5f;
        }
#endif

#if UNITY_WEBGL
        [DllImport("__Internal")]
        static extern float UIWidgetsWebGLDevicePixelRatio();
#endif

#if UNITY_IOS
        [DllImport("__Internal")]
        static extern float IOSDeviceScaleFactor();

		[DllImport("__Internal")]
		static extern viewMetrics IOSGetViewportPadding();
#endif
    }
}