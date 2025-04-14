# Discord Bot written in C# with fully functional Application Commands

## Requirements
- Docker

## Installation steps
- Create a new folder containing the folders `Database` and `plugins`
  - Note: You may need to edit the permissions of these folders
- Make a copy of `application.yml.example` called `application.yml`
- Make a copy of `config.json.example` called `config.json`
  - Set the token found on the discord developer portal
  - Set a new password for the lavalink connection
- Make a copy of `docker-compose.yml.example` called `docker-compose.yml`
  - Change the password to the same as in `config.json`
- Run `docker compose up -d` to launch the bot

## Optional
- Enable spotify in `application.yml` by setting spotify to true
  - This requires setting the clientId and clientSecret from the spotify dev area
- To run only in certain servers you can add `"TestServer": [ your guild id ]` to `config.json`
- More guild ids can be added like so `"TestServer": [ 123456789, 987654321 ]`
