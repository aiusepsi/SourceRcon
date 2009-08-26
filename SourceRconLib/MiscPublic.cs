using System;

namespace SourceRconLib
{
    public enum MessageCode
    {
        ConsoleOutput, ConnectionClosed, ConnectionSuccess, ConnectionFailed, 
        UnknownResponse, JunkPacket, TooMuchData, CantDisconnectIfNotConnected,
        SendCommandsWhenConnected, EmptyPacket, AlreadyDisposed
    }

    public delegate void BoolInfo(bool info);
    public delegate void RconOutput(MessageCode code, string data);
}