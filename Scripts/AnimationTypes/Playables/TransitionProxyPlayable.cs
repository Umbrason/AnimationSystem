using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class TransitionProxyPlayable : PlayableBehaviour
{
    public float transitionDuration = .3f;
    private AnimationMixerPlayable transitionMixer;
    private readonly List<TransitionInfo> TransitionStack = new();
    readonly struct TransitionInfo
    {
        /// <summary>
        /// Always refers to the animation at the index of this in the TransitionStack and the animation  at that index + 1
        /// </summary>
        public readonly float start;
        public readonly float duration;
        public TransitionInfo(float start, float duration)
        {
            this.start = start;
            this.duration = duration;
        }
        public readonly float T => Mathf.Clamp01((Time.time - start) / duration);
    }
    public TransitionProxyPlayable() { }

    public Playable CurrentPlayable
    {
        get
        {
            var playableCount = transitionMixer.GetInputCount();
            return playableCount > 0 ? transitionMixer.GetInput(playableCount - 1) : Playable.Null;
        }
        set
        {
            if (CurrentPlayable.Equals(value)) return;
            transitionMixer.AddInput(value, 0, 0);
            if (transitionDuration > 0) TransitionStack.Add(new(Time.time, transitionDuration));
        }
    }

    public override void OnPlayableCreate(Playable playable)
    {
        base.OnPlayableCreate(playable);
        var graph = playable.GetGraph();
        transitionMixer = AnimationMixerPlayable.Create(graph, 0);
        playable.SetPropagateSetTime(true);
        playable.SetInputCount(1);
        playable.SetOutputCount(1);
        playable.ConnectInput(0, transitionMixer, 0);
        transitionMixer.SetPropagateSetTime(true);
        transitionMixer.SetInputCount(1); //initialize with an empty base layer so the first animation is transitioned in properly
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        base.OnPlayableDestroy(playable);
        transitionMixer.Destroy();
    }

    //
    // 2:                   WalkAnim  | 100%
    // 1:  Transition A     IdleAnim  |   0%
    // 0:  Transition B     MeleeAnim |   0%
    //
    public override void PrepareFrame(Playable playable, FrameData info)
    {
        var latestExpiredTransition = -1; //can remove all previous animation connections because the newest transition has completed thus fully hiding all earlier transitions, even if they are still in progress
        var remainingWeight = 1f;
        for (int i = TransitionStack.Count - 1; i >= 0; i--)
        {
            var transition = TransitionStack[i];
            transitionMixer.SetInputWeight(i + 1, remainingWeight * transition.T);
            remainingWeight *= 1 - transition.T;
            if (transition.T == 1f)
            {
                latestExpiredTransition = i;
                break;
            }
        }
        transitionMixer.SetInputWeight(0, remainingWeight);

        if (latestExpiredTransition < 0) return;
        var remainingPlayablesCount = transitionMixer.GetInputCount() - (latestExpiredTransition + 1);
        for (int i = 0; i < remainingPlayablesCount; i++)
        {
            var oldIdx = i + latestExpiredTransition + 1;
            var oldPlayable = transitionMixer.GetInput(oldIdx);
            var oldWeight = transitionMixer.GetInputWeight(oldIdx);
            transitionMixer.DisconnectInput(oldIdx); //this will result in getting null on later iterations??
            transitionMixer.DisconnectInput(i);
            if (!oldPlayable.IsNull())
                transitionMixer.ConnectInput(i, oldPlayable, 0, oldWeight);
        }
        transitionMixer.SetInputCount(remainingPlayablesCount);
        TransitionStack.RemoveRange(0, latestExpiredTransition + 1);
    }
}
