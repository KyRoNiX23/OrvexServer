namespace Orvex.Server.Core;

public sealed class ServerLifecycle
{
    private ServerState _state = ServerState.Stopped;

    public ServerState State => _state;

    public bool IsRunning => _state == ServerState.Running;

    public void SetState(ServerState state)
    {
        _state = state;
    }
}