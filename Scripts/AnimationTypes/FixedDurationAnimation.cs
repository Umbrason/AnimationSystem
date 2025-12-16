using UnityEngine.Playables;

public class FixedDurationAnimation : IAnimation
{
    public float? ExitTransitionDuration { get; set; }
    float? IAnimation.ExitTransitionDuration { get => ExitTransitionDuration; }

    public FixedDurationAnimation(IAnimation animation, float duration)
    {
        this.Animation = animation;
        this.Duration = duration;
    }
    public IAnimation Animation { get; private set; }
    public float Duration { get; private set; }
    public Playable CreatePlayable(PlayableGraph graph)
    {
        var durationSetterPlayable = ScriptPlayable<DurationSetterPlayable>.Create(graph);
        var durationSetter = durationSetterPlayable.GetBehaviour();
        durationSetter.Duration = Duration;
        durationSetter.TargetPlayable = Animation.CreatePlayable(graph);
        return durationSetterPlayable;
    }
}