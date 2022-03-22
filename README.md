![](https://imgur.com/QViYxpM.png)

# SongLinkBot
[![Publish Docker Image](https://github.com/micahmo/SongLinkBot/actions/workflows/docker-image.yml/badge.svg)](https://github.com/micahmo/SongLinkBot/actions/workflows/docker-image.yml)

SongLinkBot is a Telegram bot that listens for song links and finds corresponding links for the same song on other music services.

## Usage

Add [@mdmSongLinkBot](https://t.me/mdmSongLinkBot) to your chats. When it detects a song link from either Spotify or YouTube Music, it sends a message to the chat with corresponding links to the other service. It also includes a link to the song's landing page on [song.link](https://song.link), which includes many other services.

## Running

To run your own instance of this bot with Docker, use the following command. You will have to create a Telegram bot and obtain the token.

``` powershell
nerdctl run -d --name=SongLinkBot -e BOT_TOKEN="YOUR_BOT_TOKEN" ghcr.io/micahmo/songlinkbot:latest
```

# Attribution

Thanks to the great service [song.link](https://song.link), which actually finds all of the corresopnding links.

[Icon](https://www.flaticon.com/premium-icon/music-notes_1895657) created by [Freepik](https://www.flaticon.com/authors/freepik) - [Flaticon](https://www.flaticon.com/)