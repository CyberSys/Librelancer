using System;
using System.Collections.Generic;
using LibreLancer.Ini;

namespace LibreLancer.Data.Universe.Rooms;
public record SceneEntry(bool AmbientAll, bool TrafficPriority, string Path);

public class RoomInfo : ICustomEntryHandler
{
    [Entry("set_script")] 
    public string SetScript;

    public List<SceneEntry> SceneScripts = new List<SceneEntry>();
    
    [Entry("goodscart_script")]
    public string GoodscartScript;
    
    public IEnumerable<CustomEntry> CustomEntries => new[] {
        new CustomEntry( "scene",Scene)
    };

    static void Scene(ICustomEntryHandler h, Entry e)
    {
        var self = (RoomInfo) h;
        try
        {
            int i = 0;
            bool all = false;
            if (e[0].ToString().Equals("all", StringComparison.OrdinalIgnoreCase)) {
                all = true;
                i++;
            }
            if (!e[i].ToString().Equals("ambient", StringComparison.OrdinalIgnoreCase)) {
                FLLog.Warning("Ini", $"Invalid room scene entry {e}");
            }
            i++;
            var path = e[i].ToString();
            var trafficPriority = (i + 1 < e.Count) && 
                              e[i + 1].ToString().Equals("TRAFFIC_PRIORITY", StringComparison.OrdinalIgnoreCase);
            self.SceneScripts.Add(new SceneEntry(all, trafficPriority, path));
        }
        catch
        {
            FLLog.Error("Ini", $"Bad formatting for scene entry {e}");
        }
    }
}