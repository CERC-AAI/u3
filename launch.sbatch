#!/bin/bash
#SBATCH --cpus-per-task=1
#SBATCH --ntasks=1
#SBATCH --mem=80G
#SBATCH --time=4:00:00

echo CPUS: $SLURM_CPUS_ON_NODE
srun xvfb-run "-screen 0 1280x1024x24" python main.py