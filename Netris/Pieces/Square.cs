using Microsoft.Xna.Framework;

namespace Netris.Pieces;

public class Square : Piece
{
    public override Color Color => Color.Yellow;
    public override Point[] CellOffsets => new[]
    {
        new Point(0, 0),
        new Point(1, 0),
        new Point(1, 1),
        new Point(0, 1)
    };

    public override Point[] RotatedOffsets(int rotation)
    {
        return new []
        {
            new Point(0, 0),
            new Point(0, 0),
            new Point(0, 0),
            new Point(0, 0)
        };
    }
}