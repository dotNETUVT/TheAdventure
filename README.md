# TheAdventure

TheAdventure is a C# game project using SDL2 and Silk.NET for rendering. The game includes various features such as player movement, health management, and a superpower with a cooldown mechanism.

## Features

- **Player Movement**: Use the arrow keys to move the player around the game world.
- **Health Management**: The player has a health bar that decreases when taking damage from game objects.
- **Superpower**: The player can activate a superpower that temporarily increases speed. The superpower has a cooldown period and an icon indicating its status.

## Installation

1. **Clone the repository**:

```bash
git clone https://github.com/yourusername/TheAdventure.git
cd TheAdventure
```

2. **Install .NET SDK**:

Make sure you have the .NET SDK installed. You can download it from [here](https://dotnet.microsoft.com/download).

3. **Restore dependencies**:
    
```bash
dotnet restore
```

## Running the Game

To run the game, use the following command:
```bash
dotnet run --project TheAdventure
```

## Formatting Code

This project uses `dotnet format` for code formatting. To format the code, run the following command in the project directory:

### Formatting the Solution

```sh
dotnet format TheAdventure.sln
```

### Formatting a Specific Project

```sh
dotnet format TheAdventure.csproj
```

Ensure your code adheres to the project's style guidelines by running these commands before committing changes.

## Contributing

1. Fork the repository.

2. Create a new branch for your feature or bugfix.

3. Make your changes.

4. Ensure the code is formatted using `dotnet format`.

5. Commit and push your changes to your fork.

6. Create a pull request detailing your changes.
