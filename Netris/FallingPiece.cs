using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netris.Pieces;

namespace Netris;

public class FallingPiece
{
    private IDictionary<int, PieceCell> _cellsById;
    private IDictionary<int, IList<int>> _cellsByHeight;
    private IDictionary<int, IList<int>> _cellsByWidth;
    private int _currentRotation;
    private readonly Piece _piece;
    private bool _switchedForHold;
        
    public int MinY { get; private set; }
    public int MaxY { get; private set; }
    
    public int MinX { get; private set; }
    public int MaxX { get; private set; }    

    public FallingPiece(Piece piece)
    {
        _switchedForHold = false;
        _piece = piece;
        _currentRotation = 0;
        _cellsById = new Dictionary<int, PieceCell>(4);
        _cellsByHeight = new Dictionary<int, IList<int>>(4);
        _cellsByWidth = new Dictionary<int, IList<int>>(4);
    }

    public void SetSwitchForHold(bool newValue)
    {
        _switchedForHold = newValue;
    }
    
    public bool CanSwitchForHold()
    {
        return !_switchedForHold;
    }
    
    public PieceCell[] GetCurrentCells()
    {
        return _cellsById.Values.ToArray();
    }
    public int GetCurrentRotation()
    {
        return _currentRotation;
    }

    public Piece GetPiece()
    {
        return _piece;
    }
    
    public void SetRotation(int rotation)
    {
        _currentRotation = rotation;
    }

    public Point[] GetRotationOffsets(int newRotation)
    {
        return _piece.RotatedOffsets(newRotation);
    }
    
    public PieceCell? GetPieceCellById(int id)
    {
        if (_cellsById.TryGetValue(id, out var pieceCell))
        {
            return pieceCell;
        }
        return null;
    }
    public bool IsMyCell(int id)
    {
        return _cellsById.ContainsKey(id);
    }

    public void RemoveCell(int id, int x, int y)
    {
        _cellsById.Remove(id);
        var heightList = _cellsByHeight[y];
        if (heightList.Count == 1)
        {
            _cellsByHeight.Remove(y);
        }
        else
        {
            heightList.Remove(x);
        }
        
        var widthList = _cellsByWidth[x];
        if (widthList.Count == 1)
        {
            _cellsByWidth.Remove(x);
        }
        else
        {
            widthList.Remove(y);
        }
    }
    public void MoveCell(int number, int oldId, int oldX, int oldY, int newId, int newX, int newY, Color color, Vector2 textureOffset)
    {
        RemoveCell(oldId, oldX, oldY);
        AddCell(number, newId, newX, newY, color, textureOffset);
    }

    public void AddCell(int number, int id, int x, int y, Color color, Vector2 textureOffset)
    {
        _cellsById[id] = new PieceCell(number, id, new Point(x, y), color, textureOffset);

        if (_cellsByHeight.ContainsKey(y))
        {
            var list = _cellsByHeight[y];
            list.Add(x);
        }
        else
        {
            _cellsByHeight.Add(y, new List<int>(4){x});
        }
        if (_cellsByWidth.ContainsKey(x))
        {
            var list = _cellsByWidth[x];
            list.Add(y);
        }
        else
        {
            _cellsByWidth.Add(x, new List<int>(4){y});
        }       
        

        MinY = _cellsByHeight.Keys.Min();
        MaxY = _cellsByHeight.Keys.Max();
        MinX = _cellsByWidth.Keys.Min();
        MaxX = _cellsByWidth.Keys.Max();        
    }

    public IEnumerable<int> GetCellsYAtX(int x)
    {
        return _cellsByWidth[x].ToArray();
    }    
    
    public IEnumerable<int> GetCellsXAtY(int y)
    {
        return _cellsByHeight[y].ToArray();
    }
}

public struct PieceCell
{
    public int Number { get; }
    public int Id { get; }
    public Point Point { get; }
    public Color Color { get; }
    public Vector2 TextureOffset { get; }

    public PieceCell(int number, int id, Point point, Color color, Vector2 textureOffset)
    {
        Number = number;
        Id = id;
        Point = point;
        Color = color;
        TextureOffset = textureOffset;
    }
}