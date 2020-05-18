using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ChineseChessServer
{

    class PlayerInfo
    {
        public String name, id, status;
        public Socket clientSocket;

        public PlayerInfo(String name, String id, Socket clientSocket) {
            this.name = name;
            this.id = id;
            this.clientSocket = clientSocket;
            status = "Idle";
        }

        public override string ToString() { 
            return name + "." + id + "." + status;
        }

        public void SetStatus(String status) {
            this.status = status;
        }
    }

    internal class PlayerInfoList : List<PlayerInfo>
    {
        private readonly Object loc = new Object();
        public void AddNewPlayer(String name, String id, Socket clientSocket) {
            lock (loc)
            {
                Add(new PlayerInfo(name, id, clientSocket));
            }
        }

        public void RemovePlayer(String id) {
            PlayerInfo matched = this.FirstOrDefault(p => p.id == id);
            lock (loc)
            {
                Remove(matched);
            }
        }

        public String GetPlayerListPayload()
        {
            String [] playerPayload = new String[Count];

            lock (loc)
            {
                for (int i = 0; i < Count; ++i)
                {
                    playerPayload[i] = this[i].ToString();
                }
            }

            return String.Join(",", playerPayload);
        }


        public PlayerInfo FindPlayer(String id) {
            PlayerInfo info = null;
            lock (loc) {
                info = this.FirstOrDefault(p => p.id == id);
            }
            return info;
        }

        public void UpdateStatus(String id, String status)
        {
            FindPlayer(id).status = status;
        }

        public Socket GetSocket(String id) {
            return FindPlayer(id).clientSocket;
        }

        public String GetName(String id) {
            return FindPlayer(id).name;
        }

        public String GetStatus(String id)
        {
            return FindPlayer(id).status;
        }
    }
}