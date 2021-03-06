﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Neva.BeatEmUp.Collision.Dynamics;
using Neva.BeatEmUp.Collision.Shape;

namespace Neva.BeatEmUp.Collision.Narrowphase
{
    public class BoxBoxSolver : ICollisionSolver
    {
        // boxilla voi olla vaan 4
        private Vector2[] axesToCheck = new Vector2[4];

        public CollisionResult SolveCollision(Body bodyA, Body bodyB)
        {
            BoxShape boxA = bodyA.Shape as BoxShape;
            BoxShape boxB = bodyB.Shape as BoxShape;
            Transform tfA = bodyA.Transform;
            Transform tfB = bodyB.Transform;


            // TODO miten toi position saatais keskelle suoraan
#if !COLLISION_USE_CENTER_POINT
            Vector2 centerA = tfA.Position + new Vector2(boxA.HalfWidth, boxA.HalfHeight);
            Vector2 centerB = tfB.Position + new Vector2(boxB.HalfWidth, boxB.HalfHeight);
#endif
#if COLLISION_USE_CENTER_POINT
            Vector2 centerA = tfA.Position; 
            Vector2 centerB = tfB.Position; 
#endif

            boxA.CalculateOrientation(ref tfA, out axesToCheck[0]);
            boxB.CalculateOrientation(ref tfB, out axesToCheck[1]);
            axesToCheck[2] = new Vector2(-axesToCheck[0].Y, axesToCheck[0].X);
            axesToCheck[3] = new Vector2(-axesToCheck[1].Y, axesToCheck[1].X);

            Vector2 distance = centerA - centerB  ;
            float projectedDistance = 0;
            float minPenetration = float.MaxValue;

            Vector2 mtv = Vector2.Zero; // minium translation vector
            for (int i = 0; i < axesToCheck.Length; i++)
            {
                Vector2 projectionA, projectionB;
                projectedDistance = Math.Abs(Vector2.Dot(distance, axesToCheck[i]));
                boxA.Project(ref tfA, ref axesToCheck[i], out projectionA);
                boxB.Project(ref tfB, ref axesToCheck[i], out projectionB);

                float aSize = Math.Abs(projectionA.X) + Math.Abs(projectionA.Y);
                float bSize = Math.Abs(projectionB.X) + Math.Abs(projectionB.Y);
                float abSize = aSize + bSize;
                float penetration = abSize - projectedDistance;

                // seperate axis, ei collisionia
                 if (penetration <= 0)
                    return CollisionResult.NoCollision;

                 if (Math.Abs(penetration) < Math.Abs(minPenetration))
                 {
                     minPenetration = penetration;
                     mtv = axesToCheck[i];
                 }
            }
            // distance määrittää liikkumisen suunnan
            if (Vector2.Dot(distance, mtv) < 0f)
                mtv = -mtv;
            // seperating axis löyty, joten bodyt törmää
            return new CollisionResult()
            {
                Us = bodyA,
                Them = bodyB,
                Response = minPenetration * mtv,
                IsColliding = true
            };
        }

        public bool IsSolveable(Body bodyA, Body bodyB)
        {
            // TODO vois olla polygon?
            return bodyA.Shape.Type == ShapeType.Box && bodyB.Shape.Type == ShapeType.Box;
        }
    }
}
