using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Netris;

public class NetrisGame : Game
{
    private Board _gameBoard;
    private Texture2D _atlas;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private SpriteFont _font;

    public NetrisGame()
    {
        this.Window.Title = "Netris";
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        _gameBoard = new Board(32);

        _graphics.PreferredBackBufferWidth = 32 * 32;
        _graphics.PreferredBackBufferHeight = 22 * 32;
        _graphics.ApplyChanges();
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        _atlas = Content.Load<Texture2D>("Sheet");
        _font = Content.Load<SpriteFont>("Block");
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume -= 0.5f;
        _gameBoard.Initialize(
            Content.Load<Song>("Menu"),
            Content.Load<Song>("Game")
            );
    }

    protected override void Update(GameTime gameTime)
    {
        Keyboard.GetState();
        if (Keyboard.IsPressed(Keys.Escape))
        {
            Exit();
        }
        
        // TODO: Add your update logic here
        _gameBoard.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code here
        _spriteBatch.Begin();
        if (_gameBoard.State == BoardState.Paused)
        {
            
        }
        else if (_gameBoard.State == BoardState.Ready)
        {
            var text = $"Press Enter to Play or ESC to exit";

            var textDimensions = _font.MeasureString(text);
            var x = (_graphics.PreferredBackBufferWidth-(textDimensions.X)) / 2;
            var y = (_graphics.PreferredBackBufferHeight-(textDimensions.Y)) / 2;
            _spriteBatch.DrawString(
                _font,
                text,
                new Vector2(x, y),
                Color.White);           
            
        }
        else if (_gameBoard.State == BoardState.Trapped)
        {
            var text = $"Your final score was {_gameBoard.Score.ToString()} Press Enter to try again or ESC to exit";
            var textDimensions = _font.MeasureString(text);
            var x = (_graphics.PreferredBackBufferWidth-(textDimensions.X)) / 2;
            var y = (_graphics.PreferredBackBufferHeight-(textDimensions.Y)) / 2;
            _spriteBatch.DrawString(
                _font,
                text,
                new Vector2(x, y),
                Color.White);
        }        
        else if (_gameBoard.State is BoardState.Clearing or BoardState.Playing)
        {
            for (var y = 0; y < _gameBoard.Dimensions.Height; ++y)
            {
                for (var x = 0; x < _gameBoard.Dimensions.Width; ++x)
                {
                    if (_gameBoard.IsOccupied(x, y))
                    {
                        _spriteBatch.Draw(
                            _atlas,
                            new Vector2(x * _gameBoard.CellSize, y * _gameBoard.CellSize),
                            new Rectangle(
                                new Point((int)_gameBoard.GetTextureOffset(x, y)!.Value.X,
                                    (int)_gameBoard.GetTextureOffset(x, y)!.Value.Y), new Point(32)),
                            _gameBoard.GetColor(x, y)!.Value);
                    }
                }
            }

            _spriteBatch.DrawString(_font, _gameBoard.Score.ToString(), _gameBoard.ScoreLocation, Color.Blue);
            _spriteBatch.DrawString(_font, _gameBoard.LinesCleared.ToString(), _gameBoard.LinesLocation, Color.Blue);
            _spriteBatch.DrawString(_font, _gameBoard.Level.ToString(), _gameBoard.LevelLocation, Color.Blue);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}