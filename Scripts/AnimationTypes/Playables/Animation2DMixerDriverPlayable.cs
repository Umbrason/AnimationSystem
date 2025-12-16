using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class Animation2DMixerDriverPlayable : PlayableBehaviour
{
    private List<BlendedMovementAnimation.AnimationBlendPart> m_clips;
    public List<BlendedMovementAnimation.AnimationBlendPart> Clips
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
    public Func<Vector2> KeyGetter { get; set; }
    public Animation2DMixerDriverPlayable() { }

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
        var velocity = KeyGetter();
        var keys = new Vector2[m_clips.Count];
        var influences = new float[m_clips.Count];
        var influenceSum = 0f;
        for (int i = 0; i < m_clips.Count; i++)
            keys[i] = m_clips[i].key;
        for (int i = 0; i < m_clips.Count; i++)
        {
            influences[i] = GradientBandInterpolation(velocity, i, keys);
            influenceSum += influences[i];
        }
        for (int i = 0; i < m_clips.Count; i++)
            mixer.SetInputWeight(i, influences[i] / influenceSum);
        for (int i = 0; i < m_clips.Count; i++)
            mixer.GetInput(i).SetSpeed(velocity.magnitude);
    }

    private float GradientBandInterpolation(Vector2 velocity, int index, Vector2[] keys)
    {
        var p = velocity;
        var pi = keys[index];
        var pimag = pi.magnitude;
        var pmag = p.magnitude;
        var min = float.PositiveInfinity;
        for (int j = 0; j < keys.Length; j++)
        {
            if (index == j) continue;
            var pj = keys[j];
            var pjmag = pj.magnitude;
            var pipj = new Vector2((pjmag - pimag) / (pjmag + pimag) * 2f, 2f * Vector2.SignedAngle(pj, pi) / 180f * Mathf.PI);
            var pip = new Vector2((pmag - pimag) / (pjmag + pimag) * 2f, 2f * Vector2.SignedAngle(p, pi) / 180f * Mathf.PI);
            var weight = 1 - Vector2.Dot(pip, pipj) / pipj.sqrMagnitude;
            min = Mathf.Min(weight, min);
            if (min <= 0) break;
        }
        return Mathf.Max(0, min);
    }
}