using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChineseChessServer
{
    partial class ChessServer
    {
        private bool HandleClientEvent(String completedMessage, Socket clientSocket)
        {
            int sepPos = completedMessage.LastIndexOf(':');
            String messageType = completedMessage.Substring(0, sepPos);
            String message = completedMessage.Substring(sepPos + 1);
            ChessGame game;

            switch (messageType)
            {
                case "CONNECT":
                    SendIDToNewPlayer(message, clientSocket);
                    break;
                case "DISCONNECT":
                    playerInfos.RemovePlayer(message);
                    UpdateClientPlayerList();
                    break;
                case "CHALLENGE":
                    SendChallengeRequest(message);
                    break;
                case "CHALLENGEREPLY":
                    HandleChallengeReply(message);
                    break;
                case "CLICK":
                    DelegateClickEventToGame(message);
                    break;
                case "BACKTOLOBBY":
                    playerInfos.UpdateStatus(message, "Idle");
                    UpdateClientPlayerList();
                    break;
                case "SURRENDER":
                    game = games.FindGame(message);
                    String winnerID = game.GetGameState().GetTheOtherID(message);
                    SendGameOverMessage(winnerID, message, false);
                    break;
                case "QUITGAME":
                    game = games.FindGame(message);
                    SendGameEvent(game.GetGameState().GetTheOtherID(message), "GAMEOVER", "Opponent just quit");
                    games.RemoveGame(message);
                    break;
            }

            return messageType != "DISCONNECT";
        }

        private void SendIDToNewPlayer(String message, Socket clientSocket)
        {
            String id = IDGenerator.Generate();

            while (playerInfos.FindPlayer(id) != null)
            {
                id = IDGenerator.Generate();
            }

            playerInfos.AddNewPlayer(message, id, clientSocket);
            SendMessage(clientSocket, "PLAYERID", id);
            UpdateClientPlayerList();
        }

        private void SendChallengeRequest(String message)
        {
            String[] splited = message.Split(' ');
            String srcID = splited[0];
            String destID = splited[1];
            String gameType = splited[2];

            playerInfos.UpdateStatus(srcID, "Issuing a Challenge");
            playerInfos.UpdateStatus(destID, "Handling a Challenge");
            UpdateClientPlayerList();

            challenges.AddChallenge(srcID, destID, gameType);
   
            SendMessage(destID, "CHALLENGE", playerInfos.GetName(srcID) + ' ' + gameType);
        }

        private void HandleChallengeReply(String message) {



            int sepPos = message.IndexOf(' ');
            String replyStr = message.Substring(0, sepPos);

            String destID = message.Substring(sepPos + 1);
            String srcID = challenges.GetSourceID(destID);
            String gameType = challenges.FindChallenge(destID).gameType;

            challenges.RemoveChallenge(destID);

            if (replyStr == "YES")
            {
                SendMessage(srcID, "CHALLENGEREPLY", playerInfos.GetName(destID) +
                    " accept your challenge request");

                playerInfos.UpdateStatus(srcID, "Playing");
                playerInfos.UpdateStatus(destID, "Playing");
                UpdateClientPlayerList();
                SendGameEvent(srcID, "INITGAME", gameType);
                SendGameEvent(destID, "INITGAME", gameType);

                if (gameType == "BLIND")
                {
                    games.AddGame(new BlindChess(this, srcID, destID));
                }
                else
                {
                    games.AddGame(new StrategicChess(this, srcID, destID));
                }
            }
            else
            {
                SendMessage(srcID, "CHALLENGEREPLY", playerInfos.GetName(destID) +
                    " refuse your challenge request");
                playerInfos.UpdateStatus(srcID, "Idle");
                playerInfos.UpdateStatus(destID, "Idle");
                UpdateClientPlayerList();
            }
        }

        private void DelegateClickEventToGame(String message) {
            int sepPos = message.IndexOf(' ');
            String id = message.Substring(0, sepPos);
            int pos = Int32.Parse(message.Substring(sepPos + 1));

            ChessGame game = games.FindGame(id);
            game.HandleClickEvent(id, pos);
        }
    }
}
