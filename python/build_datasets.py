from mlagents_envs.logging_util import *
from time import sleep
import numpy as np
import os
from tqdm import tqdm
import argparse
import time

_log_level = INFO
set_log_level(_log_level)
# How do we enable log messages in the ml-agents package??!!!

import u3_env

parser = argparse.ArgumentParser(description='Build a dataset of worlds for the XLand env. Specific a dataset to append to with "--dataset <difficulty>_<height>"')
parser.add_argument('--worker_id', type=int, help='Worker identifier. Use this to have multiple runs on the same machine', default='0')
parser.add_argument('--dataset', type=str, help='Name of dataset', default='easy_low')
parser.add_argument('--build_type', type=str, help='Which build are you using? editor, linux, windows', default='linux')
#parser.add_argument('--dataset', type=str, help='Name of dataset', default='long_many')
#parser.add_argument('--build_type', type=str, help='Which build are you using? editor, linux, windows', default='windows')
args = parser.parse_args()

dataset_name = args.dataset
worker_id = args.worker_id
build_type = args.build_type

env_width = 1
env_width_var = 0
env_width_min = 5
env_length = 1
env_length_var = 0
env_length_min = 5
env_height = 1
env_height_var = 0
env_height_min = 1

rule_start_objs = 1
rule_start_objs_var = 0
rule_start_objs_min = 1
rule_chain_length = 1
rule_chain_length_var = 0
rule_chain_length_min = 1
rule_distractor = 0
rule_distractor_var = 0
rule_distractor_min = 0


generation_times = []

parts = dataset_name.split('_')

if parts[0] == "easy":
    dataset_type = "world"
    env_width = 5
    env_width_var = 2
    env_length = 5
    env_length_var = 2
elif parts[0] == "medium":
    dataset_type = "world"
    env_width = 10
    env_width_var = 3
    env_length = 10
    env_length_var = 3
elif parts[0] == "hard":
    dataset_type = "world"
    env_width = 15
    env_width_var = 5
    env_length = 15
    env_length_var = 5
elif parts[0] == "short":
    dataset_type = "rule"
    rule_start_objs = 1
    rule_start_objs_var = 1
    rule_chain_length = 1
    rule_chain_length_var = 1
elif parts[0] == "middle":
    dataset_type = "rule"
    rule_start_objs = 3
    rule_start_objs_var = 1.5
    rule_chain_length = 3
    rule_chain_length_var = 1.5
elif parts[0] == "long":
    dataset_type = "rule"
    rule_start_objs = 5
    rule_start_objs_var = 3
    rule_chain_length = 5
    rule_chain_length_var = 3
else:
    print("Dataset name must start with 'easy', 'medium' or 'hard' for worlds, 'short', 'middle' or 'long' for rules")
    sys.exit()


if parts[1] == "low":
    if dataset_type != "world":
        print("Dataset type mismatch. 'easy', 'medium' and 'hard' go with 'low' or 'high'. 'short', 'middle' and 'long' go with 'few' or 'many'")
        sys.exit()
    env_height = 1
    env_height_var = 1
elif parts[1] == "high":
    if dataset_type != "world":
        print("Dataset type mismatch. 'easy', 'medium' and 'hard' go with 'low' or 'high'. 'short', 'middle' and 'long' go with 'few' or 'many'")
        sys.exit()
    env_height = 3
    env_height_var = 2
elif parts[1] == "few":
    if dataset_type != "rule":
        print("Dataset type mismatch. 'easy', 'medium' and 'hard' go with 'low' or 'high'. 'short', 'middle' and 'long' go with 'few' or 'many'")
        sys.exit()
    rule_distractor = 0
    rule_distractor_var = 1
elif parts[1] == "many":
    if dataset_type != "rule":
        print("Dataset type mismatch. 'easy', 'medium' and 'hard' go with 'low' or 'high'. 'short', 'middle' and 'long' go with 'few' or 'many'")
        sys.exit()
    rule_distractor = round(rule_chain_length/2)
    rule_distractor_var = rule_chain_length/2
else:
    print("Dataset name must end with 'low' or 'high' for worlds, 'few' or 'many' for rules")
    sys.exit()

dataset_folder = f"{dataset_type}s/{dataset_name}/"
min_connectivity = 0.5
total_count = 1000000


# Note that XLand has 12 frames a second
base_parameters = {"env_width": env_width, "env_length": env_length, "env_height": env_height, "min_connectivity": min_connectivity,
                   "min_init_objs": rule_start_objs, "max_init_objs": rule_start_objs, 
                   "min_rules": rule_chain_length, "max_rules": rule_chain_length, 
                   "min_deadends": rule_distractor, "max_deadends": rule_distractor}

if build_type == "editor":
    env = u3_env.create_environment(worker_id, base_parameters)
elif build_type == "linux":
    env = u3_env.create_environment_by_name(file_name=f"{os.path.dirname(os.path.abspath(__file__))}/../unity/Builds/WorldDatasetGenerator/XLand", worker_id=worker_id, parameters=base_parameters)
elif build_type == "windows":
    env = u3_env.create_environment_by_name(file_name=f"{os.path.dirname(os.path.abspath(__file__))}/../unity/Builds/WorldDatasetGeneratorWindows/unitylearning2", worker_id=worker_id, parameters=base_parameters)

root_folder = f"{os.path.dirname(os.path.abspath(__file__))}/../Datasets/"
save_folder = f"{root_folder}{dataset_name}/"

os.makedirs(save_folder, exist_ok=True)

def count_files_in_directory(directory):
    return len([name for name in os.listdir(directory) if os.path.isfile(os.path.join(directory, name))])

pbar = tqdm(total=total_count)

file_index = count_files_in_directory(save_folder)
pbar.n = file_index
pbar.last_print_n = file_index  # This ensures the average iteration speed calculation starts correctly
pbar.refresh()  # Refresh the progress bar to show the initial value
for t in range(total_count * 2):
    if dataset_type == "world":
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

    elif dataset_type == "rule":
        save_paramers = {"rule_save_file" : f"{save_folder}{file_index}.json"}

        if rule_start_objs_var > 0:
            while True:
                next_objs = np.round(np.random.normal(rule_start_objs, rule_start_objs_var, 1))
                if next_objs >= rule_start_objs_min:
                    break
            save_paramers["min_init_objs"] = next_objs.astype(int).item()
            save_paramers["max_init_objs"] = next_objs.astype(int).item()

        if rule_chain_length_var > 0:
            while True:
                next_length = np.round(np.random.normal(rule_chain_length, rule_chain_length_var, 1))
                if next_length >= rule_chain_length_min:
                    break
            save_paramers["min_rules"] = next_length.astype(int).item()
            save_paramers["max_rules"] = next_length.astype(int).item()

        if rule_distractor_var > 0:
            while True:
                next_deadends = np.round(np.random.normal(rule_distractor, rule_distractor_var, 1))
                if next_deadends >= rule_distractor_min:
                    break
            save_paramers["min_deadends"] = next_deadends.astype(int).item()
            save_paramers["max_deadends"] = next_deadends.astype(int).item()
            

    
    start_time = time.time()
    env.reset(save_paramers)
    end_time = time.time()

    if (len(env.last_env_messages) > 0):
        last_message = env.last_env_messages[list(env.last_env_messages.keys())[-1]][-1]

        if (last_message.startswith("save_complete")):
            file_index += 1
            pbar.update(1)
            
    if file_index > total_count:
        break


pbar.close()
env.close()