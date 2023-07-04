using Microsoft.Xna.Framework;

namespace Netris;

public struct Cell
{
    public bool IsOccupied { get; set; }
    public Color? Color { get; set; }
    public Vector2? TextureOffset { get; set; }

    public Cell()
    {
        IsOccupied = false;
        Color = null;
        TextureOffset = null;
    }
}