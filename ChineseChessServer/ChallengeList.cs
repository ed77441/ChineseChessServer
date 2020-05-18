using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
namespace ChineseChessServer
{

    public class Challenge {
        public String srcID, destID, gameType;

        public Challenge(String srcID, String destID, String gameType) {
            this.srcID = srcID;
            this.destID = destID;
            this.gameType = gameType;
        }
    }

    class ChallengeList : List<Challenge>
    {
        private readonly Object loc = new Object();

        public void AddChallenge(String srcID, String destID, String gameType)
        {
            Challenge c = new Challenge(srcID, destID, gameType);
            lock (loc)
            {
                Add(c);
            }
        }

        public void RemoveChallenge(String destID)
        {
            lock (loc)
            {
                Challenge matched = this.FirstOrDefault(c => c.destID == destID);
                Remove(matched);
            }
        }

        public String GetSourceID(String destID) {
            return FindChallenge(destID).srcID;
        }

        public Challenge FindChallenge(String destID) {
            Challenge result = null;
            lock (loc)
            {
                result = this.FirstOrDefault(c => c.destID == destID);
            }

            return result;
        }
    }
}
