
#if UNITY_IOS || true
using System.Runtime.InteropServices;
#endif

namespace com.binouze.FCMHelper
{
    public class LocalNotifications
    {
        #if UNITY_IOS || true
        [DllImport("__Internal")]
        private static extern void _FCMHelper_SendLocalNotification(string instanceId, string title, string body, string attachmentFile, int timeIntervalSecs);
        #endif
        
        public void Send( string title, string body, string imageUrl )
        {
            #if UNITY_IOS || true
            _FCMHelper_SendLocalNotification(
                instanceId:     DateTime.Now.ToString("yyyyMMddhhmmss"), 
                title:          title,
                body:           body, 
                attachmentFile: imageUrl, 
                timeIntervalSecs: 30
            );
            #endif
        }
    }
}