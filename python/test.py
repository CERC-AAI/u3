

from mlagents_envs.logging_util import *

_log_level = INFO
logger = get_logger(__name__)
logger.setLevel(INFO)

#How do we enable log messages in the ml-agents package??!!!

import u3_env

env = u3_env.create_environment(0)

for t in range(1000):
    # Render the environment (optional, for visualization)
    env.render()
    
    # Choose a random action from the action space
    action = env.action_space.sample()
    
    # Perform the chosen action
    observation, reward, done, info = env.step(action)
    
    # Check if the episode is done (the pole has fallen)
    if done:
        print("Episode finished after {} timesteps".format(t+1))
        break

env.close()