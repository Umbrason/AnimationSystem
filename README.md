# AnimationSystem
Code driven animation using the Playables API.  
Animations can be applied in layers and only effect parts of a skeleton using code-driven avatar masks.  
~~Does not support transitions from one animation to another on the same animation layer at this point, however animations can be faded in/out.  
To emulate a transition, keep the old animation playing one a lower layer and fade into the new animation on a higher layer.~~  
Does support transitions on the same layer since the last commit. Transition duration can be configured on a per-layer basis using ``AnimationLayer.TransitionDuration``.
