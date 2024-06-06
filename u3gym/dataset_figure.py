import os
import re
from collections import defaultdict
from tqdm import tqdm
import matplotlib.pyplot as plt

# Define the list of directories containing JSON files
LOAD_DIRECTORIES = [
    '~/scratch/u3_dataset_data/easy_low',
    '~/scratch/u3_dataset_data/easy_high',
    '~/scratch/u3_dataset_data/medium_low',
    '~/scratch/u3_dataset_data/medium_high',
    '~/scratch/u3_dataset_data/hard_low',
    '~/scratch/u3_dataset_data/hard_high',
    # Add more directories as needed
]

# Define the maximum number of files to load from each directory
MAX_FILES = 100000

# Define the output directory for the plots
OUTPUT_DIRECTORY = '~/scratch/u3/figures'

def extract_dimensions(file_path):
    with open(file_path, 'r') as file:
        # Read the first chunk of the file (assuming the first 200 characters will include w, h, l)
        chunk = file.read(200)
        # Use regular expressions to find the dimensions
        w_match = re.search(r'"w":\s*(\d+)', chunk)
        h_match = re.search(r'"h":\s*(\d+)', chunk)
        l_match = re.search(r'"l":\s*(\d+)', chunk)
        if w_match and h_match and l_match:
            w = int(w_match.group(1))
            h = int(h_match.group(1))
            l = int(l_match.group(1))
            return w, h, l
        else:
            raise ValueError("Dimensions not found in the chunk")

def build_distribution(load_directories, max_files):
    width_length_distribution = defaultdict(list)
    height_distribution = defaultdict(list)

    for load_directory in load_directories:
        # Expand the home directory in the path
        load_directory = os.path.expanduser(load_directory)
        dataset_name = os.path.basename(load_directory)
        filenames = [f for f in os.listdir(load_directory) if f.endswith('.json')][:max_files]
        
        for filename in tqdm(filenames, desc=f"Processing files in {load_directory}"):
            file_path = os.path.join(load_directory, filename)
            try:
                w, h, l = extract_dimensions(file_path)
                width_length_distribution[dataset_name].extend([w, l])
                height_distribution[dataset_name].append(h)
            except (ValueError, KeyError) as e:
                print(f"Skipping {file_path}: {e}")
    
    return width_length_distribution, height_distribution

def plot_histograms(width_length_dist, height_dist, output_directory):
    # Find global min and max for width/length and height
    all_width_length = [value for values in width_length_dist.values() for value in values]
    all_height = [value for values in height_dist.values() for value in values]

    min_val_wl, max_val_wl = min(all_width_length), max(all_width_length)
    min_val_h, max_val_h = min(all_height), max(all_height)

    # Set bin edges
    bins_wl = range(min_val_wl, max_val_wl + 1)
    bins_h = range(min_val_h, max_val_h + 1)

    # Plot width and length combined distribution
    plt.figure(figsize=(12, 8))

    for dataset, data in width_length_dist.items():
        plt.hist(data, bins=bins_wl, alpha=0.35, label=f'{dataset}', edgecolor='black')

    plt.xlabel('Value')
    plt.ylabel('Frequency')
    plt.title('Distribution of Width & Length')
    plt.legend(loc='upper right')
    plt.grid(True)
    
    # Ensure the output directory exists
    os.makedirs(output_directory, exist_ok=True)

    # Save the figure as PDF and PNG
    pdf_path = os.path.join(output_directory, 'width_length_distribution_histograms.pdf')
    png_path = os.path.join(output_directory, 'width_length_distribution_histograms.png')
    plt.savefig(pdf_path)
    plt.savefig(png_path)
    plt.close()

    # Plot height distribution separately
    plt.figure(figsize=(12, 8))

    for dataset, data in height_dist.items():
        plt.hist(data, bins=bins_h, alpha=0.35, label=f'{dataset}', edgecolor='black')

    plt.xlabel('Value')
    plt.ylabel('Frequency')
    plt.title('Distribution of Height')
    plt.legend(loc='upper right')
    plt.grid(True)

    # Save the figure as PDF and PNG
    pdf_path = os.path.join(output_directory, 'height_distribution_histograms.pdf')
    png_path = os.path.join(output_directory, 'height_distribution_histograms.png')
    plt.savefig(pdf_path)
    plt.savefig(png_path)
    plt.close()

def main():
    width_length_dist, height_dist = build_distribution(LOAD_DIRECTORIES, MAX_FILES)
    output_directory = os.path.expanduser(OUTPUT_DIRECTORY)
    plot_histograms(width_length_dist, height_dist, output_directory)

if __name__ == "__main__":
    main()
