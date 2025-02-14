using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace FlappyBird
{
    public partial class MainForm : Form
    {
        // Add field to track if a reset is in progress
        private bool isResetting = false;

        // Update speed constants for faster gameplay
        private const int BASE_PIPE_SPEED = 7;  // Increased from 6 to 7
        private const float SPEED_INCREASE_PER_5_POINTS = 0.5f;  // Increased from 0.4f to 0.5f
        private const float MAX_SPEED_MULTIPLIER = 3.0f;  // Increased from 2.5f to 3.0f
        
        // Add current speed multiplier field
        private float currentSpeedMultiplier = 1.0f;

        private Bird bird = null!;
        private System.Windows.Forms.Timer gameTimer = null!;
        private PictureBox[] pipes = null!;
        private PictureBox[] pipeEnds = null!;
        private int score;
        private bool isGameOver;
        private Random random = null!;
        private Image birdImage = null!;
        private Image pipeImage = null!;
        private Image pipeEndImage = null!;
        private Image backgroundImage = null!;

        // Update font and color fields
        private readonly Font scoreFont = new Font("Segoe UI", 28, FontStyle.Bold); // Slightly smaller font
        private readonly Font gameOverFont = new Font("Segoe UI", 72, FontStyle.Bold);
        private readonly Font restartFont = new Font("Segoe UI Light", 28, FontStyle.Regular);
        private readonly Font highScoreFont = new Font("Segoe UI", 24, FontStyle.Bold);
        
        // Modern color scheme
        private readonly Color scoreGlow = Color.FromArgb(255, 247, 142);  // Add this line
        private readonly Color scoreColor = Color.FromArgb(255, 255, 255); // Change to solid white
        private readonly Color gameOverGlow = Color.FromArgb(220, 20, 60);
        private float glowIntensity = 0f; // Changed to non-readonly for animation
        private readonly System.Windows.Forms.Timer glowTimer; // Specify full namespace

        // Add this with other field declarations
        private readonly StringFormat centerFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        // Update pipe constants for better spacing
        private const int PIPE_GAP = 200;  // Increased gap between pipes
        private const int PIPE_SPACING = 400;  // Increased from 300 to ensure consistent spacing
        private const int PIPE_HEIGHT = 300;
        private const int MIN_PIPE_TOP = -180;  // Minimum top pipe position
        private const int MAX_OFFSET = 60;  // Reduced random offset for more consistent gaps

        // Add new field to track last scored pipe
        private int lastScoredPipeIndex = -1;

        public MainForm()
        {
            InitializeComponent();
            
            // Initialize glowTimer with full namespace
            glowTimer = new System.Windows.Forms.Timer { Interval = 50 };
            glowTimer.Tick += (s, e) => {
                glowIntensity = (float)((Math.Sin(DateTime.Now.TimeOfDay.TotalSeconds * 3) + 1) / 2);
                this.Invalidate();
            };
            glowTimer.Start();
            
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Unsubscribe existing event handlers first
            if (gameTimer != null)
            {
                gameTimer.Tick -= GameTimer_Tick;
                gameTimer.Dispose();
            }
            this.KeyDown -= MainForm_KeyDown;
            this.Paint -= MainForm_Paint;

            if (isResetting)
            {
                // Stop all timers during reset
                gameTimer?.Stop();
                glowTimer?.Stop();
            }

            DisposePreviousImages();
            DisposePreviousControls();

            // Reset game state with fresh timer
            score = 0;
            isGameOver = false;
            isResetting = false;
            lastScoredPipeIndex = -1;
            currentSpeedMultiplier = 1.0f;

            try
            {
                string projectDir = AppDomain.CurrentDomain.BaseDirectory;
                string imagesDir = Path.Combine(projectDir, "Images");

                if (!Directory.Exists(imagesDir))
                {
                    Directory.CreateDirectory(imagesDir);
                    MessageBox.Show($"Created Images directory at: {imagesDir}\nPlease place the required images in this folder.");
                    Application.Exit();
                    return;
                }

                // Load images with proper memory management and more detailed error handling
                try
                {
                    using (var stream = new MemoryStream(File.ReadAllBytes(Path.Combine(imagesDir, "myBird.png"))))
                    {
                        birdImage = new Bitmap(stream);
                    }
                    using (var stream = new MemoryStream(File.ReadAllBytes(Path.Combine(imagesDir, "Pipe.jpeg"))))
                    {
                        pipeImage = new Bitmap(stream);
                    }
                    using (var stream = new MemoryStream(File.ReadAllBytes(Path.Combine(imagesDir, "pipe_part.jpeg"))))
                    {
                        pipeEndImage = new Bitmap(stream);
                    }
                    using (var stream = new MemoryStream(File.ReadAllBytes(Path.Combine(imagesDir, "background.jpeg"))))
                    {
                        backgroundImage = new Bitmap(stream);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading specific image: {ex.Message}\n\nRequired images:\n- myBird.png\n- Pipe.jpeg\n- pipe_part.jpeg\n- background.jpeg");
                    Application.Exit();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"General error: {ex.Message}\nTried path: {AppDomain.CurrentDomain.BaseDirectory}\\Images");
                Application.Exit();
                return;
            }

            bird = new Bird(this.ClientSize.Width / 3, this.ClientSize.Height / 2);
            
            pipes = new PictureBox[4];
            pipeEnds = new PictureBox[4];
            random = new Random();

            // Initialize pipes with images
            for (int i = 0; i < pipes.Length; i++)
            {
                pipes[i] = new PictureBox
                {
                    Width = 60,
                    Height = 350,  // Increased height for better coverage
                    Image = pipeImage,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Tag = "pipe"
                };

                // Create pipe end
                pipeEnds[i] = new PictureBox
                {
                    Width = 80,  // Make end slightly wider
                    Height = 40, // Height for the end piece
                    Image = pipeEndImage,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Tag = "pipe_end"
                };

                ResetPipePosition(i);
                
                this.Controls.Add(pipes[i]);
                this.Controls.Add(pipeEnds[i]);
            }

            // Create a fresh timer instance with exact interval
            gameTimer = new System.Windows.Forms.Timer
            {
                Interval = 20,
                Enabled = false // Don't start until everything is ready
            };
            gameTimer.Tick += GameTimer_Tick;

            // Resubscribe events
            this.KeyDown += MainForm_KeyDown;
            this.Paint += MainForm_Paint;

            // Start timers only after everything is initialized
            gameTimer.Start();
            glowTimer?.Start();
        }

        private void DisposePreviousImages()
        {
            // Clear any existing image references
            if (pipes != null)
            {
                foreach (var pipe in pipes)
                {
                    pipe?.Image?.Dispose();
                }
            }
            if (pipeEnds != null)
            {
                foreach (var end in pipeEnds)
                {
                    end?.Image?.Dispose();
                }
            }

            birdImage?.Dispose();
            pipeImage?.Dispose();
            pipeEndImage?.Dispose();
            backgroundImage?.Dispose();

            birdImage = null!;
            pipeImage = null!;
            pipeEndImage = null!;
            backgroundImage = null!;
        }

        // New method to properly dispose controls
        private void DisposePreviousControls()
        {
            if (pipes != null)
            {
                foreach (var pipe in pipes)
                {
                    if (pipe != null)
                    {
                        pipe.Image = null; // Clear image before disposal
                        this.Controls.Remove(pipe);
                        pipe.Dispose();
                    }
                }
            }

            if (pipeEnds != null)
            {
                foreach (var end in pipeEnds)
                {
                    if (end != null)
                    {
                        end.Image = null; // Clear image before disposal
                        this.Controls.Remove(end);
                        end.Dispose();
                    }
                }
            }

            pipes = null!;
            pipeEnds = null!;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // Set form properties
            this.Text = "Flappy Bird";
            this.BackColor = Color.SkyBlue;
            this.DoubleBuffered = true;
            this.ClientSize = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void MainForm_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            
            // Draw background
            e.Graphics.DrawImage(backgroundImage, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            
            // Draw bird with rotation based on velocity
            using (Matrix matrix = new Matrix())
            {
                matrix.RotateAt((float)(bird.Velocity * 5), new PointF(bird.X + bird.Size/2, bird.Y + bird.Size/2));
                e.Graphics.Transform = matrix;
                e.Graphics.DrawImage(birdImage, bird.X, bird.Y, bird.Size, bird.Size);
                e.Graphics.ResetTransform();
            }

            // Draw pipes manually instead of using PictureBoxes
            if (!isGameOver)
            {
                foreach (var pipe in pipes)
                {
                    e.Graphics.DrawImage(pipeImage, pipe.Bounds);
                }
                foreach (var pipeEnd in pipeEnds)
                {
                    e.Graphics.DrawImage(pipeEndImage, pipeEnd.Bounds);
                }
            }

            // Draw enhanced score with glow effect (on top of everything)
            var scoreText = $"Score: {score}";
            if (currentSpeedMultiplier > 1.0f)
            {
                scoreText += $" (Speed: x{currentSpeedMultiplier:F1})";
            }
            var scoreBounds = new RectangleF(10, 10, 350, 60);
            
            using (var path = new GraphicsPath())
            {
                path.AddString(scoreText, scoreFont.FontFamily, (int)scoreFont.Style, scoreFont.Size,
                    scoreBounds, StringFormat.GenericDefault);

                // Outer glow
                using (var glow = new Pen(Color.FromArgb((int)(100 * glowIntensity), scoreGlow), 8))
                {
                    e.Graphics.DrawPath(glow, path);
                }

                // Inner glow
                using (var glow = new Pen(Color.FromArgb((int)(180 * glowIntensity), scoreGlow), 4))
                {
                    e.Graphics.DrawPath(glow, path);
                }

                // Main text
                using (var brush = new LinearGradientBrush(scoreBounds, 
                    scoreColor, Color.FromArgb(255, 255, 200), 45))
                {
                    e.Graphics.FillPath(brush, path);
                }
            }

            // ...existing game over screen code...

            if (isGameOver)
            {
                // Full-screen semi-transparent overlay
                using (var overlay = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(overlay, 0, 0, this.Width, this.Height);
                }

                var centerY = this.ClientSize.Height / 2;

                // High score display (drawn first)
                var highScoreText = $"High Score: {Math.Max(score, 0)}";
                var highScoreBounds = new RectangleF(0, centerY - 180, this.ClientSize.Width, 40);
                
                using (var highScoreBrush = new LinearGradientBrush(highScoreBounds,
                    Color.FromArgb(255, 215, 0), Color.FromArgb(255, 255, 200), 45))
                {
                    e.Graphics.DrawString(highScoreText, highScoreFont, highScoreBrush,
                        highScoreBounds, centerFormat);
                }

                // Game Over text with enhanced effects
                var gameOverText = "GAME OVER";
                var gameOverBounds = new RectangleF(0, centerY - 100, this.ClientSize.Width, 100);

                using (var path = new GraphicsPath())
                {
                    path.AddString(gameOverText, gameOverFont.FontFamily, (int)gameOverFont.Style,
                        gameOverFont.Size, gameOverBounds, centerFormat);

                    // Dramatic outer glow
                    using (var glow = new Pen(Color.FromArgb((int)(100 * glowIntensity), gameOverGlow), 12))
                    {
                        e.Graphics.DrawPath(glow, path);
                    }

                    // Main text with gradient
                    using (var brush = new LinearGradientBrush(gameOverBounds,
                        Color.White, Color.FromArgb(220, 20, 60), 90))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                }

                // Restart text with animation
                var restartText = "Press R to Restart";
                var restartBounds = new RectangleF(0, centerY + 40, this.ClientSize.Width, 40);
                
                using (var restartBrush = new SolidBrush(
                    Color.FromArgb((int)(200 + 55 * glowIntensity), 255, 255, 255)))
                {
                    e.Graphics.DrawString(restartText, restartFont, restartBrush, 
                        restartBounds, centerFormat);
                }
            }
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && !isGameOver)
            {
                bird.Jump();
            }
            else if (e.KeyCode == Keys.R && isGameOver)
            {
                ResetGame();
            }
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (isGameOver || isResetting) return;

            // Ensure consistent timing
            if (gameTimer.Interval != 20)
                gameTimer.Interval = 20;

            bird.Update();

            // Calculate current speed based on score (every 5 points)
            float speedMultiplier = Math.Min(
                1.0f + (score / 5) * SPEED_INCREASE_PER_5_POINTS, 
                MAX_SPEED_MULTIPLIER
            );
            currentSpeedMultiplier = speedMultiplier;
            int currentSpeed = (int)(BASE_PIPE_SPEED * speedMultiplier);

            bool needsReset = false;
            int resetIndex = -1;

            // Move pipes and check for scoring/reset
            for (int i = 0; i < pipes.Length; i++)
            {
                pipes[i].Left -= currentSpeed;
                pipeEnds[i].Left = pipes[i].Left - 10;

                // Check for pipe reset
                if (pipes[i].Right < 0)
                {
                    needsReset = true;
                    resetIndex = i;
                }

                // Check for scoring (only check even-indexed pipes - top pipes)
                if (i % 2 == 0 && // Only check top pipes
                    i != lastScoredPipeIndex && // Haven't scored this pipe yet
                    bird.X > pipes[i].Right) // Bird has passed the pipe
                {
                    score++;
                    lastScoredPipeIndex = i;
                }

                // Collision detection
                if (bird.Bounds.IntersectsWith(pipes[i].Bounds) || 
                    bird.Bounds.IntersectsWith(pipeEnds[i].Bounds))
                {
                    GameOver();
                    return;
                }
            }

            // Reset pipes if needed
            if (needsReset && resetIndex >= 0)
            {
                int pairIndex = (resetIndex / 2) * 2;
                ResetPipePair(pairIndex);
            }

            // Check if bird is out of bounds
            if (bird.Y < 0 || bird.Y > this.ClientSize.Height)
            {
                GameOver();
            }

            this.Invalidate();
        }

        private void ResetGame()
        {
            isResetting = true;
            
            // Stop and cleanup timers first
            gameTimer?.Stop();
            glowTimer?.Stop();

            // Clear control states
            foreach (var pipe in pipes)
            {
                if (pipe != null)
                {
                    pipe.Image = null;
                    pipe.Visible = false;
                }
            }
            foreach (var end in pipeEnds)
            {
                if (end != null)
                {
                    end.Image = null;
                    end.Visible = false;
                }
            }

            // Use BeginInvoke with complete cleanup
            this.BeginInvoke(new Action(() => {
                DisposePreviousControls();
                DisposePreviousImages();
                GC.Collect(); // Force garbage collection
                InitializeGame();
                this.Invalidate();
            }));
        }

        private void DisposeResources()
        {
            scoreFont?.Dispose();
            gameOverFont?.Dispose();
            restartFont?.Dispose();
            highScoreFont?.Dispose();
            centerFormat?.Dispose();
            glowTimer?.Dispose();
            DisposePreviousImages();
            DisposePreviousControls();
            gameTimer?.Dispose();
        }

        // Add this new method to handle pipe positioning
        private void ResetPipePosition(int index)
        {
            int centerY = this.ClientSize.Height / 2;
            int gapCenter = centerY + random.Next(-MAX_OFFSET, MAX_OFFSET);

            if (index % 2 == 0)
            {
                // Top pipe
                pipes[index].Top = gapCenter - (PIPE_GAP / 2) - pipes[index].Height;
                pipeEnds[index].Top = pipes[index].Bottom - 20;
            }
            else
            {
                // Bottom pipe
                pipes[index].Top = gapCenter + (PIPE_GAP / 2);
                pipeEnds[index].Top = pipes[index].Top - 20;
            }

            // Ensure pipes are properly spaced horizontally
            int pairIndex = index / 2;
            int baseX = this.ClientSize.Width + (pairIndex * PIPE_SPACING);
            pipes[index].Left = baseX;
            pipeEnds[index].Left = baseX - 10;
        }

        private void GameOver()
        {
            isGameOver = true;
            gameTimer?.Stop();
            
            // Just hide the pipes without modifying their images
            foreach (var pipe in pipes)
            {
                if (pipe != null)
                {
                    pipe.Visible = false;
                }
            }
            foreach (var pipeEnd in pipeEnds)
            {
                if (pipeEnd != null)
                {
                    pipeEnd.Visible = false;
                }
            }
        }

        private void ResetPipePair(int topPipeIndex)
        {
            if (pipes == null) return;
            
            // Find the furthest pipe pair's X position
            int maxX = pipes.Max(p => p?.Right ?? 0);
            
            // Position new pipe pair after the furthest pipe
            int newX = maxX + PIPE_SPACING;
            
            // Reset both pipes in the pair
            int centerY = this.ClientSize.Height / 2;
            int gapCenter = centerY + random.Next(-MAX_OFFSET, MAX_OFFSET);

            // Reset top pipe
            if (pipes[topPipeIndex] != null)
            {
                pipes[topPipeIndex].Left = newX;
                pipes[topPipeIndex].Top = gapCenter - (PIPE_GAP / 2) - pipes[topPipeIndex].Height;
                pipes[topPipeIndex].Tag = "pipe"; // Reset scored state
            }

            if (pipeEnds[topPipeIndex] != null)
            {
                pipeEnds[topPipeIndex].Left = newX - 10;
                pipeEnds[topPipeIndex].Top = pipes[topPipeIndex]?.Bottom - 20 ?? 0;
            }

            // Reset bottom pipe
            if (pipes[topPipeIndex + 1] != null)
            {
                pipes[topPipeIndex + 1].Left = newX;
                pipes[topPipeIndex + 1].Top = gapCenter + (PIPE_GAP / 2);
                pipes[topPipeIndex + 1].Tag = "pipe"; // Reset scored state
            }

            if (pipeEnds[topPipeIndex + 1] != null)
            {
                pipeEnds[topPipeIndex + 1].Left = newX - 10;
                pipeEnds[topPipeIndex + 1].Top = pipes[topPipeIndex + 1]?.Top - 20 ?? 0;
            }
        }
    }
}
