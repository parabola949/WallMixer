# WallMixer
Random Wallpaper Application

WallMixer is an application that takes the guess work out of picking the right wallpaper - by doing it for you! WallMixer will randomly query your favorite picture / wallpaper Subreddits or [wallhaven.cc](http://alpha.wallhaven.cc), randomly select an image, download it, and set it as your desktop background at whatever internal you specify

WallMixer sits in your system tray to stay out of your way and provide a small amount of info for your current wallpaper:

![system tray icon](https://raw.githubusercontent.com/CplPwnies/WallMixer/master/docs/trayicon.PNG)
![systray info](https://raw.githubusercontent.com/CplPwnies/WallMixer/master/docs/tooltip.PNG)

You can also right click to get a menu to manage it:

![tray context menu](https://raw.githubusercontent.com/CplPwnies/WallMixer/master/docs/ContextMenu.PNG)

And, here's the configuration menu where you can add/edit your sources, set your interval, default save location and imgur api key:

![config window](https://raw.githubusercontent.com/CplPwnies/WallMixer/master/docs/Configure.PNG)

Click the button in the top right to add your source
![addsource](https://raw.githubusercontent.com/CplPwnies/WallMixer/master/docs/AddSource.PNG)

## What you need
Out of the box, WallMixer will work right away with [wallhaven.cc](http://alpha.wallhaven.cc) queries. If you want subreddit capabilities, you will need to register your own [Imgur API key](https://api.imgur.com/). I've also noticed that you need to run the application as Administrator due to the fact that it's writing to a file in current directory (especially if you install it in Program Files). Other than that, you just need .NET 4.5+

## Libraries Used
* [MahApps.Metro](http://mahapps.com/)
* [Caliburn.Micro](http://caliburnmicro.com/)
* [SQLite](https://www.sqlite.org/)
* [Dapper](https://github.com/StackExchange/dapper-dot-net)
* [Hardcodet WPF NotifyIcon](http://www.hardcodet.net/wpf-notifyicon)
