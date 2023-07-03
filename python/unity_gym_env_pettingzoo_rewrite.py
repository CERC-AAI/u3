import itertools

import numpy as np
from typing import Any, Dict, List, Optional, Tuple, Union

# import gym
from pettingzoo import AECEnv, ParallelEnv
from pettingzoo.utils import conversions

import gymnasium as gym
from gymnasium import spaces, error

# from pettingzoo import error

from mlagents_envs.base_env import ActionTuple, BaseEnv
from mlagents_envs.base_env import DecisionSteps, TerminalSteps
from mlagents_envs import logging_util


class UnityGymException(error.Error):
    """
    Any error related to the gym wrapper of ml-agents.
    """

    pass


logger = logging_util.get_logger(__name__)
GymStepResult = Tuple[np.ndarray, float, bool, Dict]


class UnityToPettingzooWrapper(ParallelEnv):
    """
    Provides Gym wrapper for Unity Learning Environments.
    """

    def __init__(
        self,
        unity_env: BaseEnv,
        uint8_visual: bool = True,
        flatten_branched: bool = False,
        allow_multiple_obs: bool = False,
        action_space_seed: Optional[int] = None,
    ):
        """
        Environment initialization
        :param unity_env: The Unity BaseEnv to be wrapped in the gym. Will be closed when the UnityToGymWrapper closes.
        :param uint8_visual: Return visual observations as uint8 (0-255) matrices instead of float (0.0-1.0).
        :param flatten_branched: If True, turn branched discrete action spaces into a Discrete space rather than
            MultiDiscrete.
        :param allow_multiple_obs: If True, return a list of np.ndarrays as observations with the first elements
            containing the visual observations and the last element containing the array of vector observations.
            If False, returns a single np.ndarray containing either only a single visual observation or the array of
            vector observations.
        :param action_space_seed: If non-None, will be used to set the random seed on created gym.Space instances.
        """
        self._env = unity_env

        # TODO: verify how this works with multiple agents
        # Take a single step so that the brain information will be sent over
        if not self._env.behavior_specs:
            self._env.step()

        # TODO: two functions: one to convert from behavior_spec to string, and one to convert from the string to behavior_spec
        # self.possible_agents = list(self._env.behavior_specs.keys())
        self.possible_agents = []
        for behavior_spec in self._env._env_state.keys():
            for agent_id in self._env._env_state[behavior_spec][0].agent_id:
                self.possible_agents.append(
                    str(behavior_spec) + "_" + str(agent_id)
                )

        self.visual_obs = None

        # Save the step result from the last time all Agents requested decisions.
        self._previous_decision_step: Optional[DecisionSteps] = None
        self._flattener = None

        # Hidden flag used by Atari environments to determine if the game is over
        self.game_over = False
        self._allow_multiple_obs = allow_multiple_obs

        # Check brain configuration
        # TODO : change this to allow multiple behaviors (agents)
        if len(self._env.behavior_specs) != 1:
            raise UnityGymException(
                "There can only be one behavior in a UnityEnvironment "
                "if it is wrapped in a gym."
            )

        # TODO: change this to deal with multiple behavior specs
        self.name = list(self._env.behavior_specs.keys())[0]
        self.group_spec = self._env.behavior_specs[self.name]

        # TODO: if there are no observations, ask unity to give us init in side channel
        if self._get_n_vis_obs() == 0 and self._get_vec_obs_size() == 0:
            raise UnityGymException(
                "There are no observations provided by the environment."
            )

        if not self._get_n_vis_obs() >= 1 and uint8_visual:
            logger.warning(
                "uint8_visual was set to true, but visual observations are not in use. "
                "This setting will not have any effect."
            )
        else:
            self.uint8_visual = uint8_visual
        if (
            self._get_n_vis_obs() + self._get_vec_obs_size() >= 2
            and not self._allow_multiple_obs
        ):
            logger.warning(
                "The environment contains multiple observations. "
                "You must define allow_multiple_obs=True to receive them all. "
                "Otherwise, only the first visual observation (or vector observation if"
                "there are no visual observations) will be provided in the observation."
            )

        # Check for number of agents in scene.
        self._env.reset()
        # decision_steps, _ = self._env.get_steps(self.name)
        # self._check_agents(len(decision_steps))
        # self._previous_decision_step = decision_steps

        # TODO: Rewrite setting of action space for pettingzoo
        # Set action spaces
        # action_spec has a counter for each discrete and continuous action
        if (
            self.group_spec.action_spec.is_discrete()
            and not self.group_spec.action_spec.is_continuous()
        ):
            self.action_size = self.group_spec.action_spec.discrete_size
            branches = self.group_spec.action_spec.discrete_branches
            if self.group_spec.action_spec.discrete_size == 1:
                self._action_space = spaces.Discrete(branches[0])
            else:
                if flatten_branched:
                    self._flattener = ActionFlattener(branches)
                    self._action_space = self._flattener.action_space
                else:
                    self._action_space = spaces.MultiDiscrete(branches)

        elif (
            self.group_spec.action_spec.is_continuous()
            and not self.group_spec.action_spec.is_discrete()
        ):
            if flatten_branched:
                logger.warning(
                    "The environment has a non-discrete action space. It will "
                    "not be flattened."
                )

            self.action_size = self.group_spec.action_spec.continuous_size
            high = np.array([1] * self.group_spec.action_spec.continuous_size)
            self._action_space = spaces.Box(-high, high, dtype=np.float32)

        elif (
            self.group_spec.action_spec.is_discrete()
            and self.group_spec.action_spec.is_continuous()
        ):
            discrete_action_space = None
            continuous_action_space = None
            discrete_action_size = self.group_spec.action_spec.discrete_size
            discrete_branches = self.group_spec.action_spec.discrete_branches
            continuous_action_size = self.group_spec.action_spec.continuous_size
            if discrete_action_size == 1:
                self._action_space = spaces.Discrete(discrete_branches[0])
            else:
                if flatten_branched:
                    self._flattener = ActionFlattener(discrete_branches)
                    discrete_action_space = self._flattener.action_space
                else:
                    discrete_action_space = spaces.MultiDiscrete(branches)
            continuous_action_size = self.group_spec.action_spec.continuous_size
            high = np.array([1] * continuous_action_size)
            continuous_action_space = spaces.Box(-high, high, dtype=np.float32)
            self._action_space = spaces.Tuple(
                (discrete_action_space, continuous_action_space)
            )
            pass

        else:
            raise UnityGymException(
                "Please make sure that group_spec.action_spec.is_discrete() or group_spec.action_spec.is_continuous() return"
            )

        if action_space_seed is not None:
            self._action_space.seed(action_space_seed)

        # MAYBE TODO: Rewrite setting of observation space for pettingzoo
        # Set observations space
        list_spaces: List[gym.Space] = []
        shapes = self._get_vis_obs_shape()
        for shape in shapes:
            if uint8_visual:
                list_spaces.append(
                    spaces.Box(0, 255, dtype=np.uint8, shape=shape)
                )
            else:
                list_spaces.append(
                    spaces.Box(0, 1, dtype=np.float32, shape=shape)
                )
        if self._get_vec_obs_size() > 0:
            # vector observation is last
            high = np.array([np.inf] * self._get_vec_obs_size())
            list_spaces.append(spaces.Box(-high, high, dtype=np.float32))
        if self._allow_multiple_obs:
            self._observation_space = spaces.Tuple(list_spaces)
        else:
            self._observation_space = list_spaces[
                0
            ]  # only return the first one

    def unique_id_to_behavior_name_and_agent_id(self, unique_id):
        return unique_id.split("_")

    def set_action_for_agent_unique_id(self, unique_id, action_tuple):
        behavior_name, agent_id = self.unique_id_to_behavior_name_and_agent_id(
            unique_id
        )
        self._env._assert_behavior_exists(behavior_name)
        if behavior_name not in self._env._env_state:
            return
        action_spec = self._env._env_specs[behavior_name].action_spec
        action_tuple = action_spec._validate_action(
            action_tuple, 1, behavior_name
        )
        if behavior_name not in self._env._env_actions:
            num_agents = len(self._env._env_state[behavior_name][0])
            self._env._env_actions[behavior_name] = action_spec.empty_action(
                num_agents
            )
        try:
            index = self._env._env_state[behavior_name][0].agent_id_to_index[
                int(agent_id)
            ]
        except IndexError as ie:
            raise IndexError(
                "agent_id {} is did not request a decision at the previous step".format(
                    agent_id
                )
            ) from ie
        if action_spec.continuous_size > 0:
            self._env._env_actions[behavior_name].continuous[
                index
            ] = action_tuple.continuous[0, :]
        if action_spec.discrete_size > 0:
            self._env._env_actions[behavior_name].discrete[
                index
            ] = action_tuple.discrete[0, :]

    def action_spaces(self, agent_name):
        return self._action_space

    def observation_spaces(self, agent_name):
        return self._observation_space

    def reset(self) -> Union[List[np.ndarray], np.ndarray]:
        """Resets the state of the environment and returns an initial observation.
        Returns: observation (object/list): the initial observation of the
        space.
        """
        self._env.reset()
        self.agents = self.possible_agents[:]
        self.num_moves = 0
        observations = {agent: None for agent in self.agents}
        infos = {agent: {} for agent in self.agents}

        # decision_step, _ = self._env.get_steps(self.name)
        # n_agents = len(decision_step)
        # self._check_agents(n_agents)
        # self.game_over = False
        # res: GymStepResult = self._single_step(decision_step)
        return observations, infos

    def step(self, actions):
        # """Run one timestep of the environment's dynamics. When end of
        # episode is reached, you are responsible for calling `reset()`
        # to reset this environment's state.
        # Accepts an action and returns a tuple (observation, reward, done, info).
        # Args:
        #     action (object/list): an action provided by the environment
        # Returns:
        #     observation (object/list): agent's observation of the current environment
        #     reward (float/list) : amount of reward returned after previous action
        #     done (boolean/list): whether the episode has ended.
        #     info (dict): contains auxiliary diagnostic information.
        # """
        """
        step(action) takes in an action for each agent and should return the
        - observations
        - rewards
        - terminations
        - truncations
        - infos
        dicts where each dict looks like {agent_1: item_1, agent_2: item_2}
        """

        if self.game_over:
            raise UnityGymException(
                "You are calling 'step()' even though this environment has already "
                "returned done = True. You must always call 'reset()' once you "
                "receive 'done = True'."
            )

        if not actions:
            self.agents = []
            return {}, {}, {}, {}, {}

        # Are flatteners relevant? (probably not)
        # if self._flattener is not None:
        #     # Translate action into list
        #     action = self._flattener.lookup_action(action)

        # TODO: change to work for multiple agents
        self.agents = self.possible_agents[:]
        agent_actions = actions[self.agents[0]]

        observations = {unique_id: {} for unique_id in self.agents}
        rewards = {unique_id: {} for unique_id in self.agents}
        terminations = {unique_id: {} for unique_id in self.agents}
        infos = {unique_id: {} for unique_id in self.agents}

        # TODO: fill action_tuple with actions provided by env
        # always have a discrete continuous tuple, and if the discrete is empty, just have the first element be null
        # self._env.set_actions  :param action: ActionTuple tuple of continuous and/or discrete action.
        # Actions are np.arrays with dimensions  (n_agents, continuous_size) and
        # (n_agents, discrete_size), respectively.        action_tuple = ActionTuple(action)
        action_tuple = ActionTuple(
            continuous=agent_actions["continuous"],
            discrete=agent_actions["discrete"],
        )

        # TODO: fix self.name to deal with multiple agents
        for unique_id in self.agents:
            self.set_action_for_agent_unique_id(unique_id, action_tuple)

        self._env.step()

        for unique_id in self.agents:
            (
                behavior_name,
                agent_id,
            ) = self.unique_id_to_behavior_name_and_agent_id(unique_id)

            decision_step, terminal_step = self._env.get_steps(behavior_name)
            # get the obs corresponding to agent_id by using agent_id_to_index
            # TODO: modify this to handle multiple agents
            # self._check_agents(max(len(decision_step), len(terminal_step)))

            if len(terminal_step) != 0:
                # The agent is done
                self.game_over = True
                (
                    observations[unique_id],
                    rewards[unique_id],
                    terminations[unique_id],
                ) = self._single_step(terminal_step, agent_id)
                # TODO: return inside a for loop will exit the loop, so this will only return the first agent's info
            else:
                (
                    observations[unique_id],
                    rewards[unique_id],
                    terminations[unique_id],
                ) = self._single_step(decision_step, agent_id)
        return (observations, rewards, terminations, {}, infos)

    # TODO: pick up here
    def _single_step(
        self, info: Union[DecisionSteps, TerminalSteps], agent_id: int
    ):  # TODO: always allow multiple observations, this flag is superfluous
        agent_index = info.agent_id_to_index[int(agent_id)]

        visual_obs = self._get_vis_obs_list(info)
        visual_obs_list = []
        for obs in visual_obs:
            visual_obs_list.append(self._preprocess_single(obs[agent_index]))
        default_observation = visual_obs_list
        if self._get_vec_obs_size() >= 1:
            default_observation.append(self._get_vector_obs(info)[0, :])

        done = isinstance(info, TerminalSteps)
        observations = default_observation[0]
        rewards = info.reward[agent_index]
        terminations = done

        return observations, rewards, terminations

    def _preprocess_single(self, single_visual_obs: np.ndarray) -> np.ndarray:
        if self.uint8_visual:
            return (255.0 * single_visual_obs).astype(np.uint8)
        else:
            return single_visual_obs

    def _get_n_vis_obs(self) -> int:
        result = 0
        for obs_spec in self.group_spec.observation_specs:
            if len(obs_spec.shape) == 3:
                result += 1
        return result

    def _get_vis_obs_shape(self) -> List[Tuple]:
        result: List[Tuple] = []
        for obs_spec in self.group_spec.observation_specs:
            if len(obs_spec.shape) == 3:
                result.append(obs_spec.shape)
        return result

    def _get_vis_obs_list(
        self, step_result: Union[DecisionSteps, TerminalSteps]
    ) -> List[np.ndarray]:
        result: List[np.ndarray] = []
        for obs in step_result.obs:
            if len(obs.shape) == 4:
                result.append(obs)
        return result

    def _get_vector_obs(
        self, step_result: Union[DecisionSteps, TerminalSteps]
    ) -> np.ndarray:
        result: List[np.ndarray] = []
        for obs in step_result.obs:
            if len(obs.shape) == 2:
                result.append(obs)
        return np.concatenate(result, axis=1)

    def _get_vec_obs_size(self) -> int:
        result = 0
        for obs_spec in self.group_spec.observation_specs:
            if len(obs_spec.shape) == 1:
                result += obs_spec.shape[0]
        return result

    def render(self, mode="rgb_array"):
        """
        Return the latest visual observations.
        Note that it will not render a new frame of the environment.
        """
        return self.visual_obs

    def close(self) -> None:
        """Override _close in your subclass to perform any necessary cleanup.
        Environments will automatically close() themselves when
        garbage collected or when the program exits.
        """
        self._env.close()

    def seed(self, seed: Any = None) -> None:
        """Sets the seed for this env's random number generator(s).
        Currently not implemented.
        """
        logger.warning("Could not seed environment %s", self.name)
        return

    # TODO: maintain a list of agents in the wrapper and have some way to identify which agent is which
    # TODO: Unity might not return the same number of agents at every timestep since they might not all update
    @staticmethod
    def _check_agents(n_agents: int) -> None:
        # TODO: change this
        if n_agents > 2:
            raise UnityGymException(
                f"There can only be one Agent in the environment but {n_agents} were detected."
            )

    @property
    def metadata(self):
        return {"render.modes": ["rgb_array"]}

    @property
    def reward_range(self) -> Tuple[float, float]:
        return -float("inf"), float("inf")

    @property
    def action_space(self) -> gym.Space:
        return self._action_space

    @property
    def observation_space(self):
        return self._observation_space


class ActionFlattener:
    """
    Flattens branched discrete action spaces into single-branch discrete action spaces.
    """

    def __init__(self, branched_action_space):
        """
        Initialize the flattener.
        :param branched_action_space: A List containing the sizes of each branch of the action
        space, e.g. [2,3,3] for three branches with size 2, 3, and 3 respectively.
        """
        self._action_shape = branched_action_space
        self.action_lookup = self._create_lookup(self._action_shape)
        self.action_space = spaces.Discrete(len(self.action_lookup))

    @classmethod
    def _create_lookup(self, branched_action_space):
        """
        Creates a Dict that maps discrete actions (scalars) to branched actions (lists).
        Each key in the Dict maps to one unique set of branched actions, and each value
        contains the List of branched actions.
        """
        possible_vals = [range(_num) for _num in branched_action_space]
        all_actions = [
            list(_action) for _action in itertools.product(*possible_vals)
        ]
        # Dict should be faster than List for large action spaces
        action_lookup = {
            _scalar: _action for (_scalar, _action) in enumerate(all_actions)
        }
        return action_lookup

    def lookup_action(self, action):
        """
        Convert a scalar discrete action into a unique set of branched actions.
        :param action: A scalar value representing one of the discrete actions.
        :returns: The List containing the branched actions.
        """
        return self.action_lookup[action]
