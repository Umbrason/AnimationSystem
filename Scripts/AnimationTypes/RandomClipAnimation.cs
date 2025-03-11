using UnityEngine;
using UnityEngine.Playables;

public class RandomClipAnimation : IAnimation
{
    public RandomClipAnimation(AnimationClip[] clips)
    {
        Clips = clips;
    }
    public AnimationClip[] Clips { get; private set; }
    public Playable CreatePlayable(PlayableGraph graph)
    {
        var randomAnimationClipPlayable = ScriptPlayable<RandomAnimationClipPlayable>.Create(graph);
        var randomAnimationClip = randomAnimationClipPlayable.GetBehaviour();
        randomAnimationClip.Clips = Clips;
        return randomAnimationClipPlayable;
    }
}