namespace LibreLancer.Server.ConsoleCommands
{
    [ConsoleCommand]
    public class BaseCommand : IConsoleCommand
    {
        public string Name => "base";
        public bool Admin => true;

        public void Run(Player player, string arguments)
        {
            var baseName = arguments.Trim();
            if (player.Game.GameData.Bases.Contains(baseName))
            {
                player.ForceLand(baseName);
            }
            else
            {
                player.RemoteClient.OnConsoleMessage($"Base does not exist '{baseName}'");
            }
        }
    }
}