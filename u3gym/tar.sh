#!/bin/bash

# Directory containing folders to be tarred
DIRECTORY="/home/mila/c/connor.brennan/scratch/u3_dataset_data/"

# Navigate to the specified directory
cd "$DIRECTORY" || exit

# Loop through each folder in the directory
for folder in */; do
  # Remove the trailing slash from the folder name
  folder_name="${folder%/}"

  # Create a tar file for the folder
  tar -czf "${folder_name}.tar.gz" "$folder_name"
done

echo "All folders have been tarred into distinct tar files."