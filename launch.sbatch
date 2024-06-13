#!/bin/bash
#SBATCH --cpus-per-task=32
#SBATCH --ntasks=1
#SBATCH --gpus=1
#SBATCH --mem=180G
#SBATCH --time=24:00:00

module load cudatoolkit/12.2.2 
echo "CPUS: " $SLURM_CPUS_ON_NODE
srun xvfb-run -a python train.py