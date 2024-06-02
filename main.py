import numpy as np
import time
import gymnasium as gym
import u3gym


def time_f(n_envs):
    env = gym.make(
            "U3GymEnv-v0",
            file_name=None,#'unity/Builds/WindowsTraining/unitylearning2',
            worker_id=0,
            disable_env_checker=True,
            camera_width=64,
            camera_height=64,
            world_folder='../../u3_datasets/easy_low',
            rule_folder='../../u3_datasets/short_few',
        )
    obs, _ = env.reset(seed=42)

    print(obs.shape)
    print(np.all(obs == 0))

    n_steps = 1000000

    t1 = time.time()
    for t in range(n_steps):
        obs, _, done, _, _ = env.step(env.action_space.sample())

        print(f"{t}", end = '\r', flush=True)

        if done:
            print('reset')
            env.reset()
    
    t2 = time.time()
    SPS = n_envs * n_steps / (t2 - t1)
    print(f"FPS with n_envs={n_envs}:", SPS)
    env.close()
    return SPS

if __name__ == "__main__":
    ts = []
    for n_envs in [1, 4, 16, 64]:
        t = time_f(n_envs)
        ts.append(t)
        print(ts)
