from mlagents_envs.logging_util import *
import cv2
import numpy as np

_log_level = INFO
set_log_level(_log_level)
# How do we enable log messages in the ml-agents package??!!!

import u3_env

env = u3_env.create_environment(0)

for t in range(100):
    # Render the environment (optional, for visualization)
    env.render()

    agents = env.possible_agents

    # Choose a random action from the action space
    # TODO: every step, passed an array of behaviorspecs and an array of agents
    # TODO: one action space per agent
    # action = env.action_space.sample()
    actions = {
        agent: {
            "discrete": env.action_space.sample()[0].reshape(
                1, env.group_spec.action_spec.discrete_size
            ),
            "continuous": env.action_space.sample()[1].reshape(
                1, env.group_spec.action_spec.continuous_size
            ),
        }
        for agent in agents
    }

    # Perform the chosen action
    observation, reward, done, truncation, info = env.step(actions)

    # Check if the episode is done (the pole has fallen)
    """if len(agents) > 0 and done[agents[0]]:
        print("Episode finished after {} timesteps".format(t + 1))
        break"""

    for agent in agents:
        if not observation[agent] is None:
            image = cv2.cvtColor(observation[agent], cv2.COLOR_RGB2BGR)
            frame_name = agent.replace("?", "_")
            frame_name = frame_name.replace(" ", "_")
            frame_name = frame_name.replace("=", "_")
            cv2.imwrite(f"python/Images/{frame_name}-frame{t}.png", image)
    print(actions)

env.close()
