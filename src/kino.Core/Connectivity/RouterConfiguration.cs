namespace kino.Core.Connectivity
{
    public class RouterConfiguration
    {
        public SocketEndpoint RouterAddress { get; set; }
        public SocketEndpoint ScaleOutAddress { get; set; }
    }
}