package com.binouze;

import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.AsyncTask;
import android.os.Build;
import android.graphics.Color;

import androidx.core.app.NotificationCompat;
import androidx.core.app.NotificationManagerCompat;

import java.io.IOException;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import android.net.Uri;

public class PictureStyleNotification extends AsyncTask<String, Void, Bitmap> 
{
    private static int  ID = 100;

    private Context mContext;
    private String titre, texte, image, typeString, dlink, channelId, channelName, icon, packageName;
    private int color;

    public PictureStyleNotification(Context context, String titre, String texte, String colorString, /*String typeString,*/ String image, String dlink, String channelId, String channelName, String icon, String packageName )
    {
        super();

        this.mContext      = context;
        this.image         = image;
        this.titre         = titre;
        this.texte         = texte;
        this.color         = Color.ParseColor(colorString);
        this.dlink         = dlink;
        this.typeString    = "";//typeString;
        this.channelId     = channelId;
        this.channelName   = channelName;
        this.icon          = icon;
        this.packageName   = packageName;
    }

    @Override
    protected Bitmap doInBackground(String... params)
    {
        if( this.image != null && this.image.length() > 0 )
        {
            InputStream in;
            try
            {
                URL url = new URL(this.image);
                HttpURLConnection connection = (HttpURLConnection) url.openConnection();
                connection.setDoInput(true);
                connection.connect();
                in = connection.getInputStream();
                return BitmapFactory.decodeStream(in);
            }
            catch( IOException e )
            {
                e.printStackTrace();
            }
        }
        return null;
    }

    @Override
    protected void onPostExecute( Bitmap result )
    {
        super.onPostExecute(result);

        Intent intent; 
        if( dlink != null && dlink.length() > 1 ) { intent = new Intent( Intent.ACTION_VIEW, Uri.parse(dlink) );                   }
        else                                      { intent = new Intent( mContext, com.unity3d.player.UnityPlayerActivity.class ); }
        intent.addFlags( Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP );
        
        PendingIntent pendingIntent = null;
        if( android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.S ) 
        {
            pendingIntent = PendingIntent.getActivity( mContext, 0, intent, PendingIntent.FLAG_MUTABLE );
        }
        else
        {
             pendingIntent = PendingIntent.getActivity(mContext, 0, intent, PendingIntent.FLAG_ONE_SHOT);
        }
        
        int notifID = ID++;

        // recuperer le gestionnaire de notifications
        NotificationManagerCompat notificationManager = NotificationManagerCompat.from( mContext );

        // Since android Oreo notification channel is needed.
        if( Build.VERSION.SDK_INT >= Build.VERSION_CODES.O ) 
        {
            NotificationChannel channel = new NotificationChannel( channelId, channelName, NotificationManager.IMPORTANCE_HIGH );
            notificationManager.createNotificationChannel( channel );
        }

        NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder( mContext, channelId )
                .setSmallIcon( mContext.getResources().getIdentifier(icon, "drawable", packageName) )
                .setContentTitle( titre )
                .setContentText( texte )
                .setStyle(new NotificationCompat.BigTextStyle().bigText(texte))
                .setAutoCancel(true)
                .setContentIntent( pendingIntent )
                .setColor( color )
                //.setSortKey( typeString )
                .setDefaults( NotificationCompat.DEFAULT_ALL )
                .setPriority( NotificationCompat.PRIORITY_HIGH )
                .setGroup(packageName+"default");

        if( result != null )
            notificationBuilder.setLargeIcon( result );

        NotificationCompat.Builder groupBuilder = new NotificationCompat.Builder( mContext, channelId )
                .setSmallIcon( mContext.getResources().getIdentifier(icon, "drawable", packageName) )
                .setContentTitle( titre )
                .setContentText( texte )
                //.setStyle(new NotificationCompat.BigTextStyle().bigText(texte))
                .setColor( color )
                .setGroup(packageName+"default")
                .setGroupAlertBehavior( NotificationCompat.GROUP_ALERT_CHILDREN )
                .setGroupSummary( true );

        notificationManager.notify( notifID, notificationBuilder.build());
        notificationManager.notify( 0,   groupBuilder.build());
    }
}
