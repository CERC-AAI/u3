#!/bin/bash

# Initialize variables with default values
DATASET=""
WORKER_ID=""
START_INDEX=""
END_INDEX=""
BUILD_TYPE=""

# Parse command line arguments
for i in "$@"
do
case $i in
    --dataset=*)
    DATASET="${i#*=}"
    shift # past argument=value
    ;;
    --worker_id=*)
    WORKER_ID="${i#*=}"
    shift # past argument=value
    ;;
    --start_index=*)
    START_INDEX="${i#*=}"
    shift # past argument=value
    ;;
    --end_index=*)
    END_INDEX="${i#*=}"
    shift # past argument=value
    ;;
    --build_type=*)
    BUILD_TYPE="${i#*=}"
    shift # past argument=value
    ;;
    *)
    # unknown option
    ;;
esac
done

# Check if required arguments are provided
if [ -z "$DATASET" ] || [ -z "$WORKER_ID" ] || [ -z "$START_INDEX" ] || [ -z "$END_INDEX" ]; then
    echo "Usage: $0 --dataset=DATASET --worker_id=WORKER_ID --start_index=START_INDEX --end_index=END_INDEX [--build_type=BUILD_TYPE]"
    exit 1
fi

# Load anaconda module
module load anaconda

# Activate conda environment
conda activate u3

cd u3gym

# Run the python script with the provided arguments
python build_datasets.py --dataset "$DATASET" --worker_id "$WORKER_ID" --start_index "$START_INDEX" --end_index "$END_INDEX" --build_type "$BUILD_TYPE"
