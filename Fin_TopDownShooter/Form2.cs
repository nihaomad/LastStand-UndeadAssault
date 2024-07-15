
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WMPLib;

namespace Fin_TopDownShooter
{
    public partial class Form2 : Form
    {
        bool goup;
        bool godown;
        bool goleft;
        bool goright;
        string facing = "up";
        double playerHealth;
        int speed = 10;
        int ammo;
        int zombieSpeed = 3;
        int score;
        bool gameOver;
        bool gamePaused; // Flag to track game pause state

        Random rnd = new Random();
        private WindowsMediaPlayer gunshotSound = new WindowsMediaPlayer();
        private WindowsMediaPlayer reloadingSound = new WindowsMediaPlayer();

        // Keep track of initial player position for reset
        private Point initialPlayerPosition;

        // Controls panel
        private Panel panelControls;

        public Form2()
        {
            InitializeComponent();
            InitializeSounds();
            InitializeGame();
            InitializeControlsPanel(); // Initialize the controls panel
        }

        private void InitializeSounds()
        {
            gunshotSound.URL = @"C:\Users\63929\source\repos\Fin_TopDownShooter\Fin_TopDownShooter\gun-shot-1-176892.mp3";
            reloadingSound.URL = @"C:\Users\63929\source\repos\Fin_TopDownShooter\Fin_TopDownShooter\load-gun-sound-effect-5-11003.mp3";
        }

        private void InitializeGame()
        {
            gameOver = false;
            gamePaused = false;
            score = 0;
            playerHealth = 100;
            ammo = 10;
            player.Image = Properties.Resources.pUp1;

            // Store initial player position for reset
            initialPlayerPosition = new Point(ClientSize.Width / 2, ClientSize.Height / 2);
            player.Location = initialPlayerPosition;

            progressBar1.Value = Convert.ToInt32(playerHealth);
            label1.Text = "   Ammo:  " + ammo;
            label2.Text = "Kills: " + score;
            progressBar1.ForeColor = Color.Green;
            timer1.Start();
        }

        private void keyisdown(object sender, KeyEventArgs e)
        {
            if (gameOver) return;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    goleft = true;
                    facing = "left";
                    player.Image = Properties.Resources.pRight1;
                    break;
                case Keys.Right:
                    goright = true;
                    facing = "right";
                    player.Image = Properties.Resources.pLeft1;
                    break;
                case Keys.Up:
                    goup = true;
                    facing = "up";
                    player.Image = Properties.Resources.pUp1;
                    break;
                case Keys.Down:
                    godown = true;
                    facing = "down";
                    player.Image = Properties.Resources.pdown1;
                    break;
                case Keys.Escape:
                    TogglePauseGame();
                    break;
            }
        }

        private void keyisup(object sender, KeyEventArgs e)
        {
            if (gameOver) return;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    goleft = false;
                    break;
                case Keys.Right:
                    goright = false;
                    break;
                case Keys.Up:
                    goup = false;
                    break;
                case Keys.Down:
                    godown = false;
                    break;
                case Keys.Space:
                    if (ammo > 0)
                    {
                        ammo--;
                        shoot(facing);
                        gunshotSound.controls.play();
                        if (ammo < 1)
                        {
                            DropAmmo();
                        }
                        else if (ammo <= 2) // spawn ammo when ammo is 2 or less
                        {
                            DropAmmo();
                        }
                    }
                    break;
            }
        }

        private void gameEngine(object sender, EventArgs e)
        {
            if (!gameOver && !gamePaused) // Only update game state if not paused
            {
                if (playerHealth > 1)
                {
                    progressBar1.Value = Convert.ToInt32(playerHealth);
                }
                else
                {
                    player.Image = Properties.Resources.dead;
                    timer1.Stop();
                    gameOver = true;
                    ShowGameOverMessage();
                }

                label1.Text = "   Ammo:  " + ammo;
                label2.Text = "Kills: " + score;

                if (playerHealth < 20)
                {
                    progressBar1.ForeColor = Color.Red;
                }

                MovePlayer();
                UpdateZombies();
                CheckCollisions(); // Ensure CheckCollisions is called in gameEngine
                CheckHealingItemSpawn(); // Check for healing item spawn periodically
            }
        }


        private void MovePlayer()
        {
            if (goleft && player.Left > 0)
            {
                player.Left -= speed;
            }
            if (goright && player.Left + player.Width < 930)
            {
                player.Left += speed;
            }
            if (goup && player.Top > 60)
            {
                player.Top -= speed;
            }
            if (godown && player.Top + player.Height < 700)
            {
                player.Top += speed;
            }
        }

        private void UpdateZombies()
        {
            foreach (Control x in Controls)
            {
                if (x is PictureBox && x.Tag == "zombie")
                {
                    var zombie = (PictureBox)x;
                    if (zombie.Bounds.IntersectsWith(player.Bounds))
                    {
                        playerHealth -= 1;
                        player.BringToFront();
                    }
                    MoveZombie(zombie);
                }
            }
        }

        private void MoveZombie(PictureBox zombie)
        {
            if (zombie.Left > player.Left)
            {
                zombie.Left -= zombieSpeed;
                zombie.Image = Properties.Resources.zleft;
            }
            if (zombie.Top > player.Top)
            {
                zombie.Top -= zombieSpeed;
                zombie.Image = Properties.Resources.zup;
            }
            if (zombie.Left < player.Left)
            {
                zombie.Left += zombieSpeed;
                zombie.Image = Properties.Resources.zright;
            }
            if (zombie.Top < player.Top)
            {
                zombie.Top += zombieSpeed;
                zombie.Image = Properties.Resources.zdown;
            }
        }

        private void CheckCollisions()
        {
            foreach (Control x in Controls)
            {
                if (x is PictureBox && x.Tag == "ammo")
                {
                    if (x.Bounds.IntersectsWith(player.Bounds))
                    {
                        Controls.Remove(x);
                        x.Dispose();
                        ammo += 5;
                    }
                }
                if (x is PictureBox && x.Tag == "bullet")
                {
                    if (x.Left < 1 || x.Left > 930 || x.Top < 10 || x.Top > 700)
                    {
                        Controls.Remove(x);
                        x.Dispose();
                    }
                }
                if (x is PictureBox && x.Tag == "zombie")
                {
                    if (x.Bounds.IntersectsWith(player.Bounds))
                    {
                        playerHealth -= 0.2;
                        player.BringToFront();
                    }
                    foreach (Control j in Controls)
                    {
                        if (j is PictureBox && j.Tag == "bullet" && x.Bounds.IntersectsWith(j.Bounds))
                        {
                            score++;
                            Controls.Remove(j);
                            j.Dispose();
                            Controls.Remove(x);
                            x.Dispose();
                            makeZombies();
                            CheckHealingItemSpawn(); // Check for healing item spawn after killing zombies
                        }
                    }
                }
                if (x is PictureBox && x.Tag == "heal_item")
                {
                    if (x.Bounds.IntersectsWith(player.Bounds))
                    {
                        playerHealth += 6; // Increase player health
                        Console.WriteLine($"Player Health: {playerHealth}"); // Debug message
                        progressBar1.Value = Convert.ToInt32(playerHealth); // Update progress bar
                        Controls.Remove(x);
                        x.Dispose();
                        break;
                    }
                }
            }
        }


        private void DropAmmo()
        {
            if (ammo <=3)
            {
                PictureBox ammoBox = new PictureBox
            {
                Image = Properties.Resources.loot_Image,
                SizeMode = PictureBoxSizeMode.AutoSize,
                Left = rnd.Next(10, 890),
                Top = rnd.Next(50, 600),
                Tag = "ammo"
            };
            Controls.Add(ammoBox);
            ammoBox.BringToFront();
            player.BringToFront();
            reloadingSound.controls.play();
            }
        }

        private void shoot(string direction)
        {
            bullet shoot = new bullet
            {
                direction = direction,
                bulletLeft = player.Left + (player.Width / 2),
                bulletTop = player.Top + (player.Height / 2)
            };
            shoot.mkBullet(this);
        }

        private void makeZombies()
        {
            // Check current number of zombies
            int zombieCount = Controls.OfType<PictureBox>().Count(pb => pb.Tag?.ToString() == "zombie");

            // Spawn new zombie only if there are less than 4 zombies
            if (zombieCount < 4)
            {
                PictureBox zombie = new PictureBox
                {
                    Tag = "zombie",
                    Image = Properties.Resources.zdown,
                    Left = rnd.Next(0, 900),
                    Top = rnd.Next(0, 800),
                    SizeMode = PictureBoxSizeMode.AutoSize
                };
                Controls.Add(zombie);
                zombie.BringToFront();
            }
        }

        private void ShowGameOverMessage()
        {
            string message = $"You killed: {score} zombies!\n\nThere's no such thing as a second life in real life, and so in this game!";
            MessageBox.Show(message, "Game Over", MessageBoxButtons.OK, MessageBoxIcon.None);

            // Redirect to the main menu or restart the game
            // For redirecting to the main menu, assuming you have a Form1 for the main menu:
            Shooter mainMenu = new Shooter();
            mainMenu.Show();
            this.Hide(); // Hide the current game form
        }

        private void InitializeControlsPanel()
        {
            panelControls = new Panel();
            panelControls.BackColor = Color.Black;
            panelControls.Size = new Size(300, 400);
            panelControls.Location = new Point((ClientSize.Width - panelControls.Width) / 2, (ClientSize.Height - panelControls.Height) / 2);

            Label labelControls = new Label();
            labelControls.Text = "Controls and Mechanics";
            labelControls.ForeColor = Color.White;
            labelControls.Font = new Font("Arial", 14, FontStyle.Bold);
            labelControls.Location = new Point(50, 50);
            labelControls.AutoSize = true;
            panelControls.Controls.Add(labelControls);

            TextBox textBoxInfo = new TextBox();
            textBoxInfo.Text = "Use arrow keys to move.\nPress Space to shoot.\nPress ESC to toggle this panel.";
            textBoxInfo.ForeColor = Color.White;
            textBoxInfo.BackColor = Color.Black;
            textBoxInfo.Multiline = true;
            textBoxInfo.ReadOnly = true;
            textBoxInfo.BorderStyle = BorderStyle.None;
            textBoxInfo.Location = new Point(50, 100);
            textBoxInfo.Size = new Size(200, 200);
            panelControls.Controls.Add(textBoxInfo);

            Controls.Add(panelControls);
            panelControls.Visible = false; // Start with panel hidden
        }

        private void TogglePauseGame()
        {
            gamePaused = !gamePaused;
            panelControls.Visible = gamePaused;

            // Optionally pause game logic or timer here
            if (gamePaused)
            {
                timer1.Stop();
            }
            else
            {
                timer1.Start();
            }
            panelControls.BringToFront(); // Ensure panel is in front when shown
        }

        private bool healingItemSpawned = false; // Flag to track if healing item has been spawned

        private void CheckHealingItemSpawn()
        {
            // Check if score is a multiple of 10 and healing item hasn't been spawned yet
            if (score % 10 == 0 && score > 0 && !healingItemSpawned)
            {
                // Spawn a healing item
                PictureBox healingItem = new PictureBox
                {
                    Image = Properties.Resources.heal_item,
                    SizeMode = PictureBoxSizeMode.AutoSize,
                    Left = rnd.Next(10, 890),
                    Top = rnd.Next(50, 600),
                    Tag = "heal_item" // Corrected tag to match with collision check
                };
                Controls.Add(healingItem);
                healingItem.BringToFront();

                healingItemSpawned = true; // Set flag to true indicating item has been spawned
            }
        }
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop any ongoing actions (e.g., timer)
            timer1.Stop();

            // Dispose resources like sounds
            gunshotSound.close();
            reloadingSound.close();

            Application.Exit(); // Exit the application
        }

    }
}
