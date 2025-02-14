# Flappy Bird Game Clone

A modern C# Windows Forms implementation of the classic Flappy Bird game, featuring smooth animations, progressive difficulty, and visual effects.

## 🎮 Features

- Smooth bird movement with gravity physics
- Dynamic obstacle generation
- Progressive difficulty increase
- Visual effects including:
  - Score glow animations
  - Bird rotation based on velocity
  - Game over screen with effects
- Speed multiplier system
- High score tracking

## 🔧 Technical Details

- Built with .NET 9.0 Windows Forms
- Written in C# with modern coding practices
- Uses GDI+ for enhanced graphics rendering
- Implements proper resource management and disposal
- Double buffered rendering for smooth animations

## 🚀 Getting Started

### Prerequisites

- Windows OS
- .NET 9.0 SDK or later
- Visual Studio 2022 or later

### Installation

1. Clone the repository
```bash
git clone [your-repository-url]
```

2. Navigate to the project directory
```bash
cd FlappyBird
```

3. Create an `Images` folder and add the required images:
   - `myBird.png` - The bird sprite
   - `Pipe.jpeg` - The pipe obstacle
   - `pipe_part.jpeg` - The pipe end piece
   - `background.jpeg` - The game background

4. Build and run the project
```bash
dotnet build
dotnet run
```

## 🎯 How to Play

- Press **SPACE** to make the bird jump
- Avoid hitting the pipes
- Score increases as you pass through pipes
- Game speed increases every 5 points
- Press **R** to restart after game over

## 🎨 Game Mechanics

- Bird Physics:
  - Constant gravity effect
  - Jump force for upward movement
  - Rotation based on velocity

- Difficulty Progression:
  - Base speed: 7 units
  - Speed increases every 5 points
  - Maximum speed multiplier: 3.0x
  - Progressive gap positioning

## 🛠️ Technical Implementation

- **Graphics**: Uses GDI+ with double buffering
- **Animation**: Custom timer-based animation system
- **Memory Management**: Proper resource disposal
- **Collision Detection**: Rectangle-based collision system
- **Performance**: Optimized rendering and update loops


# Required Game Images

Place the following image files in this directory:

1. `myBird.png` - Bird sprite
2. `Pipe.jpeg` - Pipe obstacle
3. `pipe_part.jpeg` - Pipe end piece
4. `background.jpeg` - Game background

Note: Make sure the images are appropriately sized and formatted for the game.
