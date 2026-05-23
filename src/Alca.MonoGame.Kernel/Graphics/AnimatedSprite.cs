namespace MonoGameLibrary.Graphics;

/// <summary>Represents a sprite that can be animated.</summary>
public sealed class AnimatedSprite : Sprite
{
    private int _currentFrame;
    private TimeSpan _elapsed;
    private Animation _animation;

    /// <summary>Gets or Sets the animation to use for this animated sprite.</summary>
    public Animation Animation
    {
        get => _animation;
        set
        {
            _animation = value;
            // Starting with the first frame when setting a new animation
            // ensures consistent behavior when switching between different animations.
            Region = _animation?.Frames.Count > 0 ? _animation.Frames[0] : null;
        }
    }

    /// <summary>Creates a new animated sprite.</summary>
    public AnimatedSprite() { }

    /// <summary>Creates a new animated sprite with the specified frames and delay.</summary>
    /// <param name="animation">The animation for this animated sprite.</param>
    public AnimatedSprite(Animation animation)
    {
        Animation = animation;
    }

    /// <summary>Updates this animated sprite.</summary>
    /// <param name="gameTime">A snapshot of the game timing values provided by the framework.</param>
    public void Update(GameTime gameTime)
    {
        _elapsed += gameTime.ElapsedGameTime;

        if (_elapsed >= _animation.Delay)
        {
            _elapsed -= _animation.Delay;
            _currentFrame++;

            if (_currentFrame >= _animation.Frames.Count)
            {
                _currentFrame = 0;
            }

            Region = _animation.Frames[_currentFrame];
        }
    }
}
