# AnimationSystem
Code driven animation using Playables API.\\
Animations can be applied in layers and only effect parts of a skeleton using code-driven avatar masks.\\
Does not support transitions from one animation to another on the same layer, however animations can be faded in/out.\\
To emulate a transition, keep the old animation playing one a lower layer and fade into the new animation on a higher layer.\\