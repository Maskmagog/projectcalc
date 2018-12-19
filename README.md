# projectcalc
Project Cars 2 personal leaderboards for consoles. Receives UDP from game, and scrapes Steam leaderboards.

Project CALC - Community Assisted Leaderboards for Console

We all know the state of console leaderboards for Project CARS 2. So with some great help from others, I've built a solution, that runs on a Raspberry Pi, and saves all your laps (laptime, sector times etc). 

This runs on the Pi:
•.NET C# that receives UDP data
•database where laps are stored
•apache2 web server and php page that displays everything
•Python script that scrapes the cars2-stats-steam page and stores all times in the database

On the Pi you need to install php7, MariaDB, Apache2, Mono.
To edit the UDP program, you need Visual Studio.

Set Project CARS 2 to output UDP in PC2 format (version 2).

The .NET program receives and interprets various UDP data from the game. Thank you Zeratall for the library, and all the help! It checks for your current lap record for the car-track combo you are currently running. You could set it to record all laps, or just laps that are valid, or just laps in Time Trial, or whatever you want. Store the data you want. Track temp? Ride height? Tyre temps? Rain density?

The database stores everything, and you could use that data anyway you want. It's a MariaDB, very similar to MySQL, and easy to use.

I have a php page that shows leaderboards, with dropdown selections for track and car. It's possible to show just the top times for each car for selected track, or show all your stored laps. All this was a huge improvement over in-game leaderboards to me. I'm not a programmer at all, so it has taken a lot of trial and error. I know that there are plenty of computer programs that does this too, probably in a better way. The thing for me is that I don't want my computer in all the time, running the program. The Pi on the other hand, can be on all the time. It's slightly larger than a credit card, and costs about $35. I have an old tablet next to my monitor that shows the leaderboards, and my lap record for current car-track combo that I'm running. 

Including data from cars2-stat-steam
I, as many others, have used the cars2-stat-steam page to compare lap times. I found a scraper for this (Thanks Jonas Gulle!), and modified it to scrape all leaderboard pages I want, and store them in the database. Now i can instantly see where I stand on my tablet: my laptimes are mixed in with the steam laptimes. For me, the statistics are a big part of racing/gaming. I want to see my lap times and sector times, to analyze it. Now I can! No longer is my (for me) super awesome lap in a Ginetta G40 Junior erased by a crappy lap in a Toyota GT-One. It's all stored, and easily viewed.

Use it freely, and please post any improvements you make here, so others can enjoy it too. As I said, I'm not a programmer, and I have no doubt that things can be done better. Especially, I'd like some of you that have knowledge in security to have a look at the code. To the best of my ability, I've tried to avoid SQL injections for example, but I'm not sure if it's enough.

The future
If you are interested, we could look at moving it all to Azure servers. This way, console players could upload their data to the same database, also including PC players (from scraping). A Grand Unified Leaderboard! JasonSandwich, that has assisted in the develoment, knows more on this. Console players would need a Raspberry Pi, or desktop application, or something else, that receives 
the UDP and transmits data to the Azure server. I haven't looked into this at all, but it's an exciting thought.

Use at your own risk. I take no responsibility.
This project is in no way affiliated with Slightly Mad Studios.

![image](https://i.postimg.cc/ZnJr9pBj/Screenshot-1.jpg)
![image](https://i.postimg.cc/NFbRRFRt/Screenshot-2.jpg)
