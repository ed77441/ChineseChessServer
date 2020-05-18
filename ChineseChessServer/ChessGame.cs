using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChineseChessServer.ChessPiece;

namespace ChineseChessServer
{
    class Point {
        public int x,  y;

        public Point() { }
        public Point(int x, int y) {
            this.x = x;
            this.y = y;
        }


    }

    abstract class ChessGame
    {
        protected GameState gameState;
        protected Point selected = new Point();
        protected ChessServer server;
        protected ChessPiece[,] chessBoard;

        public ChessGame(ChessServer server, String player1ID, String player2ID) {
            this.server = server;
            gameState = new GameState(player1ID, player2ID);

            SendTurnMessage();
            ResetSelectedPos();
        }
        protected void SendTurnMessage() {
            server.SendGameEvent(gameState.ActiveID, "TURN", "It's your turn");
            server.SendGameEvent(gameState.PassiveID, "TURN", "Waiting for opponent...");
        }

        protected ChessPiece CreatePiece(ColorConstants color, RankConstants rank) {
            return new ChessPiece(color, rank);
        }

        protected void ResetSelectedPos() {
            selected.x = selected.y = -1;
        }

        protected bool IsNextToSelectedPos(Point newPoint)
        {
            return (selected.x == newPoint.x && Math.Abs(selected.y - newPoint.y) == 1) ||
               (selected.y == newPoint.y && Math.Abs(selected.x - newPoint.x) == 1);
        }

        protected bool IsOnTheSameLine(Point newPoint)
        {
            return selected.x == newPoint.x || selected.y == newPoint.y;
        }

        protected int CountPiecesBetweenPoints(Point newPoint) {
            int counter = 0;
            if (selected.x == newPoint.x)
            {
                int minY = Math.Min(selected.y, newPoint.y);
                int maxY = Math.Max(selected.y, newPoint.y);

                for (int i = minY + 1; i < maxY; ++i)
                {
                    if (chessBoard[i, newPoint.x] != null)
                    {
                        counter++;
                    }
                }
            }
            else
            {
                int minX = Math.Min(selected.x, newPoint.x);
                int maxX = Math.Max(selected.x, newPoint.x);

                for (int i = minX + 1; i < maxX; ++i)
                {
                    if (chessBoard[newPoint.y, i] != null)
                    {
                        counter++;
                    }
                }
            }

            return counter;
        }


        public GameState GetGameState() {
            return gameState;
        }

        protected void PrintChessBoardState()
        {
            String[] redName = new String[] { "兵", "炮", "馬", "車", "相", "仕", "帥" };
            String[] blackName = new String[] { "卒", "炮", "馬", "車", "象", "士", "將" };


            for (int i = 0; i < chessBoard.GetLength(0); ++i)
            {
                for (int j = 0; j < chessBoard.GetLength(1); ++j)
                {
                    ChessPiece piece = chessBoard[i, j];
                    if (piece != null)
                    {
                        if (piece.flipped)
                        {
                            if (piece.color == ColorConstants.RED)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write(redName[(int)piece.rank] + ' ');
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.Write(blackName[(int)piece.rank] + ' ');
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.Write("蓋 ");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("空 ");
                    }
                }

                Console.WriteLine();
            }

            Console.Out.Flush();
            Console.ForegroundColor = ConsoleColor.Gray;
        }


        public Point TranslateToPoint(int pos) {
            int colWidth = chessBoard.GetLength(1);
            return new Point(pos % colWidth, pos / colWidth);
        }

        public int TranslateToSingleValue(Point point)
        {
            int colWidth = chessBoard.GetLength(1);
            return point.y * colWidth + point.x;
        }


        public abstract void HandleClickEvent(String id, int pos);
    }
}
