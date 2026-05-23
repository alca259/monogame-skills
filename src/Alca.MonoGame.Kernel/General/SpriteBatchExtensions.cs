namespace MonoGameLibrary.General;

public static class SpriteBatchExtensions
{
    public static void DrawLine(this SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 end, Color color, float thickness = 1f)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);
        spriteBatch.Draw(
            texture: texture,
            position: start,
            sourceRectangle: null,
            color: color,
            rotation: angle,
            origin: Vector2.Zero,
            scale: new Vector2(edge.Length(), thickness),
            effects: SpriteEffects.None,
            layerDepth: 0);
    }
}