﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AirForce.AirObjects;
using AirForce.Enums;

namespace AirForce
{
    public sealed class GameController
    {
        private GameState gameState = GameState.Play;

        private readonly Size gameFieldSize;
        private readonly Line groundLine;

        private readonly PlayerShip playerShip;
        private readonly Point2D playerShipStartPosition;

        private readonly Timer enemyCreatorTimer = new Timer();
        private readonly Timer objectMovingTimer = new Timer();

        private readonly Random random = new Random();

        private readonly List<AirObject> airObjects = new List<AirObject>();

        /// -------------------------------------------------------

        public GameController(Size gameFieldSize)
        {
            this.gameFieldSize = gameFieldSize;

            groundLine = new Line(
                new Point2D(0, gameFieldSize.Height - 30),
                new Point2D(gameFieldSize.Width, gameFieldSize.Height - 30));

            playerShipStartPosition = new Point2D
            {
                X = 150,
                Y = groundLine.FirstPoint.Y / 2
            };

            playerShip = new PlayerShip(playerShipStartPosition, 30, 6);

            // enemyCreatorTimer setting
            enemyCreatorTimer.Interval = 1000; // 1500
            enemyCreatorTimer.Tick += (s, e) => AddNewRandomEnemy();
            enemyCreatorTimer.Start();

            // objectMovingTimer setting
            objectMovingTimer.Interval = 1;
            objectMovingTimer.Tick += (s, e) => Update();
            objectMovingTimer.Start();         
        }

        public void TryMovePlayer(Point2D movespeedModifer)
        {
            if (gameState == GameState.Wait)
                return;

            playerShip.Move(movespeedModifer, gameFieldSize, groundLine);
        }

        public void TryCreatePlayerBullet()
        {
            if (gameState == GameState.Wait)
            {
                Restart();
                return;
            }

            Point2D bulletStartPosition = new Point2D
            {
                X = playerShip.Position.X + playerShip.Radius,
                Y = playerShip.Position.Y
            };

            airObjects.Add(new PlayerBullet(bulletStartPosition, 8, 8));
        }

        private void Update()
        {
            // adding new enemy bullets
            List<EnemyBullet> newEnemyBullets = airObjects
                .OfType<ChaserShip>()
                .Where(x => x.IsShoting)
                .Select(x => new EnemyBullet(new Point2D(x.Position.X - x.Radius, x.Position.Y), 8, 8))
                .ToList();

            airObjects.AddRange(newEnemyBullets);
            airObjects.ForEach(a => a.Move(gameFieldSize, groundLine, airObjects));
            FindAirObjectsAllCollisions();
        }

        private void Restart()
        {
            airObjects.Clear();
            playerShip.Refresh(playerShipStartPosition, 100);
            gameState = GameState.Play;
        }

        private void FindAirObjectsAllCollisions()
        {
            for (int i = 0; i < airObjects.Count; i++)
            {
                for (int j = i + 1; j < airObjects.Count; j++)
                    if (IsAirObjectsHaveCollision(airObjects[i], airObjects[j]))
                    {
                        airObjects[i].CollisionWithOtherAirObject(airObjects[j]);
                        airObjects[j].CollisionWithOtherAirObject(airObjects[i]);
                    }

                if (IsAirObjectsHaveCollision(playerShip, airObjects[i]))
                {
                    playerShip.CollisionWithOtherAirObject(airObjects[i]);
                    airObjects[i].CollisionWithOtherAirObject(playerShip);
                }
            }

            airObjects.RemoveAll(a => a.Durability <= 0);

            if (playerShip.Durability <= 0)
            {
                gameState = GameState.Wait;
                playerShip.Refresh(new Point2D(-200, -200), 0);
            }
        }

        private bool IsAirObjectsHaveCollision(AirObject airObject1, AirObject airObject2)
        {
            return Math.Pow(airObject1.Radius + airObject2.Radius, 2) >
                   Math.Pow(airObject1.Position.X - airObject2.Position.X, 2)
                   + Math.Pow(airObject1.Position.Y - airObject2.Position.Y, 2);
        }

        private void AddNewRandomEnemy()
        {
            Point2D startPosition;
            int radius;
            int movespeedShift;

            int randomNumber = random.Next(0, 4);
            
            switch (randomNumber)
            {
                case 0:
                    radius = 50;
                    movespeedShift = 8;
                    startPosition = new Point2D
                    {
                        X = gameFieldSize.Width + radius,
                        Y = random.Next(radius, groundLine.FirstPoint.Y - radius)
                    };

                    airObjects.Add(new BigShip(startPosition, radius, movespeedShift));
                    break;

                case 1:
                    radius = 30;
                    movespeedShift = 3;
                    startPosition = new Point2D
                    {
                        X = gameFieldSize.Width + radius,
                        Y = random.Next(radius, groundLine.FirstPoint.Y - radius)
                    };

                    airObjects.Add(new ChaserShip(startPosition, radius, movespeedShift, playerShip));
                    break;

                case 2:
                    radius = 15;
                    movespeedShift = 2;
                    startPosition = new Point2D
                    {
                        X = gameFieldSize.Width + radius,
                        Y = random.Next(groundLine.FirstPoint.Y - 10 * radius, groundLine.FirstPoint.Y - radius)
                    };

                    airObjects.Add(new Bird(startPosition, radius, movespeedShift));
                    break;

                case 3:
                    radius = 70;
                    movespeedShift = 2;
                    startPosition = new Point2D
                    {
                        X = random.Next(0, gameFieldSize.Width),
                        Y = 0
                    };

                    airObjects.Add(new Meteor(startPosition, radius, movespeedShift));
                    break;
            }
        }

        #region drawingMethods

        public void DrawAllElements(Graphics graphics)
        {
            airObjects.ForEach(o => o.Draw(graphics));

            playerShip.Draw(graphics);

            DrawGround(graphics);

            DrawPlayerDurabulity(graphics);

            if (gameState == GameState.Wait)
                DrawWaitingStateString(graphics);
        }

        private void DrawGround(Graphics graphics)
        {
            Brush groundBrush = Brushes.Green;
            Rectangle groundRectangle = new Rectangle(groundLine.FirstPoint, gameFieldSize);

            graphics.FillRectangle(groundBrush, groundRectangle);
        }

        private void DrawWaitingStateString(Graphics graphics)
        {
            string message = "Press SPACE to start game";

            Font font = new Font("Segoe UI", 12, FontStyle.Bold);
            Brush brush = Brushes.White;
            Rectangle gameFieldRectangle = new Rectangle(new Point(), gameFieldSize);

            StringFormat stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(message, font, brush, gameFieldRectangle, stringFormat);
        }

        private void DrawPlayerDurabulity(Graphics graphics)
        {
            Image image = Properties.Resources.heart;
            Rectangle imageRectangle = new Rectangle
            {
                Location = new Point2D(0, 0),
                Size = new Size(20, 20)
            };

            graphics.DrawImage(image, imageRectangle);

            Font font = new Font("Segoe UI", 15, FontStyle.Bold);
            Brush brush = Brushes.White;
            string message = " x " + playerShip.Durability;

            graphics.DrawString(message, font, brush, new PointF(18, -5));
        }

        #endregion drawingMethods
    }
}
