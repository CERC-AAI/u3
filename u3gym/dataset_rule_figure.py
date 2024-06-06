import os
import json
from collections import defaultdict
from tqdm import tqdm
import matplotlib.pyplot as plt

# Define the list of directories containing JSON files
LOAD_DIRECTORIES = [
    '~/scratch/u3_dataset_data/short_few',
    '~/scratch/u3_dataset_data/short_many',
    '~/scratch/u3_dataset_data/middle_few',
    '~/scratch/u3_dataset_data/middle_many',
    '~/scratch/u3_dataset_data/long_few',
    '~/scratch/u3_dataset_data/long_many',
    # Add more directories as needed
]

# Define the maximum number of files to load from each directory
MAX_FILES = 100000

# Define the output directory for the plots
OUTPUT_DIRECTORY = '~/scratch/u3/figures'

def extract_lengths(file_path):
    with open(file_path, 'r') as file:
        data = json.load(file)
        initial_objects_count = len(data.get("i", []))
        
        rules = data.get("r", [])
        reward_index = next((i for i, rule in enumerate(rules) if rule.get("a0", {}).get("a") == "REWARD"), len(rules))
        rules_after_reward_count = len(rules) - reward_index - 1
        
        return initial_objects_count, rules_after_reward_count

def build_distribution(load_directories, max_files):
    initial_objects_distribution = defaultdict(list)
    rules_distribution = defaultdict(list)

    for load_directory in load_directories:
        # Expand the home directory in the path
        load_directory = os.path.expanduser(load_directory)
        dataset_name = os.path.basename(load_directory)
        filenames = [f for f in os.listdir(load_directory) if f.endswith('.json')][:max_files]
        
        for filename in tqdm(filenames, desc=f"Processing files in {load_directory}"):
            file_path = os.path.join(load_directory, filename)
            try:
                initial_objects_count, rules_count = extract_lengths(file_path)
                initial_objects_distribution[dataset_name].append(initial_objects_count)
                rules_distribution[dataset_name].append(rules_count)
            except (ValueError, KeyError, json.JSONDecodeError) as e:
                print(f"Skipping {file_path}: {e}")
    
    return initial_objects_distribution, rules_distribution

def plot_histograms(initial_objects_dist, rules_dist, output_directory):
    # Find global min and max for initial objects and rules
    all_initial_objects = [value for values in initial_objects_dist.values() for value in values]
    all_rules = [value for values in rules_dist.values() for value in values]

    min_val_io, max_val_io = min(all_initial_objects), max(all_initial_objects)
    min_val_r, max_val_r = min(all_rules), max(all_rules)

    # Set bin edges
    bins_io = range(min_val_io, max_val_io + 1)
    bins_r = range(min_val_r, max_val_r + 1)

    # Plot initial objects distribution
    plt.figure(figsize=(12, 8))

    for dataset, data in initial_objects_dist.items():
        plt.hist(data, bins=bins_io, alpha=0.35, label=f'{dataset}', edgecolor='black')

    plt.xlabel('Initial Objects Count')
    plt.ylabel('Frequency')
    plt.title('Distribution of Initial Objects Count')
    plt.legend(loc='upper right')
    plt.grid(True)
    
    # Ensure the output directory exists
    os.makedirs(output_directory, exist_ok=True)

    # Save the figure as PDF and PNG
    pdf_path = os.path.join(output_directory, 'initial_objects_distribution_histograms.pdf')
    png_path = os.path.join(output_directory, 'initial_objects_distribution_histograms.png')
    plt.savefig(pdf_path)
    plt.savefig(png_path)
    plt.close()

    # Plot rules distribution separately
    plt.figure(figsize=(12, 8))

    for dataset, data in rules_dist.items():
        plt.hist(data, bins=bins_r, alpha=0.35, label=f'{dataset}', edgecolor='black')

    plt.xlabel('Rules Count')
    plt.ylabel('Frequency')
    plt.title('Distribution of Distractor Rules')
    plt.legend(loc='upper right')
    plt.grid(True)

    # Save the figure as PDF and PNG
    pdf_path = os.path.join(output_directory, 'rules_distribution_histograms.pdf')
    png_path = os.path.join(output_directory, 'rules_distribution_histograms.png')
    plt.savefig(pdf_path)
    plt.savefig(png_path)
    plt.close()

def main():
    initial_objects_dist, rules_dist = build_distribution(LOAD_DIRECTORIES, MAX_FILES)
    output_directory = os.path.expanduser(OUTPUT_DIRECTORY)
    plot_histograms(initial_objects_dist, rules_dist, output_directory)

if __name__ == "__main__":
    main()
