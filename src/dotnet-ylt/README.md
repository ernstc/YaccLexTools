![Nuget](https://img.shields.io/nuget/dt/vs-secrets) ![Nuget](https://img.shields.io/nuget/v/vs-secrets)

# ***Visual Studio Solution Secrets***

Tool for syncing Visual Studio solution secrets across different development machines.

* [Get Started](#get-started)
* [Best Practices](#best-practices)
* [The Problem](#the-problem)
* [The Solution](#the-solution)
* [How to use it](#how-to-use-it)
* [Visual Studio Solution Secrets files](#visual-studio-solution-secrets-files)

# Get Started

If you already know it, here are the quick start commands.

```
dotnet tool install --global vs-secrets
```
```
vs-secrets init -p <your-passphrase>
```
```
vs-secrets pull
```

# Best Practices

If you are good in DevOps practices, you should know that secrets (sensitive data like passwords, connection strings, access keys, etc.) must not be committed with your code in any case and must not be deployed with the apps.

Fortunately Visual Studio and .Net help us in separating secrets from our code with the ***User Secrets Manager*** tool that let us store secrets out of the solution folder. The User Secrets Manager hides implementation details, but essentially it stores secrets in files located in the machine's user profile folder.

You can find the **User Secrets Manager** documentation [here](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows#secret-manager).

# The Problem

When you change your development machine usually you clone your project code from a remote repository and then you would like to be up and running for developing and testing you code in a matter of seconds.

But if you have managed secrets with the tool User Secrets Manager you will not be immediatly able to test your code because you will miss something very important on your new machine: **the secret settings** that let your code work.

# The Solution

For being immediatley ready to start developing and testing on the new development machine you have three choices.

1) Manually copy secret files from the old machine to the new one, if you still have access to the old machine.
2) Recreate the secret settings on your new machine for each project of the solution, but this can be tedious because you have to recover passwords, keys, etc. from different resources and it can be time consuming.
3) Use **Visual Studio Solution Secrets** tool for synchronizing secret settings through the cloud in a quick and secure way.

The idea is to use GitHub Gists as the repository for your secrets. Visual Studio Solution Secrets collects all the secret settings used in the solution, encrypts and pushes them on GitHub in a secret Gist, so that only you can see them. The encryption key is generated from a passphrase or a key file that you specify during the one time initialization phase of the tool.

Once you change the development machine, you don't have to copy any file from the old one. Just install the tool, recreate the encryption key with your passphrase or your key file, authorize the tool on GitHub and you are ready.

![Concept](https://raw.githubusercontent.com/ernstc/VisualStudioSolutionSecrets/main/Concept.png)

# How to use it

For installing the tool, use the command below:

```
dotnet tool install --global vs-secrets
```

If you already have the tool, but you want to update to the latest version, use the command:

```
dotnet tool update --global vs-secrets
```

After you have installed the tool, you need to create the encryption key and then authorize it to use yours GitHub Gists. 
You can do this with the command:
```
vs-secrets init -p <your-passphrase>
```
For creating the encryption key, by default the tool will ask you for a passphrase. If you prefer, you can use a key file as the input to the encryption key generation algorithm with the command below:
```
vs-secrets init --keyfile <file-path>
```

## Push solution secrets

For pushing the secrets of the solution in current folder:
```
vs-secrets push
```
For pushing the secrets of the solution in another folder:
```
vs-secrets push --path <solution-path>
```
For pushing the secrets of all the solutions in a folder tree:
```
vs-secrets push --all
```
or
```
vs-secrets push --path <path> --all
```

## Pull solution secrets

For pulling the secrets of the solution in current folder:
```
vs-secrets pull
```
For pulling the secrets of the solution in another folder:
```
vs-secrets pull --path <solution-path>
```
For pulling the secrets of all the solutions in a folder tree:
```
vs-secrets pull --all
```
or
```
vs-secrets pull --path <path> --all
```

## Searching for solution secrets

You can also use the tool for just searching solutions and projects that use secrets
```
vs-secrets search
```
```
vs-secrets search --path <solution-path>
```
```
vs-secrets search --all
```
```
vs-secrets search --path <path> --all
```

# Visual Studio Solution Secrets files

Visual Studio Solution Secrets tool stores its files in the machine's user profile folder.

| Platform | Path |
|----------|------|
| Windows | `%APPDATA%\Visual Studio Solution Secrets` |
| macSO | `~/.config/Visual Studio Solution Secrets` |
| Linux | `~/.config/Visual Studio Solution Secrets` |

Below are listed the files generated by the tool.

| File | Description |
|------|-------------|
| cipher.json | Contains the encryption key |
| github.json | Contains the access token for managing user's GitHub Gists |

