from mlagents_envs.logging_util import *
from time import sleep
import numpy as np
import os
import time

_log_level = INFO
set_log_level(_log_level)
# How do we enable log messages in the ml-agents package??!!!

import u3_env

# Note that XLand has 20 frames a second
#env = u3_env.create_environment_by_name(file_name=f"{os.path.dirname(os.path.abspath(__file__))}/../unity/Builds/LinuxTraining/XLand", worker_id=0, 
#                                        parameters={"camera_width": 256, "camera_height": 256, "trial_count" : 2, "trial_seconds" : 10.0, "frames_per_second" : 50})
env = u3_env.create_environment_by_name(file_name=f"{os.path.dirname(os.path.abspath(__file__))}/../unity/Builds/WindowsTraining/unitylearning2", worker_id=0, 
                                        parameters={"camera_width": 64, "camera_height": 64, "trial_count" : 2, "trial_seconds" : 10.0, "frames_per_second" : 12})
#env = u3_env.create_environment(worker_id=0, parameters={"camera_width": 256, "camera_height": 256, "trial_count" : 2, "trial_seconds" : 10.0, "frames_per_second" : 50})

n_steps = 1000

start_time = time.time()
for t in range(n_steps):
    # Render the environment (optional, for visualization)
    #env.render()

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
    print(f"Step {t + 1}")
    observation, reward, done, truncation, info = env.step(actions)

    # Check if the episode is done (the pole has fallen)
    if len(agents) > 0 and done[agents[0]]:
        print("Episode finished after {} timesteps".format(t + 1))
        env.reset()
    
    '''for agent in agents:
        if not observation[agent] is None:
            image = cv2.cvtColor(np.swapaxes(observation[agent], 0, -1), cv2.COLOR_RGB2BGR)
            frame_name = agent.replace("?", "_")
            frame_name = frame_name.replace(" ", "_")
            frame_name = frame_name.replace("=", "_")
            cv2.imwrite(f"python/Images/{frame_name}-frame{t}.png", image)
    print(observation[agent].shape)'''

total_time = time.time() - start_time
print(f"average step per second: {n_steps/total_time}")

env.close()