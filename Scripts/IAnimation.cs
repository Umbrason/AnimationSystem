using UnityEngine.Playables;

public interface IAnimation
{
    Playable CreatePlayable(PlayableGraph graph);
}