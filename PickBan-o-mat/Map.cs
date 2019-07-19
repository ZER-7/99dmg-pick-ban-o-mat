using System.ComponentModel;

namespace PickBan_o_mat
{
    public enum Map
    {
        [Description("Overpass")] Overpass,
        [Description("Train")] Train,
        [Description("Nuke")] Nuke,
        [Description("Inferno")] Inferno,
        [Description("Mirage")] Mirage,
        [Description("Dust2")] Dust2,
        [Description("Cobble")] Cobblestone,
        [Description("Vertigo")] Vertigo,
        [Description("Unknown")] Unknown
    }
}