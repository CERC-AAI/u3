from python.u3_env import create_environment_by_name
import imageio


if __name__ == "__main__":
    env = create_environment_by_name("XLand", 1)

    print(env.reset().shape)

    imgs = []
    for _ in range(64):
        obs, _, done, _ = env.step(env.action_space.sample())
        # img = env.render()
        imgs.append(obs)
    
    imageio.mimsave("video.gif", imgs, fps=16)
    env.close()