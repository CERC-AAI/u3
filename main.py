from python.u3_env import create_environment_by_name
import imageio
import numpy as np


if __name__ == "__main__":
    env = create_environment_by_name("XLand", 42)

    obs = env.reset()
    print(obs.shape)

    imgs = []
    for _ in range(64):
        obs, _, done, _ = env.step(env.action_space.sample())
        if done:
            env.reset()
        print(np.all(obs == 0))
        imgs.append(np.moveaxis(obs, 0, -1))
    
    imageio.mimsave("video.gif", imgs, fps=16)
    env.close()
