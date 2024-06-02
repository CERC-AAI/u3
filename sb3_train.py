import gymnasium as gym
from stable_baselines3 import SAC
from stable_baselines3.common.vec_env import DummyVecEnv, SubprocVecEnv
from wandb.integration.sb3 import WandbCallback
import wandb

ENV_ID = 'u3gym:U3GymEnv-v0'
NUM_ENVS = 1

run = wandb.init(
    project="sb3",
    sync_tensorboard=True,
    monitor_gym=True,
    save_code=True,
)

def make_env(env_id, rank, seed=0):
    def _init():
        env = gym.make(
            env_id,
            file_name='unity/Builds/LinuxTraining/XLand',
            worker_id=rank,
            seed=seed,
            disable_env_checker=True,
            camera_width=64,
            camera_height=64,
        )
        return env
    return _init


env = DummyVecEnv([make_env(ENV_ID, i) for i in range(NUM_ENVS)])
model = SAC('CnnPolicy', env, verbose=1,buffer_size=int(1e7), tensorboard_log=f"runs/{run.id}", policy_kwargs=dict(normalize_images=False))

model.learn(
    total_timesteps=int(1e8),  # Total number of training steps
    callback=[WandbCallback(
        model_save_path=f"/network/scratch/o/omar.younis/sac_models/{run.id}",
        model_save_freq=10000
    )]
)

model.save('./sac_final_model')
