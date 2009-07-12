using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.IO;
using System.Text;

namespace SourceRconLib
{
    /// <summary>
    /// Encapsulates an RCON packet. Includes methods to output a packet as a byte array,
    /// and to import a packet from a byte array.
    /// </summary>
    internal class RCONPacket
    {
        internal int RequestId;
        internal string String1;
        internal string String2;
        internal RCONPacket.SERVERDATA_sent ServerDataSent;
        internal RCONPacket.SERVERDATA_rec ServerDataReceived;

        internal RCONPacket()
        {
            RequestId = 0;
            String1 = "fakefakefake";
            String2 = String.Empty;
            ServerDataSent = SERVERDATA_sent.None;
            ServerDataReceived = SERVERDATA_rec.None;
        }

        internal byte[] OutputAsBytes()
        {
            // Experimental

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms, new ASCIIEncoding());

            bw.Write((int)0);
            bw.Write(RequestId);
            bw.Write((int)ServerDataSent);
            bw.Write(String1);
            bw.Write(String2);

            // End

            byte[] packetsize;
            byte[] req_id;
            byte[] serverdata;
            byte[] bstring1;
            byte[] bstring2;

            ASCIIEncoding Ascii = new ASCIIEncoding();

            bstring1 = Ascii.GetBytes(String1);
            bstring2 = Ascii.GetBytes(String2);

            serverdata = BitConverter.GetBytes((int)ServerDataSent);
            req_id = BitConverter.GetBytes(RequestId);

            // Compose into one packet.
            byte[] FinalPacket = new byte[4 + 4 + 4 + bstring1.Length + 1 + bstring2.Length + 1];
            packetsize = BitConverter.GetBytes(FinalPacket.Length - 4);

            int BPtr = 0;
            packetsize.CopyTo(FinalPacket, BPtr);
            BPtr += 4;

            req_id.CopyTo(FinalPacket, BPtr);
            BPtr += 4;

            serverdata.CopyTo(FinalPacket, BPtr);
            BPtr += 4;

            bstring1.CopyTo(FinalPacket, BPtr);
            BPtr += bstring1.Length;

            FinalPacket[BPtr] = (byte)0;
            BPtr++;

            bstring2.CopyTo(FinalPacket, BPtr);
            BPtr += bstring2.Length;

            FinalPacket[BPtr] = (byte)0;
            BPtr++;

            return FinalPacket;
        }

        internal void ParseFromBytes(byte[] bytes, Rcon parent)
        {
            int BPtr = 0;
            ArrayList stringcache;
            ASCIIEncoding Ascii = new ASCIIEncoding();

            // First 4 bytes are ReqId.
            RequestId = BitConverter.ToInt32(bytes, BPtr);
            BPtr += 4;
            // Next 4 are server data.
            ServerDataReceived = (SERVERDATA_rec)BitConverter.ToInt32(bytes, BPtr);
            BPtr += 4;
            // string1 till /0
            stringcache = new ArrayList();
            while (bytes[BPtr] != 0)
            {
                stringcache.Add(bytes[BPtr]);
                BPtr++;
            }
            String1 = Ascii.GetString((byte[])stringcache.ToArray(typeof(byte)));
            BPtr++;

            // string2 till /0

            stringcache = new ArrayList();
            while (bytes[BPtr] != 0)
            {
                stringcache.Add(bytes[BPtr]);
                BPtr++;
            }
            String2 = Ascii.GetString((byte[])stringcache.ToArray(typeof(byte)));
            BPtr++;

            // Repeat if there's more data?

            if (BPtr != bytes.Length)
            {
                parent.OnError(MessageCode.TooMuchData, null);
            }
        }

        public enum SERVERDATA_sent : int
        {
            SERVERDATA_AUTH = 3,
            SERVERDATA_EXECCOMMAND = 2,
            None = 255
        }

        public enum SERVERDATA_rec : int
        {
            SERVERDATA_RESPONSE_VALUE = 0,
            SERVERDATA_AUTH_RESPONSE = 2,
            None = 255
        }
    }
}