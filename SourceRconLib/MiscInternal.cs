using System;

namespace SourceRconLib
{
    /// <summary>
    /// Keeps track of a packet's state as it is reconstituted from the network.
    /// </summary>
    internal class PacketState
    {
        internal PacketState()
        {
            PacketLength = -1;
            BytesSoFar = 0;
            IsPacketLength = false;
        }

        public int PacketCount;
        public int PacketLength;
        public int BytesSoFar;
        public bool IsPacketLength;
        public byte[] Data;
    }
}