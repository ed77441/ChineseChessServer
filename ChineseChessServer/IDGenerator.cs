using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseChessServer
{
    class IDGenerator
    {
        static readonly String content = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static String Generate() {
            Random random = new Random();
            String result = "";

            for (int i = 0; i < 10; ++i) {
                result += content[random.Next(0, content.Length)];
            }

            return result;
        }
    }
}
