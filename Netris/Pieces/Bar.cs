using Microsoft.Xna.Framework;

namespace Netris.Pieces;

public class Bar : Piece
{
    public override Color Color => Color.Aqua;
    public override Point[] CellOffsets => new[]
    {
        new Point(0, 0),
        new Point(0, 1),
        new Point(0, 2),
        new Point(0, 3)
    };

    public override Point[] RotatedOffsets(int rotation)
    {
        switch (rotation)
        {
            case 0:
                return new[]
                {
                    new Point(2, -1),
                    new Point(1, 0),
                    new Point(0, 1),
                    new Point(-1, 2)
                };
            case 90:
                return new[]
                {
                    new Point(1, 2),
                    new Point(0, 1),
                    new Point(-1, 0),
                    new Point(-2, -1)
                };
            case 180:
                return new[]
                {
                    new Point(-2, 1),
                    new Point(-1, 0),
                    new Point(0, -1),
                    new Point(1, -2)
                };
            case 270:
                return new[]
                {
                    new Point(-1, -2),
                    new Point(0, -1),
                    new Point(1, 0),
                    new Point(2, 1)
                };          
        }
        return CellOffsets;
    }

    public override Piece Clone()
    {
        return new Bar();
    }
}