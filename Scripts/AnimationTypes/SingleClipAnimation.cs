using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class SingleClipAnimation : IAnimation
{
    public float? ExitTransitionDuration { get; set; }
    float? IAnimation.ExitTransitionDuration { get => ExitTransitionDuration; }

    public SingleClipAnimation(AnimationClip clip) => this.Clip = clip;
    private AnimationClip m_clip;
    public AnimationClip Clip
    {
        get => m_clip;
        private set
        {
            m_clip = value;
            if (m_clip.wrapMode == WrapMode.Clamp)
                m_clip.wrapMode = WrapMode.ClampForever; //for transitions to properly fade out
        }
    }
    public Playable CreatePlayable(PlayableGraph graph) => AnimationClipPlayable.Create(graph, Clip);
}