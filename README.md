# u3
Unified Unity Universe
A farmework for rapid development of RL environments in Unity.

We also include the Open-XLand implementation of: https://arxiv.org/abs/2107.12808

The project is split into Unity and Python sides. Unity deals with the code to set up RL environments within the Unity game engine itself. Python deals with wrappers for interfacing with the environment during training. This entails a PettingZoo wrapper and extra API calls that enable U3 specific functionality (such as probing environment complexity).
