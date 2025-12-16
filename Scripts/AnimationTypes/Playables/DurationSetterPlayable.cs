using UnityEngine.Playables;

public class DurationSetterPlayable : PlayableBehaviour
{
    public DurationSetterPlayable() { }

    private float m_Duration = 1f;
    public float Duration
    {
        get => m_Duration;
        set
        {
            if(value == 0) throw new System.InvalidOperationException("Duration cannot be set to 0");
            m_Duration = value;
        }
    }

    public Playable m_TargetPlayable;
    public Playable TargetPlayable
    {
        get => m_TargetPlayable; set
        {
            m_TargetPlayable = value;
            ownPlayable.ConnectInput(0, value, 0);
        }
    }
    private Playable ownPlayable;

    public override void OnPlayableCreate(Playable playable)
    {
        ownPlayable = playable;
        playable.SetInputCount(1);
        playable.SetOutputCount(1);
        playable.SetInputWeight(0, 1f);
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        if (TargetPlayable.IsNull()) return;
        var durationOfTarget = TargetPlayable.GetDuration();
        var speed = durationOfTarget / m_Duration;
        ownPlayable.SetSpeed(speed);
    }
}