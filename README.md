# GitHub Action: Maven Version Checker

![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/StevenJDH/maven-version-checker?include_prereleases)
[![Public workflows that use this action.](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fused-by.vercel.app%2Fapi%2Fgithub-actions%2Fused-by%3Faction%3DStevenJDH%2Fmaven-version-checker%26badge%3Dtrue)](https://github.com/search?o=desc&q=StevenJDH+maven-version-checker+language%3AYAML&s=&type=Code)
![Maintenance](https://img.shields.io/badge/yes-4FCA21?label=maintained&style=flat)
![GitHub](https://img.shields.io/github/license/StevenJDH/maven-version-checker)

Maven Version Checker is a GitHub action that checks for any available updates for maven dependencies and plugins in both single and multi-module projects. It has been compiled to native code using [Ahead-Of-Time (AOT)](https://en.wikipedia.org/wiki/Ahead-of-time_compilation) compilation for increased performance and reduced memory usage. The native code is containerized using an Ubuntu-based [.NET Chiseled Container](https://devblogs.microsoft.com/dotnet/announcing-dotnet-chiseled-containers/) to further reduce the image size while significantly improving security and loading speeds. In fact, the base image is made up of only 6 files, which accounts for less than 10MB of the final image size. Do keep in mind that GitHub [only supports container actions on Linux runners](https://docs.github.com/en/actions/hosting-your-own-runners/managing-self-hosted-runners/about-self-hosted-runners#requirements-for-self-hosted-runner-machines), but as soon as this changes, support will be added.

[![Buy me a coffee](https://img.shields.io/static/v1?label=Buy%20me%20a&message=coffee&color=important&style=flat&logo=buy-me-a-coffee&logoColor=white)](https://www.buymeacoffee.com/stevenjdh)

## Features

* Supports single and multi-module Maven projects.
* Checks for version updates in pom parent, dependencies, plugins, and addons configured in plugins.
* Produces outputs that can be used for additional processing.
* Summary reports are generated after each run.
* Supports being ran locally or from another third-party pipeline.

## Compatibility
Below is a list of GitHub-hosted runners that support jobs using this action.

| Runner     | Supported? | 
|------------|:----------:|
| [![Ubuntu](https://img.shields.io/badge/Ubuntu-E95420?style=flat&logo=ubuntu&logoColor=white)](https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions#jobsjob_idruns-on) | ✅ |
| [![Windows](https://img.shields.io/badge/Windows-0078D6?style=flat\&logo=windows\&logoColor=white)](https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions#jobsjob_idruns-on) | ❌ |
| [![macOS](https://img.shields.io/badge/macOS-000000?style=flat\&logo=macos\&logoColor=F0F0F0)](https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions#jobsjob_idruns-on) | ❌ |

## Inputs
The following inputs are available:

| Name                                         | Type     | Required | Default                      |  Description                                                       |
|----------------------------------------------|----------|:--------:|:----------------------------:|--------------------------------------------------------------------|
| <a name="location"></a>[location](#location) | `string` | `false`  | `./pom.xml`                  | Defines the location of the main pom.xml file for a maven project. |

## Outputs
The following outputs are available:

| Name                                                                    | Type     | Example(s) | Description                                              |
|-------------------------------------------------------------------------|----------|------------|----------------------------------------------------------|
| <a name="has_updates"></a>[has_updates](#has_updates)                   | `string` | true       | Indicates whether or not artifact updates are available. |
| <a name="number_of_updates"></a>[number_of_updates](#number_of_updates) | `string` | 5          | Holds the number of artifact updates available.          |
| <a name="update_json"></a>[update_json](#update_json)                   | `string` | <code>{"parents"&#xFEFF;:&#xFEFF;["example:parent:2.0.0"], "dependencies"&#xFEFF;:&#xFEFF;["foo:bar:2.0.0"], "plugins"&#xFEFF;:&#xFEFF;["marco:polo:2.0.0"]}</code> | A map of artifacts with updates in json format. Note: The 'parents' field is maintained as an array so that processing can use the same code. |

## Usage
Implementing this action is relatively simple with just a few steps.

```yaml
name: 'build'

on:
  push:
    branches:
    - main
  pull_request:
    branches:
    - main
    types: [opened, synchronize, reopened]

jobs:
  build:
    name: Build
    runs-on: ubuntu-latest

    steps:
    - name: Check for Artifact Updates
      id: maven-artifacts
      uses: stevenjdh/maven-version-checker@v1
      with:
        location: './pom.xml'

    - name: Display Action Outputs
      env: ${{ fromJSON(steps.maven-artifacts.outputs.update_json) }}
      run: |
        echo "Action Outputs:"
        echo "- [has_updates]: ${{ steps.maven-artifacts.outputs.has_updates }}"
        echo "- [number_of_updates]: ${{ steps.maven-artifacts.outputs.number_of_updates }}"
        echo "- [update_json]: ${{ steps.maven-artifacts.outputs.update_json }}"
        echo ""
        echo "Deserialized Update JSON:"
        echo "- [parents]: $parents"
        echo "- [dependencies]: $dependencies"
        echo "- [plugins]: ${{ fromJSON(steps.maven-artifacts.outputs.update_json).plugins }}" # Alternative
        echo ""
        
        # One approach to processing an array type field using bash:
        array=($(echo "$plugins" | jq -r '.[]'))
        for element in "${array[@]}"; do
            IFS=":" read -r groupId artifactId version <<< "$element"
            echo "groupId: $groupId"
            echo "artifactId: $artifactId"
            echo -e "version: $version\n"
        done
```

## Running locally or from another third-party pipeline
Since this action is container-based, it can be ran locally or from another third-party pipeline using bash, for example, Azure Pipelines. First, create a [GitHub PAT](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens) with at least `read:packages` permissions from [here](https://github.com/settings/tokens/new?scopes=read:packages), and run the following commands from the root directory of a maven project:

```bash
echo <YOUR_GITHUB_PAT> | docker login ghcr.io -u <YOUR_GITHUB_USERNAME> --password-stdin

docker run --name maven-version-checker --workdir=/data --rm \
  -e INPUT_LOCATION="./pom.xml" \
  -e GITHUB_STEP_SUMMARY="./summary.txt" \
  -e GITHUB_OUTPUT="./output.txt" \
  -v "$(pwd):/data" \
  ghcr.io/stevenjdh/maven-version-checker:latest
```

If all goes well, there will be two generated files called `summary.txt` and `output.txt` that can be leveraged for further processing.

## Disclaimer
Maven Version Checker is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

## Contributing
Thanks for your interest in contributing! There are many ways to contribute to this project. Get started [here](https://github.com/StevenJDH/.github/blob/main/docs/CONTRIBUTING.md).

## Do you have any questions?
Many commonly asked questions are answered in the FAQ:
[https://github.com/StevenJDH/maven-version-checker/wiki/FAQ](https://github.com/StevenJDH/maven-version-checker/wiki/FAQ)

## Want to show your support?

|Method          | Address                                                                                   |
|---------------:|:------------------------------------------------------------------------------------------|
|PayPal:         | [https://www.paypal.me/stevenjdh](https://www.paypal.me/stevenjdh "Steven's Paypal Page") |
|Cryptocurrency: | [Supported options](https://github.com/StevenJDH/StevenJDH/wiki/Donate-Cryptocurrency)    |


// Steven Jenkins De Haro ("StevenJDH" on GitHub)