using System.Linq.Expressions;
using MonoGame.Extended.Tweening;

namespace Alca.MonoGame.Kernel.Tweening;

/// <summary>Wrapper around MonoGame.Extended Tweener providing lifecycle-integrated tween management.</summary>
public sealed class TweeningManager
{
    private readonly Tweener _tweener = new();

    /// <summary>Creates a tween that animates a float property from its current value to toValue over duration seconds.</summary>
    /// <returns>The created Tween, supporting fluent chaining (OnBegin, OnEnd, Repeat, etc.).</returns>
    public Tween TweenTo<T>(T target, Expression<Func<T, float>> member, float toValue, float duration, Func<float, float> easing)
        where T : class
    {
        return _tweener.TweenTo(target, member, toValue, duration, 0f).Easing(easing);
    }

    /// <summary>Advances all active tweens by the elapsed game time.</summary>
    public void Update(GameTime gameTime)
    {
        _tweener.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    /// <summary>Cancels and removes all active tweens.</summary>
    public void CancelAll()
    {
        _tweener.CancelAll();
    }

    /// <summary>Cancels a specific tween.</summary>
    public static void Cancel(Tween tween)
    {
        tween.Cancel();
    }
}
