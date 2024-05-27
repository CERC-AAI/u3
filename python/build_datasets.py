from mlagents_envs.logging_util import *
import cv2
from time import sleep
import numpy as np
import os
from tqdm import tqdm

_log_level = INFO
set_log_level(_log_level)
# How do we enable log messages in the ml-agents package??!!!

import u3_env

dataset_type = "world"
dataset_name = "easy_low"
dataset_folder = f"{dataset_type}s/{dataset_name}/"
min_connectivity = 0.5
total_count = 1000

env_width = 5
env_width_var = 2
env_width_min = 5
env_length = 5
env_length_var = 2
env_length_min = 5
env_height = 1
env_height_var = 1
env_height_min = 1

# Note that XLand has 12 frames a second
base_parameters = {"env_width": env_width, "env_length": env_length, "env_height": env_height, "min_connectivity": min_connectivity}
#env = u3_env.create_environment(0, base_parameters)
env = u3_env.create_environment_by_name(f"{os.path.dirname(os.path.abspath(__file__))}/../unity/Builds/WorldDatasetGenerator/XLand", 0, base_parameters)

root_folder = f"{os.path.dirname(os.path.abspath(__file__))}/../Datasets/"
save_folder = f"{root_folder}{dataset_name}"

os.makedirs(save_folder, exist_ok=True)

def count_files_in_directory(directory):
    return len([name for name in os.listdir(directory) if os.path.isfile(os.path.join(directory, name))])

pbar = tqdm(total=total_count)

file_index = count_files_in_directory(save_folder)
pbar.n = file_index
pbar.last_print_n = file_index  # This ensures the average iteration speed calculation starts correctly
pbar.refresh()  # Refresh the progress bar to show the initial value
for t in range(total_count * 2):
    save_paramers = {"world_save_file" : f"{save_folder}{file_index}.json"}

    if env_width_var > 0:
        while True:
            next_width = np.round(np.random.normal(env_width, env_width_var, 1))
            if next_width >= env_width_min:
                break
        save_paramers["env_width"] = next_width.astype(int).item()

    if env_length_var > 0:
        while True:
            next_length = np.round(np.random.normal(env_length, env_length_var, 1))
            if next_length >= env_length_min:
                break
        save_paramers["env_length"] = next_length.astype(int).item()

    if env_height_var > 0:
        while True:
            next_height = np.round(np.random.normal(env_height, env_height_var, 1))
            if next_height >= env_height_min:
                break
        save_paramers["env_height"] = next_height.astype(int).item()

    env.reset(save_paramers)

    if (len(env.last_env_messages) > 0):
        last_message = env.last_env_messages[list(env.last_env_messages.keys())[-1]][-1]

        if (last_message == "save_complete"):
            file_index += 1
            pbar.update(1)
            
    if file_index > 100000:
        break


pbar.close()
env.close()