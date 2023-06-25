using System;
using System.Collections.Concurrent;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using LibreLancer.GameData.World;

namespace LibreLancer.Server;

public class WorldProvider
{
    private GameServer server;
    
    public WorldProvider(GameServer server)
    {
        this.server = server;
    }

    public void RemoveWorld(StarSystem system)
    {
        worlds.TryRemove(system, out _);
    }
    
    struct WorldState
    {
        public bool Ready;
        public ServerWorld World;
    }

    private ConcurrentDictionary<StarSystem, WorldState> worlds = new ConcurrentDictionary<StarSystem, WorldState>();
    
    void LoadWorld(StarSystem system, out WorldState ws)
    {
        var x = new WorldState();
        if (worlds.TryAdd(system, new WorldState()))
        {
            x.World = new ServerWorld(system, server);
            x.Ready = true;
            server.WorldReady(x.World);
            worlds.AddOrUpdate(
                system,
                _ => x,
                (_, _) => x
            );
        }
        ws = x;
    }
    public void RequestWorld(StarSystem system, Action<ServerWorld> spunUp)
    {
        Task.Run(async () =>
        {
            if (!worlds.TryGetValue(system, out var ws))
                LoadWorld(system, out ws);
            while (!ws.Ready) {
                await Task.Delay(33);
                if(!worlds.TryGetValue(system, out ws))
                    LoadWorld(system, out ws);
            }
            spunUp(ws.World);
        });
    }
}