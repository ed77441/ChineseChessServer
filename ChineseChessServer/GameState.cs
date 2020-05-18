using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChineseChessServer.ChessPiece;

namespace ChineseChessServer
{
    class GameState
    {
        private class GamePlayer
        {
            public String id;
            public ColorConstants color;
        }

        GamePlayer active = new GamePlayer(),
            passive = new GamePlayer();

        public GameState(String p1, String p2)
        {
            Random rnd = new Random();
            bool randomChance = rnd.Next(1, 101) <= 50;

            active.id = randomChance ? p1 : p2;
            passive.id = randomChance ? p2 : p1;
        }

        public void Alternate()
        {
            GamePlayer tmp = active;
            active = passive;
            passive = tmp;
        }

        public String ActiveID {
            get { return active.id; }
        }

        public String PassiveID {
            get { return passive.id; }
        }

        public ColorConstants ActiveColor {
            get { return active.color; }
            set { active.color = value;}
        }

        public ColorConstants PassiveColor
        {
            get { return passive.color; }
            set { passive.color = value; }
        }

        public String GetIDBaseOnColor(ColorConstants color) {
            GamePlayer target = active.color == color ? active : passive;
            return target.id;
        }

        public String GetTheOtherID(String id) {
            return id == active.id ? passive.id : active.id ;
        }
    }
}
