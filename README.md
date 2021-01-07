# ChineseChessServer

This project is developed using `C# console application`, final project of algorithm class

## Description

* Server will take request from client, and add them into client list
* Server will accept several clients, but only two people will in the same game
* Server will get all request from client, game state will be stored in here as well
* Because game state is stored at server, it is not possible to cheat at client end
* Use `C#` async to listen to every single client request
* All client will receive server shutdown message if server is being shutdown
