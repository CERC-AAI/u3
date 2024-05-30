# coding=utf-8
# Copyright 2019 The SEED Authors
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
# 		 http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

"""Atari env factory."""

from mlagents_envs.environment import UnityEnvironment
from absl import flags
import json


from typing import Dict, Any
from mlagents_envs.base_env import BaseEnv
from u3gym.side_channel import U3SideChannel
from unity_gym_env_pettingzoo_rewrite import UnityToPettingzooWrapper


FLAGS = flags.FLAGS

# Environment settings.
flags.DEFINE_string("game", "ButtonGame", "Game name.")
flags.DEFINE_integer("num_action_repeats", 1, "Number of action repeats.")
flags.DEFINE_integer(
    "max_random_noops",
    30,
    "Maximal number of random no-ops at the beginning of each " "episode.",
)


class U3Wrapper(UnityToPettingzooWrapper):
    def __init__(
        self,
        unity_env: BaseEnv,
        sideChannel: U3SideChannel,
        uint8_visual: bool = False,
        flatten_branched: bool = False,
        allow_multiple_obs: bool = False,
        parameters: Dict[str, object] = {},
        task_name: str = "xland",
    ):
        self.sideChannel = sideChannel
        self.sideChannel.set_environment(self)

        self.init_env(task_name, parameters)

        super(U3Wrapper, self).__init__(
            unity_env=unity_env,
            uint8_visual=uint8_visual,
            flatten_branched=flatten_branched,
            allow_multiple_obs=allow_multiple_obs,
        )

    def init_env(self, env_name: str, parameters: Dict[str, object]) -> None:
        """Sets a parameter of this env.
        Parameters are in JSON format.
        """
        
        json_message = {}
        json_message["env"] = env_name # TODO Make this work with multiple envs in one Unity instance
        json_message["msg"] = "init"
        json_message["data"] = parameters

        string_message = json.dumps(json_message)

        self.sideChannel.send_string(string_message)
        return

    def seed(self, seed: Any = None) -> None:
        """Sets the seed for this env's random number generator(s).
        Currently not implemented.
        """
        self.sideChannel.send_string("seed{}".format(seed))
        return

    def setTraining(self, isTraining: bool) -> None:
        """Sets the seed for this env's random number generator(s).
        Currently not implemented.
        """
        if isTraining:
            self.sideChannel.send_string("training1")
        else:
            self.sideChannel.send_string("training0")

        return

    def setStatic(self, isStatic: bool) -> None:
        """Sets the seed for this env's random number generator(s).
        Currently not implemented.
        """
        if isStatic:
            self.sideChannel.send_string("static1")
        else:
            self.sideChannel.send_string("static0")

        return

    def setElement(self, elementName: str, elementData: str) -> None:
        """Sets the seed for this env's random number generator(s).
        Currently not implemented.
        """
        self.sideChannel.send_string(
            "element{}|{}".format(elementName, elementData)
        )

        return

    def clearElements(self) -> None:
        """Sets the seed for this env's random number generator(s).
        Currently not implemented.
        """
        self.sideChannel.send_string("reset")

        return
    
    def reset(self, parameters : Dict[str, object] = {}) -> None:
        json_message = {}
        json_message["env"] = 1 # TODO Make this work with multiple envs in one Unity instance
        json_message["msg"] = "reset"
        json_message["data"] = parameters

        string_message = json.dumps(json_message)

        self.sideChannel.send_string(string_message)

        super().reset()

        self.last_env_messages = self.env_messages
        self.env_messages = {}


def create_environment(
    file_name=None,
    task_name="xland",
    worker_id=0,
    parameters={},
    seed=0,
    uint8_visual=True
):
    environmentChannel = U3SideChannel()
    unity_env = UnityEnvironment(file_name=file_name, seed=seed, side_channels=[environmentChannel], worker_id=worker_id)
    
    env = U3Wrapper(
        unity_env, environmentChannel, flatten_branched=True, uint8_visual=uint8_visual, parameters=parameters, task_name=task_name
    )
    env.seed(seed)

    return env