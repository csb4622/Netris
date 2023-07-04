using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Netris.Pieces;
using Rectangle = System.Drawing.Rectangle;

namespace Netris;

public class Board
{
    private BoardState _state;
    private readonly Cell[] _area;
    private readonly Rectangle _dimensions;
    private readonly Rectangle _playAreaDimensions;
    private readonly int _playAreaOffsetX;
    private readonly int _playAreaOffsetY;
    private readonly int _cellSize;

    private readonly int _xSpawn;
    private readonly int _ySpawn;

    private FallingPiece? _fallingPiece;

    private int _fallSpeed;
    private int _currentFallTimer;
    private bool _bypassFallTimer;
    private int _level;
    private int _score;
    private int _linesCleared;
    
    private int _inputWait;
    private int _currentInputTimer;

    private readonly IList<Point> _clearingPieces;
    private readonly IList<int> _clearingRows;
    private readonly int _clearSpeed;
    

    public Board(int cellSize)
    {
        _fallingPiece = null;
        _currentFallTimer = 0;
        _fallSpeed = 1000;
        _level = 1;
        _score = 0;
        _linesCleared = 0;
        _clearingPieces = new List<Point>(4);
        _clearingRows = new List<int>(4);
        _clearSpeed = 25;

        _inputWait = 50;
        _currentInputTimer = 0;
        

        _state = BoardState.Playing;
        _cellSize = cellSize;
        _dimensions = new Rectangle(0, 0, 32, 22);
        _playAreaDimensions = new Rectangle(0, 0, 10, 20);
        _area = new Cell[_dimensions.Width*_dimensions.Height];
        _playAreaOffsetX = 11;
        _playAreaOffsetY = 1;


        _xSpawn = (_playAreaOffsetX - 1) + ((((_playAreaOffsetX + _playAreaDimensions.Width)) - _playAreaOffsetX) / 2);
        _ySpawn = _playAreaOffsetY;
        

         for (var x = _playAreaOffsetX-1; x <= _playAreaOffsetX+_playAreaDimensions.Width; ++x)
         {
             SetCell(x, _playAreaOffsetY-1, Color.White );
             SetCell(x, ((_playAreaOffsetY-1)+_playAreaDimensions.Height+1), Color.White );
         }
         for (var y = _playAreaOffsetY; y < _playAreaOffsetY+_playAreaDimensions.Height; ++y)
         {
             SetCell(_playAreaOffsetX-1, y, Color.White);
             SetCell(((_playAreaOffsetX-1)+_playAreaDimensions.Width+1), y, Color.White);
         } 
        
        
    }

    public Rectangle Dimensions => _dimensions;
    public int CellSize => _cellSize;
    public BoardState State => _state;

    
    public void Update(GameTime gameTime, KeyboardState keyboard)
    {
        if (_state == BoardState.Clearing)
        {
            ClearRows();
        }
        else if (_state == BoardState.Playing)
        {
           PlayFrame(gameTime.ElapsedGameTime.Milliseconds, keyboard);
        }
    }
    public bool IsOccupied(int x, int y)
    {
        return _area[y * _dimensions.Width + x].IsOccupied;
    }
    public Color? GetColor(int x, int y)
    {
        return _area[y * _dimensions.Width + x].Color.HasValue ? _area[y * _dimensions.Width + x].Color : null;
    }



    private void ResetData()
    {
        _bypassFallTimer = false;
    }
    private void HandleInputs(int milliseconds, KeyboardState keyboard)
    {
        _currentInputTimer += milliseconds;
        if (_currentInputTimer > _inputWait)
        {
            _currentInputTimer = 0;
            if (_fallingPiece != null)
            {
                if (keyboard.IsKeyDown(Keys.Up))
                {
                    TurnPiece();
                }

                if (keyboard.IsKeyDown(Keys.Left))
                {
                    MovePieceLeft();
                }

                if (keyboard.IsKeyDown(Keys.Right))
                {
                    MovePieceRight();
                }

                if (keyboard.IsKeyDown(Keys.Down))
                {
                    _bypassFallTimer = true;
                }
            }
        }
    }
    private void PlayFrame(int milliseconds, KeyboardState keyboard)
    {
        if (_fallingPiece == null)
        {
            SpawnPiece();
        }
        else
        {
            ResetData();
            HandleInputs(milliseconds, keyboard);

            _currentFallTimer += milliseconds;
            if (_bypassFallTimer || _currentFallTimer > _fallSpeed)
            {
                _currentFallTimer = 0;
                MovePieceDown();
            }
        }
    }
    private void ClearRows()
    {
        var allCleared = true;
        for (var i = 0; i < _clearingPieces.Count; ++i)
        {
            var oldColor = GetColor(_clearingPieces[i].X, _clearingPieces[i].Y);
            if (oldColor.Value.R == 255 && oldColor.Value.G == 255 && oldColor.Value.B == 255)
            {
                RemoveCell(_clearingPieces[i].X, _clearingPieces[i].Y);
            }
            else
            {
                var newRed = Math.Min(oldColor.Value.R + _clearSpeed, 255);
                var newGreen = Math.Min(oldColor.Value.G + _clearSpeed, 255);
                var newBlue = Math.Min(oldColor.Value.B + _clearSpeed, 255);
                SetColor(_clearingPieces[i].X, _clearingPieces[i].Y , new Color(newRed, newGreen, newBlue, oldColor.Value.A));
                allCleared = false;
            }
        }

        if (allCleared)
        {
            for (var i = 0; i < _clearingRows.Count; ++i)
            {
                MoveRowsDown(_clearingRows[i]);
            }

            _state = BoardState.Playing;
            _linesCleared += _clearingRows.Count;
            _score += 100*_clearingRows.Count;
            _clearingPieces.Clear();
            _clearingRows.Clear();
        }
    }
    private void SetColor(int x, int y, Color color)
    {
        _area[y * _dimensions.Width + x].Color = color;
    }

    private void MoveRowsDown(int stopRowIndex)
    {
        for (var y = stopRowIndex-1; y >= _playAreaOffsetY ; --y)
        {
            for (var x = _playAreaOffsetX; x < (+_playAreaOffsetX+_playAreaDimensions.Width); ++x)
            {
                if (IsOccupied(x, y))
                {
                    var id = y * _dimensions.Width + x;
                    var color = GetColor(x, y);
                    RemoveCell(x, y);
                    SetCell(x, y + 1, color!.Value);
                }
            }
        }
    }
    
    private void CheckForClearableRows()
    {
        for (var y = _playAreaOffsetY; y < (_playAreaOffsetY+_playAreaDimensions.Height); ++y)
        {
            var rowClearable = true;
            for (var x = _playAreaOffsetX; x < (_playAreaOffsetX+_playAreaDimensions.Width); ++x)
            {
                if (!IsOccupied(x, y))
                {
                    rowClearable = false;
                }
            }
            if (rowClearable)
            {
                for (var x = _playAreaOffsetX; x < (_playAreaOffsetX+_playAreaDimensions.Width); ++x)
                {
                    _clearingPieces.Add(new Point(x, y));
                }
                _clearingRows.Add(y);
            }
        }

        if (_clearingRows.Count > 0)
        {
            _state = BoardState.Clearing;
        }
    }
    
    private void RemoveCell(int x, int y)
    {
        _area[y * _dimensions.Width + x].IsOccupied = false;
    }
    
    private void SetCell(int x, int y, Color color)
    {
        _area[y * _dimensions.Width + x].IsOccupied = true;
        _area[y * _dimensions.Width + x].Color = color;
    }
    
    private void TurnPiece()
    {
        var currentRotation = _fallingPiece.GetCurrentRotation();
        var newRotation = currentRotation + 90;
        if (newRotation > 359)
        {
            newRotation = 0;
        }
        
        var offsets = _fallingPiece.GetRotationOffsets(newRotation);

        var currentCells = _fallingPiece.GetCurrentCells();
        var blocked = false;

        for (var i = 0; i < currentCells.Length; ++i)
        {
            var newX = currentCells[i].Point.X + offsets[currentCells[i].Number].X;
            var newY = currentCells[i].Point.Y + offsets[currentCells[i].Number].Y;

            if (IsOccupied(newX, newY) && !_fallingPiece.IsMyCell(newY * _dimensions.Width + newX))
            {
                blocked = true;
                break;
            }
        }

        if (!blocked)
        {
            // remove all of the current cells
            for (var i = 0; i < currentCells.Length; ++i)
            {
                RemoveCell(currentCells[i].Point.X, currentCells[i].Point.Y);
                _fallingPiece.RemoveCell(currentCells[i].Id, currentCells[i].Point.X, currentCells[i].Point.Y);
            }
            for (var i = 0; i < currentCells.Length; ++i)
            {
                var oldX = currentCells[i].Point.X;
                var oldY = currentCells[i].Point.Y;

                var newX = oldX + offsets[currentCells[i].Number].X;
                var newY = oldY + offsets[currentCells[i].Number].Y;
                var newId = newY * _dimensions.Width + newX;

                SetCell(newX, newY, currentCells[i].Color);
                _fallingPiece.AddCell(currentCells[i].Number, newId, newX, newY, currentCells[i].Color);
            }
            _fallingPiece.SetRotation(newRotation);
        }
    }
    
    private void MovePieceRight()
    {
        var minX = _fallingPiece.MinX;
        var maxX = _fallingPiece.MaxX;

        var blocked = false;
        for (var x = maxX; x >= minX; --x)
        {
            var cells = _fallingPiece.GetCellsYAtX(x);

            foreach (var cellY in cells)
            {
                if (IsOccupied(x+1, cellY) && !_fallingPiece.IsMyCell(cellY*_dimensions.Width+(x+1)))
                {
                    blocked = true;
                }
            }
        }

        if (!blocked)
        {
            // Move the piece down one
            for (var x = maxX; x >= minX; --x)
            {
                var cells = _fallingPiece.GetCellsYAtX(x);
                foreach (var cellY in cells)
                {
                    var id = cellY * _dimensions.Width + x;
                    var pieceCell = _fallingPiece.GetPieceCellById(id);
                    var color = GetColor(x, cellY);
                    RemoveCell(x, cellY);
                    SetCell(x+1, cellY, color!.Value);
                    _fallingPiece.MoveCell(pieceCell.Value.Number, id, x, cellY,
                        cellY * _dimensions.Width + (x+1), x+1, cellY, color.Value);
                }
            }
        }
    }
    
    private void MovePieceLeft()
    {
        var minX = _fallingPiece.MinX;
        var maxX = _fallingPiece.MaxX;

        var blocked = false;
        for (var x = minX; x <= maxX; ++x)
        {
            var cells = _fallingPiece.GetCellsYAtX(x);

            foreach (var cellY in cells)
            {
                if (IsOccupied(x-1, cellY) && !_fallingPiece.IsMyCell(cellY*_dimensions.Width+(x-1)))
                {
                    blocked = true;
                }
            }
        }

        if (!blocked)
        {
            // Move the piece down one
            for (var x = minX; x <= maxX; ++x)
            {
                var cells = _fallingPiece.GetCellsYAtX(x);
                foreach (var cellY in cells)
                {
                    var id = cellY * _dimensions.Width + x;
                    var pieceCell = _fallingPiece.GetPieceCellById(id);
                    var color = GetColor(x, cellY);
                    RemoveCell(x, cellY);
                    SetCell(x-1, cellY, color!.Value);
                    _fallingPiece.MoveCell(pieceCell.Value.Number, id, x, cellY,
                        cellY * _dimensions.Width + (x-1), x-1, cellY, color.Value);
                }
            }
        }
    }
    
    private void MovePieceDown()
    {
        var minY = _fallingPiece.MinY;
        var maxY = _fallingPiece.MaxY;

        var stopped = false;
        for (var y = maxY; y >= minY; --y)
        {
            var cells = _fallingPiece.GetCellsXAtY(y);

            foreach (var fallingCellX in cells)
            {
                if (IsOccupied(fallingCellX, y + 1) && !_fallingPiece.IsMyCell((y+1)*_dimensions.Width+fallingCellX))
                {
                    stopped = true;
                }
            }
        }

        if (!stopped)
        {
            // Move the piece down one
            for (var y = maxY; y >= minY; --y)
            {
                var cells = _fallingPiece.GetCellsXAtY(y);
                foreach (var fallingCellX in cells)
                {
                    var id = y * _dimensions.Width + fallingCellX;
                    var pieceCell = _fallingPiece.GetPieceCellById(id);
                    var color = GetColor(fallingCellX, y);
                    RemoveCell(fallingCellX, y);
                    SetCell(fallingCellX, y + 1, color!.Value);
                    _fallingPiece.MoveCell(pieceCell.Value.Number, id, fallingCellX, y,
                        (y + 1) * _dimensions.Width + fallingCellX, fallingCellX, y + 1, color.Value);
                }
            }
        }
        else
        {
            _fallingPiece = null;
            CheckForClearableRows();
        }
    }
    
    private void SpawnPiece()
    {
        var nextPiece = 3;//Random.Shared.Next(0, 7);
        Piece? piece = null;

        switch (nextPiece)
        {
            case 0:
                piece = new Bar();
                break;
            case 1:
                piece = new LeftEll();
                break;
            case 2:
                piece = new RightEll();
                break;
            case 3:
                piece = new Square();
                break;
            case 4:
                piece = new RightStair();
                break;
            case 5:
                piece = new Tee();
                break;
            case 6:
                piece = new LeftStair();
                break;
            default:
                return;
        }

        _fallingPiece = new FallingPiece(piece);

        var offsets = piece.CellOffsets;
        for (var i = 0; i < offsets.Length; ++i)
        {
            var x = _xSpawn + offsets[i].X;
            var y = _ySpawn + offsets[i].Y;
            if (IsOccupied(x, y))
            {
                _state = BoardState.Trapped;
                return;
            }
            else
            {
                _fallingPiece.AddCell(i, y*_dimensions.Width+x,  x, y, piece.Color);
                SetCell(x, y, piece.Color);
            }
        }

    }
}