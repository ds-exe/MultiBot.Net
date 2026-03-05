# Discord Bot written in C# with fully functional Application Commands

## Requirements
- Docker

## Installation steps
- Create a new folder containing the folders `Database`
  - Note: You may need to edit the permissions of this folder
- Make a copy of `config.json.example` called `config.json`
  - Set the token found on the discord developer portal
- Run `docker compose up -d` to launch the bot

## Optional
- To run only in certain servers you can add `"TestServer": [ your guild id ]` to `config.json`
- More guild ids can be added like so `"TestServer": [ 123456789, 987654321 ]`
