﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Neva.BeatEmUp.Collision;
using Neva.BeatEmUp.Collision.Dynamics;
using Neva.BeatEmUp.DataTypes;
using SaNi.TextureAtlas;

namespace Neva.BeatEmUp.GameObjects.Components.Shop
{
    public class ShopComponent : GameObjectComponent
    {
        private const string Root = @"Assets\Items\";
        public const int Slots = 5;
        private GameObject bigScreen;

        // haxx
        private string lastCollision;
        private string currentCollision;

        public ShopComponent(GameObject owner) : base(owner, true)
        {
            slots = new GameObject[Slots];
        }

        private GameObject[] slots;

        protected override void OnInitialize()
        {
            float initialX = 680f;
            float initialY = 320f; // blaze it
            float offsetX = 32f;

            float size = 0f;
            for (int i = 0; i < Slots; i++)
            {
                float x = (size*i) + initialX + (offsetX*i);
                float y = initialY;
                
                var slot = CreateSlot("Slot" + i , x, y);
                slots[i] = slot;
                slot.Body.OnCollision = OnCollision;
                size = slot.Size.X;
                owner.AddChild(slot);
                owner.Game.AddGameObject(slot);
                owner.Game.World.CreateBody(slot.Body, CollisionSettings.ShopCollisionGroup,
                    (CollisionGroup.All & ~CollisionSettings.ObstacleCollisionGroup) & ~CollisionSettings.ShopCollisionGroup); 
            }

            InitItems();
            InitBigScreen();
        }

        private void InitBigScreen()
        {
         // 140 korkea
            //440 levee

            // y 130
            // x 735

            bigScreen= new GameObject(owner.Game);
            bigScreen.Size = new Vector2(435f, 140f);
            bigScreen.Position = new Vector2(735f, 130f);
            bigScreen.Body.CollisionFlags = CollisionFlags.Solid;
            bigScreen.Body.BodyType = BodyType.Static;

            bigScreen.AddComponent(new ShopBigScreen(bigScreen));
            bigScreen.InitializeComponents();

            owner.Game.AddGameObject(bigScreen);
        }

        private void InitItems()
        {
            ItemData[] data = owner.Game.Content.Load<ItemData[]>(string.Format(@"Data\{0}Items", owner.Name));
            TextureAtlas itemset = owner.Game.Content.Load<TextureAtlas>(Root + "data");
            for (int i = 0; i < itemset.TextureCount && i < Slots; i++)
            {
                GameObject item = new GameObject(owner.Game);
                slots[i].AddChild(item);
                item.AddComponent(new ItemComponent(item, itemset, itemset.Rectangles.Keys.ElementAt(i), data));
                item.InitializeComponents();
                owner.Game.AddGameObject(item);
            }
        }

        private bool OnCollision(Body bodyA, Body bodyB)
        {
            currentCollision = bodyA.Owner.Name;
            // bodyA on aina tämä

            // onko meillä itemiä mitä myydä
            if (bodyA.Owner.ChildsCount == 0) return true;

            var screen = bigScreen.FirstComponentOfType<ShopBigScreen>();
            // on itemi, voidaanko heijastaa se yläruutuun?
            if (screen.IsOccupied) return true;

            // voidaan, annetaan pojalle jotain mitä piirtää
            var itemComponent = bodyA.Owner.ChildAtIndex(0).FirstComponentOfType<ItemComponent>();
            screen.Display(itemComponent);

            return true;
        }

        private GameObject CreateSlot(string name, float x, float y)
        {
            GameObject slot = new GameObject(owner.Game);
            slot.Name = name;
            slot.AddTag("ShopSlot");
            slot.Body.CollisionFlags = CollisionFlags.Sensor;
            slot.Body.BodyType = BodyType.Static;
            slot.Position = new Vector2(x, y);
            slot.Size = new Vector2(70f, 180f);
            slot.Body.Shape.Size = slot.Size;
            slot.Body.Shape.Size = slot.Size;
            //slot.AddComponent(new ColliderRenderer(slot));
            slot.InitializeComponents();
            owner.Game.World.CreateBody(slot.Body, CollisionSettings.ShopCollisionGroup, CollisionSettings.PlayerCollisionGroup);
            return slot;
        }

        protected override ComponentUpdateResults OnUpdate(GameTime gameTime, IEnumerable<ComponentUpdateResults> results)
        {
            // haxing, 
            if (currentCollision != null && currentCollision == lastCollision)
            {
                currentCollision = null;
            }
            else
            {
                bigScreen.FirstComponentOfType<ShopBigScreen>().Undisplay();
            }
            lastCollision = currentCollision;
            return new ComponentUpdateResults(this, true);
        }
    }
}
