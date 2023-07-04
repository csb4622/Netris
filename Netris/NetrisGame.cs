using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
        TextureManager.Current.Initialize(this.Content);
        
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
        _atlas = TextureManager.Current.GetTexture("Sheet");
        _font = Content.Load<SpriteFont>("Block");
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }
        
        // TODO: Add your update logic here
        if (_gameBoard.State == BoardState.Trapped)
        {
            Exit();            
        }
        else
        {
            _gameBoard.Update(gameTime, Keyboard.GetState());
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        _spriteBatch.Begin();
        
        for (var y = 0; y < _gameBoard.Dimensions.Height; ++y)
        {
            for (var x = 0; x < _gameBoard.Dimensions.Width; ++x)
            {
                if (_gameBoard.IsOccupied(x, y))
                {
                    _spriteBatch.Draw(
                        _atlas,
                        new Vector2(x*_gameBoard.CellSize, y*_gameBoard.CellSize),
                        new Rectangle(new Point((int)_gameBoard.GetTextureOffset(x,y)!.Value.X, (int)_gameBoard.GetTextureOffset(x,y)!.Value.Y), new Point(32)),
                        _gameBoard.GetColor(x, y)!.Value );
                }
            }
        }
        
        _spriteBatch.DrawString(_font, "HOLD", Vector2.One, Color.Blue);


        _spriteBatch.End();

        base.Draw(gameTime);
    }
}