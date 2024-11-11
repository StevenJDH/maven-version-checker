# This file is part of Maven Version Checker <https://github.com/StevenJDH/maven-version-checker>.
# Copyright (C) 2024 Steven Jenkins De Haro.
#
# Maven Version Checker is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# Maven Version Checker is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with Maven Version Checker.  If not, see <http://www.gnu.org/licenses/>.

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-noble-chiseled AS base
# Chiseled images use a non-root user by default, so it has to be set explicitly to 'root' to meet GitHub Action requirements.
# Otherwise, existing non-root 'USER app' or 'USER $APP_UID' can be set, which is part of runtime-deps.
# References:
# - https://docs.github.com/en/actions/creating-actions/dockerfile-support-for-github-actions#user
# - https://github.com/dotnet/dotnet-docker/blob/main/documentation/ubuntu-chiseled.md#how-do-i-handle-file-permissions-when-running-as-a-non-root-user
USER root
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Install clang/zlib1g-dev dependencies for publishing to native.
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    clang zlib1g-dev && apt-get clean
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
# Comment this line out if building from action.yml file to handle differences in build context.
WORKDIR "/src/MavenVersionChecker.Action"
RUN chmod +x ./Locals/test-action && dotnet build -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=true

FROM base AS final
COPY --from=publish /app/publish .
ENTRYPOINT ["/app/MavenVersionChecker.Action"]