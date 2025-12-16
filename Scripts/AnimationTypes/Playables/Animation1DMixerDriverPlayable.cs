using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class Animation1DMixerDriverPlayable : PlayableBehaviour
{
    private List<Blended1DAnimation.AnimationBlendPart> m_clips;
    public List<Blended1DAnimation.AnimationBlendPart> Clips
    {
        get => m_clips;
        set
        {
            if (m_clips == value) return;
            for (int i = 0; i < (m_clips?.Count ?? 0); i++)
            {
                mixer.GetInput(i).Destroy();
                mixer.DisconnectInput(i);
            }
            mixer.SetInputCount(0);
            m_clips = value;
            if (m_clips == null) return;
            mixer.SetInputCount(m_clips.Count);
            for (int i = 0; i < m_clips.Count; i++)
                mixer.ConnectInput(i, AnimationClipPlayable.Create(mixer.GetGraph(), m_clips[i].clip), 0);
        }
    }

    private AnimationMixerPlayable mixer;
    public Func<float> KeyGetter { get; set; }
    public Animation1DMixerDriverPlayable() { }

    public override void OnPlayableCreate(Playable playable)
    {
        base.OnPlayableCreate(playable);
        var graph = playable.GetGraph();
        mixer = AnimationMixerPlayable.Create(graph, 0);
        playable.SetPropagateSetTime(true);
        playable.SetInputCount(1);
        playable.SetOutputCount(1);
        playable.ConnectInput(0, mixer, 0);
        playable.SetInputWeight(0, 1f);
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        base.OnPlayableDestroy(playable);
        mixer.Destroy();
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        var key = KeyGetter();
        var keys = new float[m_clips.Count];
        var influences = new float[m_clips.Count];
        var influenceSum = 0f;
        for (int i = 0; i < m_clips.Count; i++)
            keys[i] = m_clips[i].key;
        for (int i = 0; i < m_clips.Count; i++)
        {
            influences[i] = GetInfluenceAtKey(key, i, keys);
            influenceSum += influences[i];
        }
        for (int i = 0; i < m_clips.Count; i++)
            mixer.SetInputWeight(i, influences[i] / influenceSum);
    }

    private float GetInfluenceAtKey(float key, int index, float[] keys)
    {
        var prev = index > 0 ? keys[index - 1] : float.NegativeInfinity;
        var cur = keys[index];
        var next = keys.Length > index + 1 ? keys[index + 1] : float.PositiveInfinity;

        if (key <= prev) return 0;
        if (key >= next) return 0;
        if (key == cur) return 1;

        var t = 1 - (key > cur ? (key - cur) / (next - cur) : (cur - key) / (cur - prev));
        t = Mathf.SmoothStep(0, 1, t);
        if (t == float.PositiveInfinity || t == float.NegativeInfinity || t == float.NaN) return 1f;
        return t;
    }
}