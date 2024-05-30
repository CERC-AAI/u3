#!/bin/bash
#SBATCH --cpus-per-task=1
#SBATCH --ntasks=1
#SBATCH --mem=80G
#SBATCH --time=4:00:00

echo CPUS: $SLURM_CPUS_ON_NODE
srun xvfb-run -a python main.py