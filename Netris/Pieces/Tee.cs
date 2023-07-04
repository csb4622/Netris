using Microsoft.Xna.Framework;

namespace Netris.Pieces;

public class Tee : Piece
{
    public override Color Color => Color.MediumPurple;
    public override Point[] CellOffsets => new[]
    {
        new Point(0, 0),
        new Point(1, 0),
        new Point(2, 0),
        new Point(1, 1)
    };

    public override Point[] RotatedOffsets(int rotation)
    {
        switch (rotation)
        {
            case 0:
                return new[]
                {
                    new Point(-1, -1),
                    new Point(0, 0),
                    new Point(1, 1),
                    new Point(-1, 1)
                };
            case 90:
                return new[]
                {
                    new Point(1, -1),
                    new Point(0, 0),
                    new Point(-1, 1),
                    new Point(-1, -1)
                };
            case 180:
                return new[]
                {
                    new Point(1, 1),
                    new Point(0, 0),
                    new Point(-1, -1),
                    new Point(1, -1)
                };
            case 270:
                return new[]
                {
                    new Point(-1, 1),
                    new Point(0, 0),
                    new Point(1, -1),
                    new Point(1, 1)
                };          
        }
        return CellOffsets;
    }
    
    public override Piece Clone()
    {
        return new Tee();
    }
}