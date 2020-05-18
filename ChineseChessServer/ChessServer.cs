using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows.Input;
using System.Timers;

namespace ChineseChessServer
{
    
    public partial class ChessServer
    {
        private readonly Socket serverSocket = new Socket(
            AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly PlayerInfoList playerInfos = new PlayerInfoList();
        private readonly ChallengeList challenges = new ChallengeList();
        private readonly CompletedMSGQueue messageQueue = new CompletedMSGQueue();
        private readonly GameList games = new GameList();

        private readonly byte[] buffer = new byte[1024];

        static void Main(string[] args)
        {
            ChessServer chessServer = new ChessServer();
            chessServer.SetUp();
            chessServer.AcceptLoop();
        }

        private void SetUp() {
            Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 5555));
            serverSocket.Listen(2);
        }

        private void AcceptLoop() {
            Console.WriteLine("Wainting for client connection...");
            Task.Factory.StartNew(() =>
            {
                while (Console.ReadKey(true).Key != ConsoleKey.Q) ;
                Broadcast("SERVERSHUTDOWN", "");
                serverSocket.Close();
            });

            try
            {
                while (true)
                {
                    Socket clientSocket = serverSocket.Accept();
                    StartReceiveMessage(clientSocket);
                }
            }
            catch (SocketException) {
                Console.WriteLine("Server is now offline!");
            }
        }

        private void StartReceiveMessage(Socket clientSocket) {
            try
            {
                clientSocket.BeginReceive(buffer, 0, buffer.Length,
                   SocketFlags.None, new AsyncCallback(ReceiveMessage), clientSocket);
            }
            catch (SocketException ex) {
                Console.WriteLine(ex.Message);
            }
        }
        private void ReceiveMessage(IAsyncResult result) {
            Socket clientSocket = (Socket) result.AsyncState;
            int receiveLength = clientSocket.EndReceive(result);

            byte[] data = new byte[receiveLength];
            Array.Copy(buffer, data, receiveLength);
            String arrived = Encoding.ASCII.GetString(data);

            bool clientIsUp = true;

            messageQueue.Handle(arrived);

            Console.WriteLine("Message from client: " + arrived);

            while (messageQueue.Count != 0 && clientIsUp) {
                clientIsUp = HandleClientEvent(messageQueue.Dequeue(), clientSocket);
            }

            if (clientIsUp) {
                StartReceiveMessage(clientSocket);
            }
        }

        void UpdateClientPlayerList() {
            Broadcast("PLAYERLIST", playerInfos.GetPlayerListPayload());
        }

        public void SendMessage(Socket target, String messageType, String message) {
            byte[] byteFormData = Encoding.ASCII.GetBytes(messageType + ':' + message + '\0');
            target.Send(byteFormData);
        }

        public void SendMessage(String id, String messageType, String message) {
            SendMessage(playerInfos.GetSocket(id), messageType, message);
        }

        private void Broadcast(String messageType, String message) {
            foreach (PlayerInfo info in playerInfos)
            {
                Socket socket = info.clientSocket;
                SendMessage(socket, messageType, message);
            }
        }

        public void SendGameEvent(String id, String messageType, String message)
        {
            SendMessage(id, "GAMEEVENT:" + messageType, message);
        }

        public void SendGameOverMessage(String winnerID, String loserID, bool isDraw) {
            if (isDraw)
            {
                SendGameEvent(winnerID, "GAMEOVER", "IT'S A DRAW");
                SendGameEvent(loserID, "GAMEOVER", "IT'S A DRAW");
            }
            else {
                SendGameEvent(winnerID, "GAMEOVER", "YOU WIN");
                SendGameEvent(loserID, "GAMEOVER", "YOU LOSE");
            }
            games.RemoveGame(winnerID);
        }
    }
}
