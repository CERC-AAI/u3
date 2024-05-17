from python.u3_env import create_environment_by_name
import imageio
import numpy as np


if __name__ == "__main__":
    env = create_environment_by_name("Ball", 42)

    obs, _ = env.reset()
    print(obs.shape)

    imgs = []
    for _ in range(64):
        obs, _, done, _, _ = env.step(env.action_space.sample())
        if done:
            env.reset()
        print(np.all(obs == 0))
        imgs.append(obs)
    
    imageio.mimsave("video.gif", imgs, fps=16)
    env.close()
