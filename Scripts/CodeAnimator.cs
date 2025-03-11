using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[DefaultExecutionOrder(-100)]
public class CodeAnimator : MonoBehaviour
{
    private readonly List<AnimationLayer> m_Layers = new();
    public IReadOnlyList<AnimationLayer> Layers => m_Layers;
    private readonly Dictionary<IAnimation, Playable> cached_playables = new();
    public class AnimationLayer
    {
        public string Name { get; private set; }
        public int Index { get; private set; }
        public bool IsPlaying
        {
            get
            {
                if (animator == null) return false;
                if (animator.layerPlayable.IsNull()) return false;
                if (animator.layerPlayable.GetInputCount() < Index) return false;
                if (animator.layerPlayable.GetInput(Index).IsNull()) return false;
                return !animator.layerPlayable.GetInput(Index).IsDone();
            }
        }
        private readonly CodeAnimator animator;
        private float m_weight = 1f;
        public float Weight
        {
            get => m_weight;
            set
            {
                m_weight = value;
                animator.layerPlayable.SetInputWeight(Index, m_animation == null ? 0 : m_weight);
            }
        }
        private AvatarMask layerMask;
        public IReadOnlyCollection<Transform> m_animatedTransforms;
        public IReadOnlyCollection<Transform> AnimatedTransforms
        {
            get => m_animatedTransforms; set
            {
                if (m_animation == value) return;
                layerMask ??= new();
                layerMask.transformCount = 0;
                foreach (var transform in value ?? new Transform[0]) layerMask.AddTransformPath(transform, true);
                var animatorTransformName = animator.TargetAnimator.transform.name;
                for (int i = 0; i < layerMask.transformCount; i++)
                {
                    var relativePath = layerMask.GetTransformPath(i);
                    relativePath = relativePath[(animatorTransformName.Length + 1)..];
                    layerMask.SetTransformPath(i, relativePath);
                    layerMask.SetTransformActive(i, true);
                }
                animator.layerPlayable.SetLayerMaskFromAvatarMask((uint)Index, layerMask);
            }
        }
        private IAnimation m_animation;
        public IAnimation Animation
        {
            get => m_animation;
            set
            {
                if (m_animation == value) return;
                if (m_animation != null)
                {
                    /* animator.layerPlayable.GetInput(Index).Destroy(); */ //TODO: check how not deleting the old animation playables affects performance
                    animator.layerPlayable.DisconnectInput(Index);
                }
                m_animation = value;
                if (m_animation != null)
                {
                    if (!animator.cached_playables.TryGetValue(m_animation, out var playable))
                        playable = m_animation.CreatePlayable(animator.graph);
                    animator.layerPlayable.ConnectInput(Index, playable, 0);
                }
                animator.layerPlayable.SetInputWeight(Index, m_animation == null ? 0 : m_weight);
            }
        }
        public AnimationLayer(CodeAnimator animator, int index, string name, IAnimation animation = null, IReadOnlyCollection<Transform> animatedTransforms = null)
        {
            this.Name = name;
            this.Index = index;
            this.animator = animator;
            this.Animation = animation;
            this.AnimatedTransforms = animatedTransforms;
        }
    }
    private Cached<Animator> cached_TargetAnimator = new(Cached<Animator>.GetOption.Children);
    public Animator TargetAnimator => cached_TargetAnimator[this];
    internal PlayableGraph graph;
    internal AnimationLayerMixerPlayable layerPlayable;
    public void Awake()
    {
        graph = PlayableGraph.Create();
        var animationOutput = AnimationPlayableOutput.Create(graph, "Animation Output", TargetAnimator);
        if (TargetAnimator is null) Debug.LogWarning($"No target animator found on {gameObject} or any of its children!");
        layerPlayable = AnimationLayerMixerPlayable.Create(graph, 0);
        animationOutput.SetSourcePlayable(layerPlayable);
        graph.Play();
    }


    public AnimationLayer AddLayer(string name, IAnimation animation = null, params Transform[] animatedTransform) => AddLayer(name, animation, (IReadOnlyCollection<Transform>)animatedTransform);
    public AnimationLayer AddLayer(string name, IAnimation animation = null, IReadOnlyCollection<Transform> animatedTransforms = null)
    {
        if (GetLayer(name) != null) throw new ArgumentException($"Layer {name} already exists!");
        layerPlayable.SetInputCount(Layers.Count + 1);
        var layerItem = new AnimationLayer(this, Layers.Count, name, animation, animatedTransforms);
        m_Layers.Add(layerItem);
        return layerItem;
    }

    AnimationLayer GetLayer(string name)
    {
        for (int i = 0; i < m_Layers.Count; i++) if (m_Layers[i].Name.Equals(name)) return m_Layers[i];
        return null;
    }

    public void RemoveLayer(string name)
    {
        var layer = GetLayer(name);
        m_Layers.Remove(layer);
        throw new NotImplementedException("Playable Graph needs to be updated after removing layer(s)"); //BUG: Fix this
    }

    public AnimationLayer this[string key] => GetLayer(key);
    public AnimationLayer this[int index] => index > 0 && index < Layers.Count ? Layers[index] : null;
}