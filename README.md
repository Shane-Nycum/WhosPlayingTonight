# Who's Playing Tonight
Welcome to Who's Playing Tonight! 
This is a Windows desktop app that will let you search for music events in your area and listen to previews of the artists, all in the same view.

![WhosPlayingTonightScreenshot](https://user-images.githubusercontent.com/51054106/64745585-70ad3480-d4d6-11e9-80fb-47a671431151.PNG)

The events are sourced from Eventbrite, and the artist previews are streamed from Spotify.

To use the app, you'll need to get your own API keys from Eventbrite and Spotify.
Save them to your machine in an XML file that looks like the following:
```xml
<root>
    <secrets ver="1.0">
        <secret name="eventbriteSecret" value="[Eventbrite Secret]" />
        <secret name="spotifySecret" value="[Spotify Secret]" />
        <secret name="spotifyClientId" value="[Spotify Client ID]" />
    </secrets>
</root>
```
And finally, update the App.config file with the path to the XML file.
