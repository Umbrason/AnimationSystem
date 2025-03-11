using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class BlendedMovementAnimation : IAnimation
{
    private Func<Vector2> animationKeyGetter;
    private readonly List<AnimationBlendPart> m_clips = new();

    public BlendedMovementAnimation(Func<Vector2> AnimationKeyGetter, AnimationClip forward, AnimationClip back, AnimationClip left, AnimationClip right)
    : this(AnimationKeyGetter,
           (Vector2.up, forward),
           (Vector2.down, back),
           (Vector2.left, left),
           (Vector2.right, right)
          )
    { }

    public BlendedMovementAnimation(Func<Vector2> AnimationKeyGetter, params (Vector2 key, AnimationClip clip)[] Animations) : this(AnimationKeyGetter)
    {
        foreach (var (key, clip) in Animations)
            Add(key, clip);
    }

    public BlendedMovementAnimation(Func<Vector2> animationKeyGetter)
    {
        this.animationKeyGetter = animationKeyGetter;
    }
    private AnimationBlendPart Add(Vector2 key, AnimationClip clip)
    {
        var part = new AnimationBlendPart(key, clip);
        m_clips.Add(part);
        return part;
    }

    public Playable CreatePlayable(PlayableGraph graph)
    {
        var animationMixerDriverPlayable = ScriptPlayable<AnimationMixerDriverPlayable>.Create(graph, 1);
        var animationMixerDriver = animationMixerDriverPlayable.GetBehaviour();
        animationMixerDriver.Clips = m_clips;
        animationMixerDriver.KeyGetter = () => this.animationKeyGetter?.Invoke() ?? Vector2.up;
        return animationMixerDriverPlayable;
    }

    public class AnimationBlendPart
    {
        public Vector2 key;
        public AnimationClip clip;

        public AnimationBlendPart(Vector2 key, AnimationClip clip)
        {
            this.key = key;
            this.clip = clip;
        }
    }
}