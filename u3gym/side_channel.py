import uuid

from mlagents_envs.side_channel.side_channel import (
    SideChannel,
    IncomingMessage,
    OutgoingMessage,
)


# Create the StringLogChannel class
class U3SideChannel(SideChannel):
    def __init__(self) -> None:
        super().__init__(uuid.UUID("621f0a70-4f87-11ea-a6bf-784f4387d1f7"))

    def on_message_received(self, msg: IncomingMessage) -> None:
        """
        Note: We must implement this method of the SideChannel interface to
        receive messages from Unity
        """
        # We simply read a string from the message and print it.
        message = str(msg.get_raw_bytes()[4:], "utf_8")
        if (not self.environment.current_step in self.environment.env_messages):
            self.environment.env_messages[self.environment.current_step] = []
        self.environment.env_messages[self.environment.current_step].append(message)
        # print('Unity output: {}'.format(message[0:100]))

    def send_string(self, data: str) -> None:
        # Add the string to an OutgoingMessage
        msg = OutgoingMessage()
        msg.write_string(data)
        # We call this method to queue the data we want to send
        super().queue_message_to_send(msg)

    def set_environment(self, environment):
        self.environment = environment
        self.environment.env_messages = {}
        self.environment.last_env_messages = {}