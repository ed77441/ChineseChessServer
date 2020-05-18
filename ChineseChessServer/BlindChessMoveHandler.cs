using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChineseChessServer.ChessPiece;

namespace ChineseChessServer
{
    partial class BlindChess
    {

        private bool MoveTo(Point newPoint)
        {
            if (newPoint.x < 0 || newPoint.y < 0 ||
                newPoint.x >= 8 || newPoint.y >= 4)
            {
                return false;
            }

            bool isValid = false;
            ChessPiece src = chessBoard[selected.y, selected.x];
            ChessPiece dest = chessBoard[newPoint.y, newPoint.x];
            bool isNextToEachOther = IsNextToSelectedPos(newPoint);

            if (dest != null)
            {
                if (src.color == dest.color)
                {
                    isValid = false;
                }
                else if (src.rank == RankConstants.CANNON)
                {
                    if (IsOnTheSameLine(newPoint))
                    {
                        isValid = CountPiecesBetweenPoints(newPoint) == 1;
                    }
                }
                else if (isNextToEachOther)
                {
                    if (src.rank == RankConstants.KING &&
                     dest.rank == RankConstants.SOLDIER)
                    {
                        isValid = false;
                    }
                    else if (src.rank == RankConstants.SOLDIER &&
                     dest.rank == RankConstants.KING)
                    {
                        isValid = true;
                    }
                    else
                    {
                        isValid = src.rank >= dest.rank;
                    }
                }
            }
            else
            {
                isValid = isNextToEachOther;
            }

            return isValid;
        }
    }
}
