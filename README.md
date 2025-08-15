# Sketch Bot

Sketch Bot is a multipurpose Discord bot packed with fun, utility, and moderation features to make your server more lively and engaging! \
Sketch Bot is written in C# using the [Discord.NET](https://github.com/discord-net/Discord.Net) library. \
[Sketch Bot Website](https://sketchbot.xyz)

## Features

- **Fun & Silly Commands**: Greet users, repeat messages, rate anything, ask the 8ball, and more.
- **Image Manipulation**: Apply filters, invert, pixelate, spin, and perform other fun image transformations.
- **Random Animal Pictures**: Instantly post random cat, dog, bird, duck, or fox images.
- **Leveling System**: Users earn XP and level up, with support for leveling roles.
- **Currency System**: Earn, gamble, and pay tokens with a built-in currency system.
- **Music Playback**: Play music in your server’s voice channels.
- **Bulk Message Deletion**: Clean up your channels quickly with bulk message deletion.
- **Math Calculations**: Calculate math problems directly in Discord.
- **Anime Fetching**: Fetch anime information from MyAnimeList.net.
- **And much more!**

## Getting Started

Invite Sketch Bot to your server using the button below:

[![Invite Sketch Bot](https://img.shields.io/badge/Invite%20Sketch%20Bot-7289DA?style=for-the-badge&logo=discord)](https://discord.com/api/oauth2/authorize?client_id=369865463670374400&permissions=1617578818631&scope=bot%20applications.commands)

Use `/help` to see a list of all available commands. \
Join our [support server](https://discord.gg/UPG8Vqb) for help and updates.

### Example Commands
- `/hello` - Say hello!
- `/cat` - Get a random cat picture.
- `/8ball <question>` - Ask the magic 8ball.
- `/repeat <text>` - Repeat your message.
- `/rate <thing>` - Get a rating out of 100.
- `/roll <min> <max>` - Roll a random number.
- `/purge <amount>` - Delete messages in bulk.
- `/calc <expression>` - Calculate math problems.
- `/invert <image>` - Invert the colors of an image.

### Things to Note
Due to the lack of message content intent, some commands must be invoked using a mention prefix like `@SketchBot anime bocchi the rock`

## Docker Setup

You can run Sketch Bot in a containerized environment using Docker and Docker Compose.

### 1. Clone the repository
```sh
git clone https://github.com/Taoshix/SketchBot.git
cd SketchBot
```

### 2. Create a `.env` file
Create a `.env` file in the project root with your secrets and configuration:
```env
SKETCHBOT_PREFIX=?
SKETCHBOT_TOKEN=your_discord_token
SKETCHBOT_OSU_API_ID=our_osu_app_id
SKETCHBOT_OSU_API_KEY=your_osu_api_key
SKETCHBOT_DBL_API_KEY=your_dbl_api_key
SKETCHBOT_DBGG_API_KEY=your_dbg_api_key
SKETCHBOT_DATABASE_USERNAME=dbuser
SKETCHBOT_DATABASE_PASSWORD=dbpassword
SKETCHBOT_DATABASE_NAME=dbname
SKETCHBOT_DATABASE_HOST=localhost
SKETCHBOT_IMGFLIP=your_imgflip_key
```
Do note that not all environment variables are required, but they enable additional features.

### 3. Build and run with Docker Compose
```sh
docker-compose up --build
```

This will build the container and start Sketch Bot using the secrets from your `.env` file.

### 4. Stopping the bot
Press `Ctrl+C` in the terminal, or run:
```sh
docker-compose down
```

##  **Don't delay, invite today!**
Make your server even more sketchy with Sketch Bot!