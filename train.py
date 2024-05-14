from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper

from python.u3_env import U3Environment, U3SideChannel, U3Wrapper, create_environment_by_name
import imageio


if __name__ == "__main__":
    env = create_environment_by_name("XLand", 1)

    print(env.reset().shape)

    imgs = []
    for _ in range(128):
        obs, _, _, _ = env.step(env.action_space.sample())
        img = env.render()
        imgs.append(img)
    
    imageio.mimsave("video.gif", imgs, fps=16)
    env.close()