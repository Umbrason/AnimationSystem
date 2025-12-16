using UnityEngine.Playables;

public interface IAnimation
{
    Playable CreatePlayable(PlayableGraph graph);
    virtual float? ExitTransitionDuration => null;
}