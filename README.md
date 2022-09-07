# Barriot for Discord - Serving the user.

A fully REST Discord bot implementing exclusively QOL &amp; entertainment for all users.

### 🌍 Visit the website!

- [Visit now](https://barriot.xyz) to invite the bot, get support & vote to support Barriot yourself!
> Voters get rewards such as custom Embed colors, access to more information about users, and much more.

### 🔗 Commands

- Please visit the [command docs](https://github.com/Rozen4334/Barriot/blob/master/COMMANDS.md) to see all commands and their use cases.
> Command documentation is also available on each individual command description. If a command is very unclear, and documentation does not clarify well enough, please open an issue covering it's lack of context.

### 🔏 Privacy

- Please read the [privacy policy](https://github.com/Rozen4334/Barriot/blob/master/PRIVACY.md) carefully if you have any concern about how Barriot handles your information.

### 🗑️ Data removal

- If you wish to have all stored data associated to you removed, Feel free to get into contact at `contact@rozen.one` or on any other platform. Please include your Discord user ID to speed up the process.
> Data will be removed up until the point of re-interacting with Barriot. If you wish to avoid this, please block the bot on Discord.

### ❌ Issues

- Bugs, security concerns & other issues related to Barriot can be opened in this repository, and will be handled as soon as possible.
> If you are interested in committing a code fix directly, you can open a PR following the contribution guidelines below.

----

## Code & Contribution

This project is mainly developed by me, with assistance of [Cenngo](https://github.com/Cenngo) in handling features like REST & interactions. If you have any interest in contributing, feel free to open a PR.

### 🎌 Code flags

- _**TODO**_
  - This flag should be investigated commonly for reflection on code and to revisit certain sections

```cs
// TODO,
// This is an example todo comment.
// Some other information.
```

- _**IN PROGRESS**_
  - This flag is used when marking progression on certain things but having to lay it off until further notice. Often marked for Discord.NET bugs that will be resolved.

```cs
// IN PROGRESS
// Waiting for (issue link) to be resolved.
``` 

### 🌿 Branch structure

- `unit/x` 
  - To unit test a feature.
- `fix/x` 
  - To fix an issue.
- `feature/x` 
  - To implement a new feature.
- `master` 
  - The main development branch, to which no direct commits are pushed outside of repo maintenance

### 📅 Version structure

`Major.Minor.Patch` as intended by ISO standard. Semantic versioning applies. 
Patches are often not incremented as minor version is already incremented before a patch is deployed due to the rapid development on features.

Version changes are made in the `.csproj` file, nowhere else. 
All occurences of versioning inside the code are grabbed from the project assembly, defined from `.csproj`.
