FROM gitpod/workspace-dotnet

# Install custom tools, runtimes, etc.
# For example "bastet", a command-line tetris clone:
# RUN brew install bastet
#
# More information: https://www.gitpod.io/docs/config-docker/

RUN dotnet tool install --global coverlet.console \
    && curl -sL install.mob.sh | sudo sh
