using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class SingleClipAnimation : IAnimation
{
    public SingleClipAnimation(AnimationClip clip) => this.Clip = clip;
    public AnimationClip Clip { get; private set; }
    public Playable CreatePlayable(PlayableGraph graph) => AnimationClipPlayable.Create(graph, Clip);
}