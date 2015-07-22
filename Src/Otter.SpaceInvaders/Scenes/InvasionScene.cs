using System;
using System.Collections.Generic;
using System.Linq;
using GoodStuff.NaturalLanguage;
using Otter.SpaceInvaders.Entities;
using Otter.SpaceInvaders.Entities.Abstract;

namespace Otter.SpaceInvaders.Scenes
{
    public class InvasionScene : Scene
    {
        private const string FontPath = @".\Assets\Fonts\PressStart2P.TTF";

        private const int ScoreTextSize = 40;

        private const int LivesTextSize = 40;

        private const int Rows = 5;

        private const int Columns = 11;

        private const float VerticalOffset = 150;

        private const float HorizontalOffset = 100;

        private const int DistanceBetweenInvaders = 125;

        private const int BaseMoveTimeout =  100;

        private const float InvaderSpeed = 20;

        private int _currentTime = 0;

        private readonly List<InvaderBase> _invaders = new List<InvaderBase>();

        private readonly List<MissileEntity> _alienMissiles = new List<MissileEntity>(); 

        private AlienDirection _alienDirection = AlienDirection.Right;

        private readonly Text _scoreText;

        private readonly Text _livesText;

        private readonly Music _backgroundMusic = new Music(@".\Assets\Audio\S31-Let the Games Begin.ogg");

        public readonly HeroEntity Hero = new HeroEntity();

        public InvasionScene()
        {
            _scoreText = new Text("", FontPath, ScoreTextSize);
            _livesText = new Text("", FontPath, LivesTextSize);

            var backgroundImage = Image.CreateRectangle(Game.Instance.Width, Game.Instance.Height, new Color("1B3AA9"));

            AddGraphic(backgroundImage);

            Core.PlayerLives = 3;

            Add(Hero);
            SetUpInvaders();

            _scoreText.Y = 50;
            _livesText.Y = 50;
        }

        public override void Begin()
        {
            base.Begin();

            StartMusic();
        }

        private void SetUpInvaders()
        {
            for (var col = 0; col < Columns; col++)
            {
                for (var row = 0; row < Rows; row++)
                {
                    var xPos = col * DistanceBetweenInvaders + HorizontalOffset;
                    var yPos = row * 100 + VerticalOffset;


                    InvaderBase invader;
                    switch (row)
                    {
                        case 0:
                            invader = new LargeInvader(xPos, yPos);
                            break;
                        case 2:
                        case 1:
                            invader = new MediumInvader(xPos, yPos);
                            break;
                        default:
                            invader = new SmallInvader(xPos, yPos);
                            break;
                    }
                    
                    _invaders.Add(invader);
                }
            }

            foreach (var invader in _invaders)
            {
                Add(invader);
            }
        }

        public MissileEntity PlayerMissile;

        public override void Update()
        {
            base.Update();

            if (_invaders.Any() && !Hero.IsDying)
            {
                Move();
            }
            else if (_invaders.IsEmpty())
            {
                ClearMissiles();
                SetUpInvaders();
            }

            DisplayScore();
            DisplayLives();

            if (InvadersReachedTheBottom() && !Hero.IsDying)
            {
                Core.PlayerLives = 0;
                Hero.Die();
            }

            RemoveDeadMissiles();
        }

        private bool InvadersReachedTheBottom()
        {
            if (_invaders.Any())
            {
                var lowestPosition = _invaders.Max(invader => invader.Y);

                if (lowestPosition > 850)
                {
                    return true;
                }
            }

            return false;
        }

        private void DisplayLives()
        {
            var lives = Core.PlayerLives;

            if (lives < 0)
            {
                _livesText.String = "Lives: 0";
            }
            else
            {
                _livesText.String = Core.PlayerLives.ToString("Lives: 0");
            }
            
            _livesText.CenterOrigin();
            _livesText.X = Game.Instance.Width - (_livesText.HalfWidth + 50);
        }

        private void DisplayScore()
        {
            _scoreText.String = Core.PlayerScore.ToString("Score: 000000");
            _scoreText.CenterOrigin();
            _scoreText.X = _scoreText.HalfWidth + 50;
        }

        private void Move()
        {
            if (ShouldMove())
            {
                switch (_alienDirection)
                {
                    case AlienDirection.Right:
                        MoveRight();
                        break;
                    case AlienDirection.Left:
                        MoveLeft();
                        break;
                }
            }
        }

        private bool ShouldMove()
        {
             _currentTime++;

            var deadInvaders = GetDeadInvaders();
            var speedEffect = (.03 * Math.Pow(deadInvaders, 2));

            if (_currentTime >= BaseMoveTimeout - speedEffect)
            {
                _currentTime = 0;
                return true;
            }

            _currentTime++;
            return false;
        }

        private int GetDeadInvaders()
        {
            const int totalInvaders = Rows * Columns;
            var deadInvaders = totalInvaders - _invaders.Count;
            return deadInvaders;
        }

        private void MoveRight()
        {
            var farthestInvader = _invaders.Max(invader => invader.X);
            var maxRight = Game.Instance.Width - HorizontalOffset;
            if (farthestInvader > maxRight)
            {
                _invaders.Each(invader => invader.Y += InvaderSpeed);
                _alienDirection = AlienDirection.Left;
            }
            else
            {
                _invaders.Each(invader => invader.X += InvaderSpeed);
            }
        }

        private void MoveLeft()
        {
            var farthestInvader = _invaders.Min(invader => invader.X);
            const float maxLeft = HorizontalOffset;
            if (farthestInvader < maxLeft)
            {
                _invaders.Each(invader => invader.Y += InvaderSpeed);
                _alienDirection = AlienDirection.Right;
            }
            else
            {
                _invaders.Each(invader => invader.X -= InvaderSpeed);
            }
        }

        public override void Render()
        {
            base.Render();

            Draw.Graphic(_scoreText);
            Draw.Graphic(_livesText);
        }

        public void AddAlienMissile(MissileEntity missile)
        {
            _alienMissiles.Add(missile);
            Add(missile);
        }

        public void KillInvader(InvaderBase invader)
        {
            _invaders.Remove(invader);
            Remove(invader);
        }

        public void ClearMissiles()
        {
            foreach (var missile in _alienMissiles)
            {
                Remove(missile);
            }

            _alienMissiles.Clear();
        }

        public void RemoveDeadMissiles()
        {
            var missiles = _alienMissiles.Where(OutOfBounds).ToList();

            foreach (var missile in missiles)
            {
                Remove(missile);
                _alienMissiles.Remove(missile);
            }

            if (PlayerMissile != null && OutOfBounds(PlayerMissile))
            {
                RemovePlayerMissile();
            }
        }

        public void FirePlayerMissile(MissileEntity playerMissile)
        {
            PlayerMissile = playerMissile;
            Add(playerMissile);
        }

        public void RemovePlayerMissile()
        {
            Remove(PlayerMissile);
            PlayerMissile = null;
        }

        public bool CanFirePlayerMissile()
        {
            return PlayerMissile == null;
        }

        public bool OutOfBounds(MissileEntity missile)
        {
            var outOfBounds = missile.Y < 0 || missile.Y > Game.Instance.Height;
            return outOfBounds;
        }

        private enum AlienDirection
        {
            Left,
            Right
        }

        public void RemoveAlienMissile(MissileEntity missileEntity)
        {
            _alienMissiles.Remove(missileEntity);
            Remove(missileEntity);
        }

        public override void End()
        {
            base.End();

            StopMusic();
        }

        public void StartMusic()
        {
            _backgroundMusic.Play();
        }

        public void StopMusic()
        {
            _backgroundMusic.Stop();
        }
    }
}