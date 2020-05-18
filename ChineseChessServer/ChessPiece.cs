using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseChessServer
{
    class ChessPiece
    {
        public ColorConstants color;
        public RankConstants rank;
        public bool flipped;

        public enum ColorConstants
        {
            RED, BLACK
        }
        public enum RankConstants
        {
            SOLDIER = 0,
            CANNON,
            HORSE,
            CARRIAGE,
            SECRETARY,
            IMPERIAL_GUARD,
            KING
        }

        public ChessPiece(ColorConstants color, RankConstants rank)
        {
            this.color = color;
            this.rank = rank;
            flipped = false;
        }
    }
}
