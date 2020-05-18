using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseChessServer
{
    class GameList : List<ChessGame>
    {
        private readonly Object loc = new Object();
        public void AddGame(ChessGame chessGame) {
            lock (loc) {
                Add(chessGame);
            }
        }

        public void RemoveGame(String id) {
            ChessGame game = FindGame(id);
            lock (loc) {
                Remove(game);
            }
        }

        public ChessGame FindGame(String id) {
            return this.FirstOrDefault(g => {
                GameState gameState = g.GetGameState();
                return gameState.ActiveID == id || gameState.PassiveID == id;
            });
        }
    }
}
