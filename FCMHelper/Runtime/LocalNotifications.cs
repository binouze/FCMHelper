using System;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace com.binouze.FCMHelper
{
    public class LocalNotifications
    {
        #if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _FCMHelper_SendLocalNotification(string instanceId, string title, string body, string imageUrl, int timeIntervalSecs);
        #endif
        
        public static void Send( string title, string body, string imageUrl, int delaySec )
        {
            #if UNITY_ANDROID
            
            #elif UNITY_IOS
            _FCMHelper_SendLocalNotification(
                instanceId:       DateTime.Now.ToString("yyyyMMddhhmmss"), 
                title:            title,
                body:             body, 
                imageUrl:         imageUrl, // currently not working with external urls
                timeIntervalSecs: delaySec
            );
            #endif
        }
    }
}