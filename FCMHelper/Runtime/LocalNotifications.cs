#if UNITY_IOS
using System.Runtime.InteropServices;
using System;
#endif
#if UNITY_ANDROID
using UnityEngine;
using System.Text.RegularExpressions;
#endif
namespace com.binouze.FCMHelper
{
    public class LocalNotifications
    {
        #if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _FCMHelper_SendLocalNotification(string instanceId, string title, string body, string imageUrl, int timeIntervalSecs);
        #endif
        
        #if UNITY_ANDROID
        public static void Send( string title, string body, string imageUrl, int delaySec = 0, string color = "#ffffff", string deeplink = "", string channelId = "default", string channelName = "default", string icon = "ic_stat_ic_notification" )
        {
            //String title, String message, String image, string color, String dlink, String channelId, String channelName, String icon, String packageName
            
            if( string.IsNullOrEmpty(color) || !IsValidColor( color ) )
                color = "#ffffff";
            
            using var cls = new AndroidJavaClass("com.binouze.LocalNotifications");
            cls.CallStatic("Send", title, body, imageUrl, color, deeplink, channelId, channelName, icon, Application.identifier );
        }
        
        public static bool IsValidColor(string hc)
        {
            return Regex.IsMatch(hc, @"[#][0-9A-Fa-f]{6}\b");
        }
        #endif
        
        #if UNITY_IOS
        public static void Send( string title, string body, string imageUrl, int delaySec = 0 )
        {
            _FCMHelper_SendLocalNotification(
                instanceId:       DateTime.Now.ToString("yyyyMMddhhmmss"), 
                title:            title,
                body:             body, 
                imageUrl:         imageUrl, // currently not working with external urls
                timeIntervalSecs: delaySec
            );
        }
        #endif
    }
}