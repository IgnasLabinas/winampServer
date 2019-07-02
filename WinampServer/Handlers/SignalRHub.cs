using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace WinampServer
{
    public class SignalRHub : Hub
    {
        private Player Player;

        public SignalRHub()
        {
            Player = Program.Player;
            Player.Hub = this;
        }
        
        public override Task OnConnected()
        {
            Player.AddClient(Context);
            Clients.Caller.Refresh(Player.Status);
            Clients.Caller.SetRadios(Player.Radios);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Player.RemoveClient(Context);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            Player.AddClient(Context);
            Clients.Caller.Refresh(Player.Status);
            Clients.Caller.SetRadios(Player.Radios);
            return base.OnReconnected();
        }
        
        public void Command(string commandString)
        {
            Player.HandleCommand(commandString, Player.GetClient(Context));
        }

        public void RefreshClients()
        {
            Clients.All.Refresh(Player.Status);
        }
    }
}
