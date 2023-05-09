# FCM Helper

Firebase Messaging integration for Unity with image support on push notifications

## Installation

Choose your favourite method:

- **Plain install**
    - Clone or [download](https://github.com/binouze/FCMHelper/archive/refs/heads/master.zip) 
this repository and put it in the `Assets/Plugins` folder of your project.
- **Unity Package Manager (Manual)**:
    - Add the following line to *Packages/manifest.json*:
    - `"com.binouze.fcmhelper": "https://github.com/binouze/FCMHelper.git"`
- **Unity Package Manager (Auto)**
    - in the package manager, click on the + 
    - select `add package from GIT url`
    - paste the following url: `"https://github.com/binouze/FCMHelper.git"`


## How to use

The unity implementation of FirebaseMessaging does not supports Images in push notification but default on iOS.
This plugin just add this support.

See Firebase Messaging documentation here: https://firebase.google.com/docs/cloud-messaging/unity/client
