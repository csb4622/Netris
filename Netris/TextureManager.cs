using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Netris;

public class TextureManager
{
    private static TextureManager? _instance;
    
    private ContentManager? _content;
    private bool _initialized;

    private IDictionary<string, Texture2D> _localTexture;
    private IDictionary<string, Texture2D> _globalTexture;

    private TextureManager()
    {
        _localTexture = new Dictionary<string, Texture2D>();
        _globalTexture = new Dictionary<string, Texture2D>();
    }
    
    public static TextureManager Current => _instance ??= new TextureManager();

    public bool Initialize(ContentManager content)
    {
        if (!_initialized)
        {
            _initialized = true;
            _content = content;
        }
        return _initialized;
    }

    public Texture2D LoadTexture(string textureName)
    {
        var texture = _content!.Load<Texture2D>(textureName);
        _localTexture[textureName] = texture;

        return texture;
    }

    public Texture2D LoadGlobalTexture(string textureName)
    {
        var texture = _content!.Load<Texture2D>(textureName);
        _globalTexture[textureName] = texture;

        return texture;        
    }

    public void UnloadTexture(string textureName)
    {
        if (_localTexture.Remove(textureName))
        {
            _content!.UnloadAsset(textureName);
        }
    }
    
    public void UnloadGlobalTexture(string textureName)
    {
        if (_globalTexture.Remove(textureName))
        {
            _content!.UnloadAsset(textureName);
        }
    }    
    
    public void UnloadTextures()
    {
        foreach (var texture in _localTexture)
        {
            _content!.UnloadAsset(texture.Key);
        }
        _localTexture.Clear();
    }
    
    public void UnloadGlobalTextures()
    {
        foreach (var texture in _globalTexture)
        {
            _content!.UnloadAsset(texture.Key);
        }
        _globalTexture.Clear();
    }

    public Texture2D GetTexture(string textureName)
    {
        return _localTexture.TryGetValue(textureName, out var texture)
            ? texture
            : LoadTexture(textureName);
    }
}