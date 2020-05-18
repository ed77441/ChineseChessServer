using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChineseChessServer.ChessPiece;

namespace ChineseChessServer
{
    partial class BlindChess : ChessGame
    {
        private int turnCounter = 0;
        private bool colorIsDecided;
        public BlindChess(ChessServer server, String player1ID, String player2ID) 
            : base(server, player1ID, player2ID)
        {
            colorIsDecided = false;
            InitChessPieces();
            SuffleChessPieces();
        }
        /*Set chess pieces*/
        void InitChessPieces()
        {
            ColorConstants RED = ColorConstants.RED;
            ColorConstants BLACK = ColorConstants.BLACK;

            RankConstants SOLDIER = RankConstants.SOLDIER;
            RankConstants HORSE = RankConstants.HORSE;
            RankConstants CARRIAGE = RankConstants.CARRIAGE;
            RankConstants SECRETARY = RankConstants.SECRETARY;
            RankConstants IMPERIAL_GUARD = RankConstants.IMPERIAL_GUARD;
            RankConstants KING = RankConstants.KING;
            RankConstants CANNON = RankConstants.CANNON;

            chessBoard = new ChessPiece[4, 8] {
                {
                    CreatePiece(RED, KING), CreatePiece(BLACK, KING),
                    CreatePiece(RED, IMPERIAL_GUARD), CreatePiece(RED, IMPERIAL_GUARD),
                    CreatePiece(BLACK, IMPERIAL_GUARD), CreatePiece(BLACK, IMPERIAL_GUARD),
                    CreatePiece(RED, SECRETARY), CreatePiece(RED, SECRETARY)},
                   {CreatePiece(BLACK, SECRETARY),  CreatePiece(BLACK, SECRETARY),
                    CreatePiece(RED, CARRIAGE), CreatePiece(RED, CARRIAGE),
                    CreatePiece(BLACK, CARRIAGE), CreatePiece(BLACK, CARRIAGE),
                    CreatePiece(RED, HORSE), CreatePiece(RED, HORSE) },
                   {CreatePiece(BLACK, HORSE),  CreatePiece(BLACK, HORSE),
                    CreatePiece(RED, CANNON), CreatePiece(RED, CANNON),
                    CreatePiece(BLACK, CANNON),  CreatePiece(BLACK, CANNON),
                    CreatePiece(RED, SOLDIER), CreatePiece(RED, SOLDIER) },  {CreatePiece(RED, SOLDIER),
                    CreatePiece(RED, SOLDIER), CreatePiece(RED, SOLDIER),
                    CreatePiece(BLACK, SOLDIER), CreatePiece(BLACK, SOLDIER), CreatePiece(BLACK, SOLDIER),
                    CreatePiece(BLACK, SOLDIER), CreatePiece(BLACK, SOLDIER)
                }
            };
        }

        private void SuffleChessPieces()
        {
            Random rnd = new Random();
            for (int i = 0; i < 64; ++i)
            {
                int r1 = rnd.Next(0, 4);
                int c1 = rnd.Next(0, 8);
                int r2 = rnd.Next(0, 4);
                int c2 = rnd.Next(0, 8);

                ChessPiece tmp = chessBoard[r1, c1];
                chessBoard[r1, c1] = chessBoard[r2, c2];
                chessBoard[r2, c2] = tmp;
            }
        }

        override
        public void HandleClickEvent(String id, int pos)
        {
            Point newPoint = TranslateToPoint(pos);
            if (gameState.ActiveID == id)
            {
                if (DecideForEvent(newPoint))
                {
                    gameState.Alternate();
                    server.SendGameEvent(gameState.ActiveID, "SELECTED", "");
                    server.SendGameEvent(gameState.PassiveID, "SELECTED", "");
                    SendTurnMessage();
                }
            }
        }

        private void DecideColor(ColorConstants pieceColor)
        {
            ColorConstants remainingColor =
                pieceColor == ColorConstants.RED ? ColorConstants.BLACK : ColorConstants.RED;

            gameState.ActiveColor = pieceColor;
            gameState.PassiveColor = remainingColor;

            server.SendGameEvent(gameState.ActiveID, "COLOR", gameState.ActiveColor.ToString());
            server.SendGameEvent(gameState.PassiveID, "COLOR", gameState.PassiveColor.ToString());
        }

        private bool DecideForEvent(Point newPoint)
        {

            bool isValid = false;
            ChessPiece piece = chessBoard[newPoint.y, newPoint.x];
            bool isNotNull = piece != null;
            String activeID = gameState.ActiveID;
            String passiveID = gameState.PassiveID;

            if (isNotNull && !piece.flipped)
            {
                if (!colorIsDecided)
                {
                    DecideColor(piece.color);
                    colorIsDecided = true;
                }

                String payload = piece.color.ToString() + "_" + ((int)piece.rank) + " " + (newPoint.x + newPoint.y * 8);

                server.SendGameEvent(activeID, "FLIP", payload);
                server.SendGameEvent(passiveID, "FLIP", payload);
                
                isValid = true;
                piece.flipped = true;
                //PrintChessBoardState();
            }
            else if (isNotNull && piece.color == gameState.ActiveColor)
            {
                selected = newPoint;
                String payload = (piece.color.ToString() + "_" + ((int)piece.rank));
                server.SendGameEvent(activeID, "SELECTED", payload);
            }
            else if (selected.x != -1)
            {
                if (MoveTo(newPoint))
                {
                    String payload = (selected.x + selected.y * 8) + ">" + (newPoint.x + newPoint.y * 8);

                    server.SendGameEvent(activeID, "MOVE", payload);
                    server.SendGameEvent(passiveID, "MOVE", payload);

                    turnCounter = 
                        chessBoard[newPoint.y, newPoint.x] != null ? 0 : turnCounter + 1;

                    chessBoard[newPoint.y, newPoint.x] = chessBoard[selected.y, selected.x];
                    chessBoard[selected.y, selected.x] = null;

                    isValid = true;
                    CheckIfIsGameOver();
                    ResetSelectedPos();
                  //  PrintChessBoardState();
                }
            }

            return isValid;
        }

        private void CheckIfIsGameOver()
        {
            ChessPiece[] allPieces = chessBoard.Cast<ChessPiece>().
                Where(piece => piece != null).ToArray();

            ChessPiece[] redPieces = allPieces.
                Where(piece => piece.color == ColorConstants.RED).ToArray();

            ChessPiece[] blackPieces = allPieces.
                Where(piece => piece.color == ColorConstants.BLACK).ToArray();


            if (redPieces.Length == 0)
            {
                server.SendGameOverMessage(gameState.GetIDBaseOnColor(ColorConstants.BLACK),
                    gameState.GetIDBaseOnColor(ColorConstants.RED), false);
            }
            else if (blackPieces.Length == 0)
            {
                server.SendGameOverMessage(gameState.GetIDBaseOnColor(ColorConstants.RED),
                    gameState.GetIDBaseOnColor(ColorConstants.BLACK), false);
            }
            else
            {
                bool redOnlyOneCannon = redPieces.Length == 1 && redPieces[0].rank == RankConstants.CANNON;
                bool blackOnlyOneCannon = blackPieces.Length == 1 && blackPieces[0].rank == RankConstants.CANNON;

                bool isDraw1 = redOnlyOneCannon && blackOnlyOneCannon;

                bool isDraw2 = redOnlyOneCannon &&
                    blackPieces.All(piece => piece.rank == RankConstants.SOLDIER);

                bool isDraw3 = blackOnlyOneCannon &&
                    redPieces.All(piece => piece.rank == RankConstants.SOLDIER);

                bool isDraw4 = turnCounter >= 15;

                if (isDraw1 || isDraw2 || isDraw3 || isDraw4)
                {
                    server.SendGameOverMessage(gameState.ActiveID, gameState.PassiveID, true);
                }
            }
        }
    }
}
