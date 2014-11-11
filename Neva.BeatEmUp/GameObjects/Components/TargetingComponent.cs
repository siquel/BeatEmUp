﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Neva.BeatEmUp.Collision;
using Neva.BeatEmUp.Collision.Broadphase;

namespace Neva.BeatEmUp.GameObjects.Components
{
    public class TargetingComponent : GameObjectComponent
    {
        private AABB queryRegion;
        private GameObject target;
        #region Properties

        public GameObject Target
        {
            get
            {
                return target;
            }
        }

        #endregion

        public TargetingComponent(GameObject owner) : base(owner, false)
        {
            
        }

        

        protected override ComponentUpdateResults OnUpdate(GameTime gameTime, IEnumerable<ComponentUpdateResults> results)
        {
            // ei oteta itteä mukaan areaan
            queryRegion = new AABB(owner.Position.X + owner.Size.X + 1, owner.Position.Y, 32f, 32f);
            // TODO mites jos ei ookkaan tarpeeksi iso collider?
            List<BroadphaseProxy> proxies = owner.Game.World.QueryAABB(ref queryRegion);
            target = GetClosest(proxies);
    
            return new ComponentUpdateResults(this, true);
        }

        private GameObject GetClosest(List<BroadphaseProxy> proxies)
        {
            if (proxies.Count == 0) return null;

            int i = 0;
            float min = Vector2.Distance(owner.Position, proxies[0].Client.Position);
            for (int j = 1; j < proxies.Count; j++)
            {
                float d = Vector2.Distance(owner.Position, proxies[j].Client.Position);
                if (d < min)
                {
                    i = j;
                    min = d;
                }
            }
            return proxies[i].Client.Owner;
        }

        protected override void OnDraw(SpriteBatch spriteBatch)
        {
            spriteBatch.FillRectangle(queryRegion.ToRectangle(), Color.Black, 0f);

            if (target != null)
            {
                spriteBatch.FillRectangle(target.Body.BroadphaseProxy.AABB.ToRectangle(), Color.Orange, 0f);
            }
        }
    }
}