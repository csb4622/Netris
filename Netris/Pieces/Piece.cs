using Microsoft.Xna.Framework;

namespace Netris.Pieces;

public abstract class Piece
{
    public abstract Color Color { get; }
    public abstract Point[] CellOffsets { get; }
    public abstract Point[] RotatedOffsets(int rotation);
}