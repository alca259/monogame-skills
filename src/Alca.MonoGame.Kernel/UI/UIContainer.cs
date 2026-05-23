namespace Alca.MonoGame.Kernel.UI;

public class UIContainer : UIElement
{
    protected readonly List<UIElement> Children = [];

    public void Add(UIElement element) => Children.Add(element);
    public void Remove(UIElement element) => Children.Remove(element);

    public override void Update(GameTime gameTime)
    {
        for (int i = 0; i < Children.Count; i++)
            Children[i].Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;
        for (int i = 0; i < Children.Count; i++)
            if (Children[i].IsVisible)
                Children[i].Draw(spriteBatch);
    }
}
