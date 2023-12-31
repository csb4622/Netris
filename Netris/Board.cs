﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Netris.Pieces;
using Rectangle = System.Drawing.Rectangle;

namespace Netris;

public class Board
{
    private BoardState _state;
    private readonly Cell[] _area;
    private readonly Rectangle _dimensions;
    private readonly Rectangle _playAreaDimensions;
    private Vector2 _scoreLocation;
    private Vector2 _linesLocation;
    private Vector2 _levelLocation;
    private Vector2 _defaultTextureOffset;
    private readonly int _playAreaOffsetX;
    private readonly int _playAreaOffsetY;
    private readonly int _cellSize;

    private readonly int _xSpawn;
    private readonly int _ySpawn;

    private FallingPiece? _fallingPiece;
    private Piece? _nextPiece;
    private Piece? _holdPiece;
    private int? _lastPiece;

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
    
    private Song? _menuSong;
    private Song? _gameSong;

    public Board(int cellSize)
    {
        _defaultTextureOffset = Vector2.Zero;
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
        

        _state = BoardState.Ready;
        _cellSize = cellSize;
        _dimensions = new Rectangle(0, 0, 32, 22);
        _playAreaDimensions = new Rectangle(0, 0, 10, 20);
        _area = new Cell[_dimensions.Width*_dimensions.Height];
        _playAreaOffsetX = 11;
        _playAreaOffsetY = 1;


        _xSpawn = (_playAreaOffsetX - 1) + ((((_playAreaOffsetX + _playAreaDimensions.Width)) - _playAreaOffsetX) / 2);
        _ySpawn = _playAreaOffsetY;
        
        CreateBoarders();

    }

    public void Initialize(Song menuSong, Song gameSong)
    {
        _menuSong = menuSong;
        _gameSong = gameSong;

        MediaPlayer.Play(_menuSong);
    }

    public Rectangle Dimensions => _dimensions;
    public int CellSize => _cellSize;
    public BoardState State => _state;
    public Vector2 ScoreLocation => _scoreLocation;
    public Vector2 LinesLocation => _linesLocation;
    public Vector2 LevelLocation => _levelLocation;
    public int Score => _score;
    public int LinesCleared => _linesCleared;
    public int Level => _level;

    
    public void Update(GameTime gameTime)
    {
        if (_state is BoardState.Ready or BoardState.Trapped)
        { ;
            if (Keyboard.HasBeenPressed(Keys.Enter))
            {
                _state = BoardState.Playing;
                ResetBoard();
                MediaPlayer.Stop();
                MediaPlayer.Play(_gameSong);
            }
        }
        if (_state == BoardState.Paused)
        {
            if (Keyboard.HasBeenPressed(Keys.Enter))
            {
                _state = BoardState.Playing;
                MediaPlayer.Resume();
            }
        }        
        else if (_state == BoardState.Clearing)
        {
            ClearRows();
        }
        else if (_state == BoardState.Playing)
        {
           PlayFrame(gameTime.ElapsedGameTime.Milliseconds);
        }
    }

    public void ResetBoard()
    {
        _fallingPiece = null;
        _holdPiece = null;
        _nextPiece = null;
        _lastPiece = null;
        
        _defaultTextureOffset = Vector2.Zero;
        _currentFallTimer = 0;
        _level = 1;
        _score = 0;
        _linesCleared = 0;
        _currentInputTimer = 0;
        
        ClearHoldArea();
        ClearNextArea();
        ClearPlayArea();
    }
    public bool IsOccupied(int x, int y)
    {
        return _area[y * _dimensions.Width + x].IsOccupied;
    }
    public Color? GetColor(int x, int y)
    {
        return _area[y * _dimensions.Width + x].Color.HasValue ? _area[y * _dimensions.Width + x].Color : null;
    }
    public Vector2? GetTextureOffset(int x, int y)
    {
        return _area[y * _dimensions.Width + x].TextureOffset.HasValue ? _area[y * _dimensions.Width + x].TextureOffset : null;
    }
    
    private void CreateBoarders()
    {
        // Create outer wall
        for (var x = _playAreaOffsetX-1; x <= _playAreaOffsetX+_playAreaDimensions.Width; ++x)
        {
            SetCell(x, _playAreaOffsetY-1, Color.White, Vector2.Zero );
            SetCell(x, ((_playAreaOffsetY-1)+_playAreaDimensions.Height+1), Color.White, Vector2.Zero );
        }
        for (var y = _playAreaOffsetY; y < _playAreaOffsetY+_playAreaDimensions.Height; ++y)
        {
            SetCell(_playAreaOffsetX-1, y, Color.White, Vector2.Zero);
            SetCell(((_playAreaOffsetX-1)+_playAreaDimensions.Width+1), y, Color.White, Vector2.Zero);
        }

        
        // Create Hold Area
        for (var holdX = -2; holdX >= -8; --holdX)
        {
            SetCell(_playAreaOffsetX+holdX, _playAreaOffsetY-1, Color.White, Vector2.Zero);
            SetCell(_playAreaOffsetX+holdX, _playAreaOffsetY+1, Color.White, Vector2.Zero);
            SetCell(_playAreaOffsetX+holdX, _playAreaOffsetY+8, Color.White, Vector2.Zero);
        }
        
        
        // Create Next Area
        for (var nextX = 1; nextX < 8; ++nextX)
        {
            SetCell(_playAreaOffsetX+_playAreaDimensions.Width+nextX, _playAreaOffsetY-1, Color.White, Vector2.Zero);
            SetCell(_playAreaOffsetX+_playAreaDimensions.Width+nextX, _playAreaOffsetY+1, Color.White, Vector2.Zero);
            SetCell(_playAreaOffsetX+_playAreaDimensions.Width+nextX, _playAreaOffsetY+8, Color.White, Vector2.Zero);
        }

        for (var y = 0; y < 9; ++y)
        {
            SetCell(_playAreaOffsetX-8, _playAreaOffsetY-1 + y, Color.White, Vector2.Zero);
            SetCell(_playAreaOffsetX+_playAreaDimensions.Width+7, _playAreaOffsetY-1 + y, Color.White, Vector2.Zero);
        }

        // Create Hold Text
        SetCell(_playAreaOffsetX-7, _playAreaOffsetY, Color.Aqua, Vector2.Zero);
        for (var x = -6; x < -2; ++x)
        {
            var offsetX = ((x + 6) * _cellSize)+_cellSize;
            SetCell(_playAreaOffsetX+x, _playAreaOffsetY, Color.Aqua, new Vector2(offsetX, 0));
        }
        SetCell(_playAreaOffsetX-2, _playAreaOffsetY, Color.Aqua, Vector2.Zero);
        
        
        // Create Next Text
        SetCell(_playAreaOffsetX+_playAreaDimensions.Width+1, _playAreaOffsetY, Color.Red, Vector2.Zero);
        for (var x = 2; x < 6; ++x)
        {
            var offsetX = ((x - 2) * _cellSize)+_cellSize;
            SetCell(_playAreaOffsetX+_playAreaDimensions.Width+x, _playAreaOffsetY, Color.Red, new Vector2(offsetX, _cellSize));
        }
        SetCell(_playAreaOffsetX+_playAreaDimensions.Width+6, _playAreaOffsetY, Color.Red, Vector2.Zero);  
        
        
        // Create Stats Area
        for(var x= -10; x < 0; ++x)
        {
            SetCell(_playAreaOffsetX+x, _playAreaDimensions.Height-5, Color.White, Vector2.Zero);
            SetCell(_playAreaOffsetX+x, _playAreaDimensions.Height-3, Color.White, Vector2.Zero);
            SetCell(_playAreaOffsetX+x, _playAreaDimensions.Height-1, Color.White, Vector2.Zero);
            SetCell(_playAreaOffsetX+x, _playAreaDimensions.Height+1, Color.White, Vector2.Zero);
        }

        for (var y = -5; y < 2; ++y)
        {
            SetCell(_playAreaOffsetX-11, _playAreaDimensions.Height+y, Color.White, Vector2.Zero);
        }
        // Stats Text
        {
            for (var x = -10; x < -5; ++x)
            {
                var offsetX = ((x+10) * _cellSize);
                SetCell(_playAreaOffsetX+x, _playAreaDimensions.Height-4, Color.Yellow, new Vector2(offsetX, _cellSize*2));
                SetCell(_playAreaOffsetX+x, _playAreaDimensions.Height-2, Color.LimeGreen, new Vector2(offsetX, _cellSize*3));
                SetCell(_playAreaOffsetX+x, _playAreaDimensions.Height, Color.MediumPurple, new Vector2(offsetX, _cellSize*4));
            }

            _scoreLocation = new Vector2((_playAreaOffsetX-5)*_cellSize+8, (_playAreaDimensions.Height-4)*_cellSize);
            _linesLocation = new Vector2((_playAreaOffsetX-5)*_cellSize+8, (_playAreaDimensions.Height-2)*_cellSize);
            _levelLocation = new Vector2((_playAreaOffsetX-5)*_cellSize+8, (_playAreaDimensions.Height)*_cellSize);
        }
        
    }
    private void ResetData()
    {
        _bypassFallTimer = false;
    }
    private void HandleInputs(int milliseconds)
    {
        if (Keyboard.HasBeenPressed(Keys.Enter))
        {
            _state = BoardState.Paused;
            MediaPlayer.Pause();
            return;
        }

        if (_fallingPiece != null)
        {
            if (Keyboard.HasBeenPressed(Keys.Up))
            {
                TurnPiece();
            }
            if (Keyboard.HasBeenPressed(Keys.Space))
            {
                SwitchPieceForHold();
            }
        }
        
        _currentInputTimer += milliseconds;
        if (_currentInputTimer > _inputWait)
        {
            _currentInputTimer = 0;
            if (_fallingPiece != null)
            {
                if (Keyboard.IsPressed(Keys.Left))
                {
                    MovePieceLeft();
                }

                if (Keyboard.IsPressed(Keys.Right))
                {
                    MovePieceRight();
                }

                if (Keyboard.IsPressed(Keys.Down))
                {
                    _bypassFallTimer = true;
                }
            }
        }
    }

    private void SwitchPieceForHold()
    {
        if (_fallingPiece != null && _fallingPiece.CanSwitchForHold())
        {
            if (_holdPiece == null)
            {
                _holdPiece = _fallingPiece.GetPiece().Clone();
                ClearFallingPiece();
                GetNextPiece();

            }
            else
            {
                var oldHoldPiece = _holdPiece;
                _holdPiece = _fallingPiece.GetPiece().Clone();
                ClearFallingPiece();
                SpawnPiece(oldHoldPiece.Clone());

            }
            _fallingPiece.SetSwitchForHold(true);
            UpdateHoldArea();
        }
    }

    private void ClearFallingPiece()
    {
        var cells = _fallingPiece.GetCurrentCells();
        for (var i = 0; i < cells.Length; ++i)
        {
            RemoveCell(cells[i].Point.X, cells[i].Point.Y);
        }
        _fallingPiece = null;
    }
    
    private void GetNextPiece()
    {
        if (_nextPiece == null)
        {
            _nextPiece = GetRandomPiece();
        }
        SpawnPiece(_nextPiece.Clone());
        _nextPiece = GetRandomPiece();
        UpdateNextArea();
    }
    
    private void PlayFrame(int milliseconds)
    {
        if (_fallingPiece == null)
        {
            GetNextPiece();
        }
        else
        {
            ResetData();
            HandleInputs(milliseconds);

            _currentFallTimer += milliseconds;
            if (_bypassFallTimer || _currentFallTimer > (_fallSpeed-(_level*5)))
            {
                _currentFallTimer = 0;
                MovePieceDown();
            }
        }
    }

    private void ClearNextArea()
    {
        for (var y = 2; y < 8; ++y)
        {
            for (var x = 11; x < 17; ++x)
            {
                RemoveCell(_playAreaOffsetX+x, _playAreaOffsetY+y);
            }   
        }
    }

    private void ClearPlayArea()
    {
        for (var y = _playAreaOffsetY; y < _playAreaOffsetY+_playAreaDimensions.Height; ++y)
        {
            for (var x = _playAreaOffsetX; x < _playAreaOffsetX+_playAreaDimensions.Width; ++x)
            {
                if (IsOccupied(x, y))
                {
                    RemoveCell(x, y);
                }
            }
        }
    }

    private void UpdateNextArea()
    {
        ClearNextArea();
        var offsets = _nextPiece.CellOffsets;
        var startX = (_playAreaOffsetX+_nextPiece.DisplayOffset.X)+14;
        var startY = (_playAreaOffsetY+_nextPiece.DisplayOffset.Y)+3;
        for (var i = 0; i < offsets.Length; ++i)
        {
            var x = startX + offsets[i].X;
            var y = startY + offsets[i].Y;
            SetCell(x, y, _nextPiece.Color, _defaultTextureOffset);
        }
    }

    private void ClearHoldArea()
    {
        for (var y = 2; y < 8; ++y)
        {
            for (var x = -7; x < -1; ++x)
            {
                RemoveCell(_playAreaOffsetX+x, _playAreaOffsetY+y);
            }   
        }
    }

    private void UpdateHoldArea()
    {
        ClearHoldArea();
        var offsets = _holdPiece.CellOffsets;
        var startX = (_playAreaOffsetX + _holdPiece.DisplayOffset.X)-4;
        var startY = (_playAreaOffsetY + _holdPiece.DisplayOffset.Y)+3;
        for (var i = 0; i < offsets.Length; ++i)
        {
            var x = startX + offsets[i].X;
            var y = startY + offsets[i].Y;
            SetCell(x, y, _holdPiece.Color, _defaultTextureOffset);
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
            _level = (_linesCleared / 10)+1;
            if (_level > 10 && _defaultTextureOffset.Y == 0)
            {
                SetTextures(new Vector2(0, _cellSize));
            }
        }
    }

    private void SetTextures(Vector2 newTextureOffset)
    {
        for (var y = _playAreaOffsetY; y < _playAreaOffsetY+_playAreaDimensions.Height; ++y)
        {
            for (var x = _playAreaOffsetX; x < _playAreaOffsetX+_playAreaDimensions.Width; ++x)
            {
                if (IsOccupied(x, y))
                {
                    SetTextureOffset(x, y, newTextureOffset);
                }
            }
        }
        for (var y = 2; y < 8; ++y)
        {
            for (var x = -7; x < -1; ++x)
            {
                if (IsOccupied(_playAreaOffsetX+x, _playAreaOffsetY+y))
                {
                    SetTextureOffset(_playAreaOffsetX+x, _playAreaOffsetY+y, newTextureOffset);
                }
            }   
        }
        for (var y = 2; y < 8; ++y)
        {
            for (var x = 11; x < 17; ++x)
            {
                if (IsOccupied(_playAreaOffsetX+x, _playAreaOffsetY+y))
                {
                    SetTextureOffset(_playAreaOffsetX+x, _playAreaOffsetY+y, newTextureOffset);
                }
            }
        }
        SetDefaultTextureOffset(newTextureOffset);        
    }
    private void SetColor(int x, int y, Color color)
    {
        _area[y * _dimensions.Width + x].Color = color;
    }
    private void SetTextureOffset(int x, int y, Vector2 textureOffset)
    {
        _area[y * _dimensions.Width + x].TextureOffset = textureOffset;
    }

    private void SetDefaultTextureOffset(Vector2 textureOffset)
    {
        _defaultTextureOffset = textureOffset;
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
                    var textureOffset = GetTextureOffset(x, y);
                    RemoveCell(x, y);
                    SetCell(x, y + 1, color!.Value, textureOffset!.Value);
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
    
    private void SetCell(int x, int y, Color color, Vector2 textureOffset)
    {
        _area[y * _dimensions.Width + x].IsOccupied = true;
        _area[y * _dimensions.Width + x].Color = color;
        _area[y * _dimensions.Width + x].TextureOffset = textureOffset;
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

                SetCell(newX, newY, currentCells[i].Color, currentCells[i].TextureOffset);
                _fallingPiece.AddCell(currentCells[i].Number, newId, newX, newY, currentCells[i].Color, currentCells[i].TextureOffset);
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
                    var textureOffset = GetTextureOffset(x, cellY);
                    RemoveCell(x, cellY);
                    SetCell(x+1, cellY, color!.Value, textureOffset!.Value);
                    _fallingPiece.MoveCell(pieceCell.Value.Number, id, x, cellY,
                        cellY * _dimensions.Width + (x+1), x+1, cellY, color.Value, textureOffset.Value);
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
                    var textureOffset = GetTextureOffset(x, cellY);
                    RemoveCell(x, cellY);
                    SetCell(x-1, cellY, color!.Value, textureOffset!.Value);
                    _fallingPiece.MoveCell(pieceCell.Value.Number, id, x, cellY,
                        cellY * _dimensions.Width + (x-1), x-1, cellY, color.Value, textureOffset.Value);
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
                    var textureOffset = GetTextureOffset(fallingCellX, y);
                    RemoveCell(fallingCellX, y);
                    SetCell(fallingCellX, y + 1, color!.Value, textureOffset!.Value);
                    _fallingPiece.MoveCell(pieceCell.Value.Number, id, fallingCellX, y,
                        (y + 1) * _dimensions.Width + fallingCellX, fallingCellX, y + 1, color.Value, textureOffset.Value);
                }
            }
        }
        else
        {
            _fallingPiece = null;
            CheckForClearableRows();
        }
    }

    private Piece GetRandomPiece()
    {
        var nextPiece = Random.Shared.Next(0, 7);
        
        while (nextPiece == _lastPiece)
        {
            nextPiece = Random.Shared.Next(0, 7);
        }

        _lastPiece = nextPiece;
        Piece? piece = null;

        switch (nextPiece)
        {
            case 0:
                return new Bar();
            case 1:
                return new LeftEll();
            case 2:
                return new RightEll();
            case 3:
                return new Square();
            case 4:
                return new RightStair();
            case 5:
                return new Tee();
                
            case 6:
                return new LeftStair();
                
            default:
                return new Square();
        }
    }
    private void SpawnPiece(Piece piece)
    {
        _fallingPiece = new FallingPiece(piece);

        var offsets = piece.CellOffsets;
        for (var i = 0; i < offsets.Length; ++i)
        {
            var x = _xSpawn + offsets[i].X;
            var y = _ySpawn + offsets[i].Y;
            if (IsOccupied(x, y))
            {
                _state = BoardState.Trapped;
                MediaPlayer.Stop();
                MediaPlayer.Play(_menuSong);
                return;
            }
            else
            {
                _fallingPiece.AddCell(i, y*_dimensions.Width+x,  x, y, piece.Color, _defaultTextureOffset);
                SetCell(x, y, piece.Color, _defaultTextureOffset);
            }
        }

    }
}