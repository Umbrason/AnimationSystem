using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class RandomAnimationClipPlayable : PlayableBehaviour
{
    private IReadOnlyCollection<AnimationClip> m_clips;
    public IReadOnlyCollection<AnimationClip> Clips
    {
        get => m_clips;
        set
        {
            if (m_clips == value) return;
            m_ClipQueue.Clear();
            m_clips = value;
        }
    }
    public RandomAnimationClipPlayable() { }

    private readonly Queue<AnimationClip> m_ClipQueue = new();
    AnimationClip NextClip
    {
        get
        {
            if (Clips.Count == 0) return null;
            if (m_ClipQueue.Count > 1)
                return m_ClipQueue.Dequeue();
            var lastClipInQueue = m_ClipQueue.Count > 0 ? m_ClipQueue.Peek() : null;
            foreach (var clip in Clips.Where(clip => clip != lastClipInQueue).OrderBy(_ => UnityEngine.Random.value))
                m_ClipQueue.Enqueue(clip);
            m_ClipQueue.Enqueue(lastClipInQueue);
            return m_ClipQueue.Dequeue();
        }
    }


    public override void OnPlayableCreate(Playable playable)
    {
        base.OnPlayableCreate(playable);
        var graph = playable.GetGraph();
        playable.SetPropagateSetTime(true);
        playable.SetOutputCount(1);
        playable.SetInputCount(1);
        playable.SetInputWeight(0, 1f);
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        base.OnPlayableDestroy(playable);
    }

    AnimationClipPlayable? animClipPlayable;
    public override void PrepareFrame(Playable playable, FrameData info)
    {
        if (!(animClipPlayable?.IsDone() ?? true)) return; //animClipPlayable exists and is not done yet
        if (animClipPlayable != null)
        {
            playable.DisconnectInput(0);
            animClipPlayable?.Destroy();
        }
        animClipPlayable = AnimationClipPlayable.Create(playable.GetGraph(), NextClip);
        playable.ConnectInput(0, animClipPlayable.Value, 0);
    }
}