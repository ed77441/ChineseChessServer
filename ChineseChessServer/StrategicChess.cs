using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChineseChessServer.ChessPiece;

namespace ChineseChessServer
{
    partial class StrategicChess : ChessGame
    {
        int turnCounter = 0;
        public StrategicChess(ChessServer server, String player1ID, String player2ID) :
            base (server, player1ID, player2ID) {
            InitColor();
            InitChessPieces();
        }

        private void InitColor() {
            gameState.ActiveColor = ColorConstants.RED;
            gameState.PassiveColor = ColorConstants.BLACK;
            server.SendGameEvent(gameState.ActiveID, "COLOR", ColorConstants.RED.ToString());
            server.SendGameEvent(gameState.PassiveID, "COLOR", ColorConstants.BLACK.ToString());
        }

        private void InitChessPieces() {
            chessBoard = new ChessPiece[10, 9];
            RankConstants?[] lastRow = new RankConstants?[9] {
                RankConstants.CARRIAGE, RankConstants.HORSE, RankConstants.SECRETARY, 
                RankConstants.IMPERIAL_GUARD, RankConstants.KING, RankConstants.IMPERIAL_GUARD,
                RankConstants.SECRETARY, RankConstants.HORSE, RankConstants.CARRIAGE
            };

            RankConstants?[] secondRow = new RankConstants?[9] {
                null,  RankConstants.CANNON, null, null,
                null, null, null, RankConstants.CANNON, null
            };

            RankConstants?[] firstRow = new RankConstants?[9] {
                RankConstants.SOLDIER, null, RankConstants.SOLDIER, null,
                RankConstants.SOLDIER, null, RankConstants.SOLDIER, null, RankConstants.SOLDIER
            };

            PlaceChessPieces(0, lastRow);
            PlaceChessPieces(2, secondRow);
            PlaceChessPieces(3, firstRow);
        }

        private void PlaceChessPieces(int startPos, RankConstants? [] rankList) {
            for (int i = 0; i < 9; ++i) {
                RankConstants? nullableRank = rankList[i];

                if (nullableRank.HasValue) {
                    RankConstants rank = nullableRank.Value;
                    ChessPiece black = CreatePiece(ColorConstants.BLACK, rank);
                    ChessPiece red = CreatePiece(ColorConstants.RED, rank);

                    black.flipped = red.flipped = true;

                    chessBoard[startPos, i] = black;
                    chessBoard[9 - startPos, i] = red;
                }
            }
        }


        override
        public void HandleClickEvent(String id, int pos)
        {
            Point newPoint = TranslateToPoint(pos);

            if (gameState.ActiveID == id) {
                if (ActiveColorIsBlack()) {
                    newPoint = TranposeToNewPoint(newPoint);
                }

                if (DecideForEvent(newPoint)) {
                    gameState.Alternate();
                    server.SendGameEvent(gameState.ActiveID, "SELECTED", "");
                    server.SendGameEvent(gameState.PassiveID, "SELECTED", "");
                    SendTurnMessage();
                }
            }
        }


        private Point TranposeToNewPoint(Point newPoint) {
            return new Point (8 - newPoint.x, 9 - newPoint.y);
        }

        private bool DecideForEvent(Point newPoint) {
            
            ChessPiece piece = chessBoard[newPoint.y, newPoint.x];

            if (piece != null 
                && piece.color == gameState.ActiveColor) {
                selected = newPoint;
                String payload = (piece.color.ToString() + "_" + ((int)piece.rank));
                server.SendGameEvent(gameState.ActiveID, "SELECTED", payload);
            }
            else if (selected.x != -1) {
                if (MoveTo(newPoint)) {

                    int src = TranslateToSingleValue(selected);
                    int dest = TranslateToSingleValue(newPoint);

                    server.SendGameEvent(
                        gameState.GetIDBaseOnColor(ColorConstants.RED), "MOVE", src + ">" + dest);
                    server.SendGameEvent(
                        gameState.GetIDBaseOnColor(ColorConstants.BLACK), "MOVE", (89 - src) + ">" + (89 - dest));

                    ChessPiece target = chessBoard[newPoint.y, newPoint.x];
                    chessBoard[newPoint.y, newPoint.x] = chessBoard[selected.y, selected.x];
                    chessBoard[selected.y, selected.x] = null;
                    //PrintChessBoardState();

                    CheckIfIsGameOver(target);
                    ResetSelectedPos();
                    return true;
                }
            }

            return false;
        }

        private bool ActiveColorIsBlack()
        {
            return gameState.ActiveColor == ColorConstants.BLACK;
        }

        private void CheckIfIsGameOver(ChessPiece target) {
            if (target != null)
            {
                if (target.rank == RankConstants.KING) {
                    server.SendGameOverMessage(gameState.ActiveID, gameState.PassiveID, false);
                }

                turnCounter = 0;
            }
            else {
                turnCounter++;
            }

            if (turnCounter >= 20) {
                server.SendGameOverMessage(gameState.ActiveID, gameState.PassiveID, true);
            }
        }
    }
}
