using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChineseChessServer.ChessPiece;

namespace ChineseChessServer
{
    partial class StrategicChess
    {
        private bool MoveTo(Point newPoint)
        {
            ChessPiece src = chessBoard[selected.y, selected.x];

            switch (src.rank)
            {
                case RankConstants.SOLDIER:
                    return CheckMovementForSoldier(newPoint);
                case RankConstants.CANNON:
                    return CheckMovementForCannon(newPoint);
                case RankConstants.HORSE:
                    return CheckMovementForHorse(newPoint);
                case RankConstants.CARRIAGE:
                    return CheckMovementForCarriage(newPoint);
                case RankConstants.SECRETARY:
                    return CheckMovementForSecretary(newPoint);
                case RankConstants.IMPERIAL_GUARD:
                    return CheckMovementForGuard(newPoint);
                case RankConstants.KING:
                    return CheckMovementForKing(newPoint);
            }

            return false;
        }

        private bool CheckMovementForSoldier(Point newPoint)
        {
            bool isBlack = ActiveColorIsBlack();
            Point tmpSelected =
                isBlack ? TranposeToNewPoint(selected) : selected;
            Point tmpNewPoint = isBlack ? TranposeToNewPoint(newPoint) : newPoint;

            if (tmpSelected.y >= 5)
            {
                return tmpNewPoint.y == tmpSelected.y - 1 &&
                    tmpNewPoint.x == tmpSelected.x; /*only up*/
            }

            return tmpSelected.y >= tmpNewPoint.y
                && IsNextToSelectedPos(newPoint); /*up , left, right*/
        }

        private bool CheckMovementForCannon(Point newPoint)
        {
            if (IsOnTheSameLine(newPoint))
            {
                int piecesBetweenPoints = CountPiecesBetweenPoints(newPoint);
                if (chessBoard[newPoint.y, newPoint.x] != null)
                {
                    return piecesBetweenPoints == 1;
                }
                else
                {
                    return piecesBetweenPoints == 0;
                }
            }

            return false;
        }
        private bool CheckMovementForHorse(Point newPoint)
        {
            int offSetX = Math.Abs(selected.x - newPoint.x);
            int offSetY = Math.Abs(selected.y - newPoint.y);

            if ((offSetX == 1 && offSetY == 2) ||
                (offSetY == 1 && offSetX == 2))
            {
                int oneStep;

                if (offSetY == 2)
                {
                    oneStep = selected.y - newPoint.y > 0 ? -1 : 1;
                    return chessBoard[selected.y + oneStep, selected.x] == null;
                }
                else
                {
                    oneStep = selected.x - newPoint.x > 0 ? -1 : 1;
                    return chessBoard[selected.y, selected.x + oneStep] == null;
                }
            }

            return false;
        }

        private bool CheckMovementForCarriage(Point newPoint)
        {
            return IsOnTheSameLine(newPoint) &&
                CountPiecesBetweenPoints(newPoint) == 0;
        }

        private bool CheckMovementForSecretary(Point newPoint)
        {
            int offSetX = Math.Abs(selected.x - newPoint.x);
            int offSetY = Math.Abs(selected.y - newPoint.y);

            bool isInTheItsOwnField =
                ActiveColorIsBlack() ? newPoint.y < 5 : newPoint.y >= 5;

            if (isInTheItsOwnField &&
                offSetX == 2 && offSetY == 2)
            {
                int oneStepX = selected.x - newPoint.x > 0 ? -1 : 1;
                int oneStepY = selected.y - newPoint.y > 0 ? -1 : 1;
                return chessBoard[selected.y + oneStepY, selected.x + oneStepX] == null;
            }

            return false;
        }

        private bool IsInThePalace(Point newPoint)
        {
            if (ActiveColorIsBlack())
            {
                newPoint = TranposeToNewPoint(newPoint);
            }

            return newPoint.x >= 3 && newPoint.x <= 5 &&
                   newPoint.y >= 7 && newPoint.y <= 9;
        }

        private bool CheckMovementForGuard(Point newPoint)
        {
            int offSetX = Math.Abs(selected.x - newPoint.x);
            int offSetY = Math.Abs(selected.y - newPoint.y);
            return IsInThePalace(newPoint) && offSetX == 1 && offSetY == 1;
        }

        private bool CheckMovementForKing(Point newPoint)
        {
            if (IsInThePalace(newPoint) && IsNextToSelectedPos(newPoint))
            {
                return true;
            }

            ChessPiece target = chessBoard[newPoint.y, newPoint.x];

            return target != null &&
                target.rank == RankConstants.KING &&
                IsOnTheSameLine(newPoint) &&
                CountPiecesBetweenPoints(newPoint) == 0;
        }
    }
}
