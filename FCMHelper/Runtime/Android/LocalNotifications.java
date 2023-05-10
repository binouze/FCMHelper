package com.binouze.FCMHelper;

public class LocalNotifications
{
    private static final String TAG = "LocalNotifications";

    public static void Send( String title, String message, int color, String typeString, String image, String dlink, String channelId, String channelName, String icon, String packageName ) 
    {
        Log.i( TAG, "Send" );
        new PictureStyleNotification(UnityPlayer.currentActivity,title, message,color,typeString,image,dlink,channelId,channelName,icon,packageName).execute();
    }
}