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
parser.add_argument('--dataset', type=str, help='Name of dataset', default='easy_high')
parser.add_argument('--build_type', type=str, help='Which build are you using? editor, linux, windows', default='windows')
parser.add_argument('--worker_id', type=int, help='Worker identifier. Use this to have multiple runs on the same machine', default='0')
args = parser.parse_args()

dataset_type = "world"
dataset_name = args.dataset
worker_id = args.worker_id
build_type = args.build_type
dataset_folder = f"{dataset_type}s/{dataset_name}/"
min_connectivity = 0.5
total_count = 1000000

env_width = 5
env_width_var = 2
env_width_min = 5
env_length = 5
env_length_var = 2
env_length_min = 5
env_height = 1
env_height_var = 1
env_height_min = 1

generation_times = []

parts = dataset_name.split('_')

if parts[0] == "easy":
    env_width = 5
    env_width_var = 2
    env_length = 5
    env_length_var = 2
elif parts[0] == "medium":
    env_width = 10
    env_width_var = 3
    env_length = 10
    env_length_var = 3
elif parts[0] == "hard":
    env_width = 15
    env_width_var = 5
    env_length = 15
    env_length_var = 5
else:
    print("Dataset name must start with 'easy', 'medium' or 'hard'")
    sys.exit()


if parts[1] == "low":
    env_height = 1
    env_height_var = 1
elif parts[1] == "high":
    env_height = 3
    env_height_var = 2
else:
    print("Dataset name must end with 'low' or 'high'")
    sys.exit()


# Note that XLand has 12 frames a second
base_parameters = {"env_width": env_width, "env_length": env_length, "env_height": env_height, "min_connectivity": min_connectivity}

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

def get_missing_files(directory):
    # Get a list of all files in the directory
    files = os.listdir(directory)
    
    # Filter the list to include only .json files
    json_files = [f for f in files if f.endswith('.json')]
    
    # Extract the numeric part from the filenames
    file_numbers = [int(f.split('.')[0]) for f in json_files]
    
    # Determine the maximum file number
    if not file_numbers:
        print("No JSON files found in the directory.")
        return []
    
    max_file_number = max(file_numbers)
    
    # Create a set of all expected file numbers
    expected_files = set(range(max_file_number + 1))
    
    # Create a set of actual file numbers
    actual_files = set(file_numbers)
    
    # Find the missing files by subtracting the sets
    missing_files = expected_files - actual_files
    
    # Return the sorted list of missing file names
    missing_file_names = sorted([num for num in missing_files])
    
    return missing_file_names

missing_files = get_missing_files(save_folder)


pbar = tqdm(total=total_count)

file_index = count_files_in_directory(save_folder) - len(missing_files)
pbar.n = file_index
pbar.last_print_n = file_index  # This ensures the average iteration speed calculation starts correctly
pbar.refresh()  # Refresh the progress bar to show the initial value
for t in range(total_count * 2):
    use_file_index = file_index
    if len(missing_files) > 0:
        use_file_index = missing_files[0]
        del missing_files[0]

    save_paramers = {"world_save_file" : f"{save_folder}{use_file_index}.json"}

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