﻿using System;
using System.Drawing;
using AirForce.Enums;

namespace AirForce.AirObjects
{
    public sealed class PlayerShip : AirObject
    {
        public PlayerShip(Size spaceSize, Action<AirObject> deathPlayerShipMethod)
            : base(new Point(30, spaceSize.Height / 2), 30, 5, 15, Properties.Resources.player_ship, deathPlayerShipMethod)
        {
        }

        public override void CollisionWithOtherAirObject(AirObject otherAirObject)
        {
            Strength--;

            if (Strength == 0)
                OnDeathObjectEvent();
        }

        public void Move(Direction direction, Size spaceSize, Line groundLine)
        {
            Point nextPositionInSpace;

            switch (direction)
            {
                case Direction.Up:
                    nextPositionInSpace = new Point(PositionInSpace.X, PositionInSpace.Y - MovespeedShift);
                    break;
                case Direction.Down:
                    nextPositionInSpace = new Point(PositionInSpace.X, PositionInSpace.Y + MovespeedShift);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            if (IsNextPositionAreBeingInSpace(nextPositionInSpace, spaceSize))
                PositionInSpace = nextPositionInSpace;

            if (!IsAboveGroundLine(groundLine))
                OnDeathObjectEvent();
        }

        public void Shoot()
        {
            
        }

        private bool IsNextPositionAreBeingInSpace(Point nextPosition, Size spaceSize)
        {
            bool isUnderTopBorderLine =
                nextPosition.Y - Radius >= 0;

            bool isAboveBottomBorderLine =
                nextPosition.Y + Radius <= spaceSize.Height;

            bool isLeftOfRightBorderLine =
                nextPosition.X + Radius <= spaceSize.Width;

            bool isRightOfLeftBorderLine =
                nextPosition.X - Radius >= 0;

            return isUnderTopBorderLine &&
                   isAboveBottomBorderLine &&
                   isLeftOfRightBorderLine &&
                   isRightOfLeftBorderLine;
        }
    }
}
