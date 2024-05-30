import numpy as np
import time
import gymnasium as gym
import u3gym


def time_f(n_envs):
    env = gym.vector.AsyncVectorEnv([
        lambda i=i: gym.make(
            "U3GymEnv-v0",
            file_name='unity/Builds/LinuxTraining/XLand',
            worker_id=i + 2,
            disable_env_checker=True,
            camera_width=64,
            camera_height=64
        )
        for i in range(n_envs)
    ])
    obs, _ = env.reset(seed=42)

    print(obs.shape)
    print(np.all(obs == 0))

    t1 = time.time()
    for _ in range(100):
        obs, _, done, _, _ = env.step(env.action_space.sample())
    
    t2 = time.time()
    SPS = n_envs * 100 / (t2 - t1)
    print(f"FPS with n_envs={n_envs}:", SPS)
    env.close()
    return SPS

if __name__ == "__main__":
    ts = []
    for n_envs in [1, 4, 16, 64]:
        t = time_f(n_envs)
        ts.append(t)
        print(ts)
