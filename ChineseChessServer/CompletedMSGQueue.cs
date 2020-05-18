using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseChessServer
{
    class CompletedMSGQueue : Queue<String>
    {
        String incompleteMSG = "";

        public void Handle (String arrived) {
            arrived = incompleteMSG + arrived;

            int endPos;
            while ((endPos = arrived.IndexOf('\0')) != -1) {
                Enqueue(arrived.Substring(0, endPos));
                arrived = arrived.Substring(endPos + 1);
            }

            incompleteMSG = arrived;
        }
    }
}
