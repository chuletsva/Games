﻿using System;
using AirForce.AirObjects.Bullets;

namespace AirForce.AirObjects.EnemyAI
{
    public sealed class BigShip : EnemyAI
    {
        private int strength = 3;

        public BigShip(Point2D position, Action<EnemyAI> objectDeathMethod) : base(position, 50, 5, Properties.Resources.big_enemy_ship, objectDeathMethod)
        {
        }

        public override void CollisionWithOtherAirObject(AirObject otherAirObject)
        {
            switch (otherAirObject)
            {
                case Meteor _:
                case PlayerShip _:
                    strength = 0;
                    break;

                case PlayerBullet _:
                    strength--;
                    break;
            }

            if (strength == 0)
                OnObjectDeathEvent(this);
        }

        public override void Move(Line groundLine)
        {
            if (Position.X + Radius >= 0) // 0
                Position = new Point2D(Position.X - MovespeedShift, Position.Y);
            else
                OnObjectDeathEvent(this);
        }
    }
}
