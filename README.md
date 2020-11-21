Asteria
=======

Official Shared Source from the Asteria Game - https://legendstudio.com/asteria/ & https://store.steampowered.com/app/307130/Asteria/

This is the source for the core of the game. It builds into a .net assembly dll thst is loaded at runtime by the game engine.

The language is C#, which is the same language the game is built in so it's very tightly integrated. It's not so much a scripting language like JS but actually loading code directly into the core of the game. There's no scripting plugin barrier, everything is exposed directly to you. And strong typing and intellisense will make your life a lot easier.

We're using a very loose hacky Javascript style of coding though. It looks a lot like JSON with all the Object Initalizers and has pretty heavy use of lambda expressions (Anonymous Methods in .Net lingo). Check it out: https://github.com/Bryan-Legend/asteria/blob/master/HD.Asteria/Items.cs

Feel free to use any of this code in your own Asteria mods.

### Building

To build this you will need Visual Studio 2010 or 2010 Express (http://www.microsoft.com/visualstudio/eng/downloads#d-2010-express). It won't build with Visual Studio 2012 because XNA doesn't support 2012.

You'll also need XNA Game Studio installed: http://www.microsoft.com/en-us/download/details.aspx?id=23714

To install XNA on Windows 8 you'll first need to install games for windows client first: http://www.xbox.com/en-US/LIVE/PC/DownloadClient

Someday when we switch to MonoGame you'll be able to use any Visual Studio version and won't need to install XNA.

### Enemy Creation Tutorial

This 10 minute video shows you how create a proximity mine enemy type. It shows the basics of the EnemyType class and how to do things like play an animation, delay an action using the enemy scheduler and create an explosion in the map terrain engine. It also explains how to setup your plugin project.

<a href="http://www.youtube.com/watch?feature=player_embedded&v=CwBFFtkam44" target="_blank"><img src="http://img.youtube.com/vi/CwBFFtkam44/0.jpg" width="480" height="360" border="10" /></a>

### Source Code Tour

This is a tour of the source code release explaning the various parts and quickly shows how they work.

<a href="http://www.youtube.com/watch?feature=player_embedded&v=jGR0EVYc_Bo" target="_blank"><img src="http://img.youtube.com/vi/jGR0EVYc_Bo/0.jpg" width="480" height="360" border="10" /></a>

I know I said "uh" way too much in that video. I've not done a lot of videos and I'll slow it down a bit next time.

### World Generator Tour

Here's a quick introduction to the map generator code that you can use as a base to build your own procedurally generated worlds with.

<a href="http://www.youtube.com/watch?feature=player_embedded&v=mn_wW8uZ6eQ" target="_blank"><img src="http://img.youtube.com/vi/mn_wW8uZ6eQ/0.jpg" width="480" height="360" border="10" /></a>
