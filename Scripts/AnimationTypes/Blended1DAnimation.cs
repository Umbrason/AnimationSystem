using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Blended1DAnimation : IAnimation
{
    private readonly Func<float> animationKeyGetter;
    private readonly List<AnimationBlendPart> m_clips = new();

    public float? ExitTransitionDuration { get; set; }
    float? IAnimation.ExitTransitionDuration { get => ExitTransitionDuration; }

    public Blended1DAnimation(Func<float> AnimationKeyGetter, AnimationClip negative, AnimationClip neutral, AnimationClip positive)
        : this(AnimationKeyGetter,
               (-1f, negative),
               (0f, neutral),
               (1f, positive)
              )
    { }

    public Blended1DAnimation(Func<float> AnimationKeyGetter, params (float key, AnimationClip clip)[] Animations) : this(AnimationKeyGetter)
    {
        foreach (var (key, clip) in Animations)
            Add(key, clip);
    }

    public Blended1DAnimation(Func<float> animationKeyGetter)
    {
        this.animationKeyGetter = animationKeyGetter;
    }
    private AnimationBlendPart Add(float key, AnimationClip clip)
    {
        var part = new AnimationBlendPart(key, clip);
        m_clips.Add(part);
        return part;
    }

    public Playable CreatePlayable(PlayableGraph graph)
    {
        var animationMixerDriverPlayable = ScriptPlayable<Animation1DMixerDriverPlayable>.Create(graph, 1);
        var animationMixerDriver = animationMixerDriverPlayable.GetBehaviour();
        animationMixerDriver.Clips = m_clips;
        animationMixerDriver.KeyGetter = () => this.animationKeyGetter?.Invoke() ?? 0f;
        return animationMixerDriverPlayable;
    }

    public class AnimationBlendPart
    {
        public float key;
        public AnimationClip clip;

        public AnimationBlendPart(float key, AnimationClip clip)
        {
            this.key = key;
            this.clip = clip;
        }
    }

}