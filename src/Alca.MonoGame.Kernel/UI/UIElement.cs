namespace Alca.MonoGame.Kernel.UI;

public abstract class UIElement
{
    public bool IsVisible { get; set; } = true;

    public virtual void Update(GameTime gameTime) { }
    public abstract void Draw(SpriteBatch spriteBatch);
}
