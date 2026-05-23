namespace Alca.MonoGame.Kernel.UI;

public class UIRoot : UIContainer
{
    public void DrawAll(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            depthStencilState: null,
            rasterizerState: null);
        Draw(spriteBatch);
        spriteBatch.End();
    }
}
