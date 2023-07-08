using System;
using LibreLancer;
using LibreLancer.GameData;
using LibreLancer.GameData.World;

namespace LancerEdit;

public class SystemEditData
{
    private StarSystem sys;
    
    public SystemEditData(StarSystem sys)
    {
        this.sys = sys;
        this.SpaceColor = sys.BackgroundColor;
        this.Ambient = sys.AmbientColor;
        this.MusicSpace = sys.MusicSpace;
        this.MusicBattle = sys.MusicBattle;
        this.MusicDanger = sys.MusicDanger;
        this.StarsBasic = sys.StarsBasic;
        this.StarsComplex = sys.StarsComplex;
        this.StarsNebula = sys.StarsNebula;
    }
    
    public Color4 SpaceColor;
    public Color4 Ambient;
    public string MusicSpace;
    public string MusicBattle;
    public string MusicDanger;
    public ResolvedModel StarsBasic;
    public ResolvedModel StarsComplex;
    public ResolvedModel StarsNebula;

    static bool ModelsEqual(ResolvedModel a, ResolvedModel b)
    {
        if (a == null && b != null) return false;
        if (b == null && a != null) return false;
        if (a == b) return true;
        return a.ModelFile.Equals(b.ModelFile, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsDirty() =>
        SpaceColor != sys.BackgroundColor ||
        Ambient != sys.AmbientColor ||
        MusicSpace != sys.MusicSpace ||
        MusicBattle != sys.MusicBattle ||
        MusicDanger != sys.MusicDanger ||
        !ModelsEqual(StarsBasic, sys.StarsBasic) ||
        !ModelsEqual(StarsComplex, sys.StarsComplex) ||
        !ModelsEqual(StarsNebula, sys.StarsNebula);
    

    public void Apply()
    {
        sys.BackgroundColor = SpaceColor;
        sys.AmbientColor = Ambient;
        sys.MusicSpace = MusicSpace;
        sys.MusicBattle = MusicBattle;
        sys.MusicDanger = MusicDanger;
        sys.StarsBasic = StarsBasic;
        sys.StarsComplex = StarsComplex;
        sys.StarsNebula = StarsNebula;
    }
}