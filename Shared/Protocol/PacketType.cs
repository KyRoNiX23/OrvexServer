namespace Orvex.Shared.Protocol;

public enum PacketType : ushort
{
    Handshake = 1,
    HandshakeResponse = 2,

    Login = 10,
    LoginResponse = 11,

    Disconnect = 20,
    KeepAlive = 21,
    KeepAliveResponse = 22,

    PlayerJoin = 30,
    PlayerLeave = 31,
    PlayerMovement = 32,

    WorldInfo = 40,
    ChunkData = 41,
    BlockUpdate = 42,

    ChatMessage = 50,

    ServerInfo = 60
}