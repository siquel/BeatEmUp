﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Neva.BeatEmUp.Collision.Dynamics;
using Neva.BeatEmUp.Collision.Shape;
using Neva.BeatEmUp.GameObjects.Components;
using Neva.BeatEmUp.Scripts.CSharpScriptEngine.Builders;
using Neva.BeatEmUp.Scripts.CSharpScriptEngine.ScriptClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neva.BeatEmUp.GameObjects
{
    public sealed class GameObject
    {
        #region Vars
        private readonly List<GameObject> childs;
        private readonly TagContainer tags;
        private readonly ComponentManager componentManager;
        private readonly List<Behaviour> behaviours;
        private readonly BeatEmUpGame game;

        private Body body;
        private Vector2 size;

        private bool destroyed;
        private bool visible;
        private bool enabled;

        private string name;

        private GameObject parent;
        #endregion

        #region Events
        public event GameObjectEventHandler<GameObjectEventArgs> OnDestroy;
        public event GameObjectEventHandler<GameObjectEventArgs> OnDisable;
        public event GameObjectEventHandler<GameObjectEventArgs> OnEnable;
        public event GameObjectEventHandler<GameObjectEventArgs> OnShow;
        public event GameObjectEventHandler<GameObjectEventArgs> OnHide;
        #endregion

        #region Properties
        public bool Destroyed
        {
            get
            {
                return destroyed;
            }
        }
        public BeatEmUpGame Game
        {
            get
            {
                return game;
            }
        }

        public Body Body
        {
            get
            {
                return body;
            }
            set
            {
                if (game.ContainsBody(body))
                {
                    game.RemoveBody(body);
                }

                body = value;
            }
        }
        public Vector2 Position
        {
            get
            {
                return body.Position;
            }
            set
            {
                body.Position = value;
            }
        }
        public Vector2 Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }
        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                visible = value;
            }
        }
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }
        public int ChildsCount
        {
            get
            {
                return childs.Count;
            }
        }
        public bool IsChild
        {
            get
            {
                return parent != null;
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        #endregion

        public GameObject(BeatEmUpGame game)
        {
            this.game = game;

            behaviours = new List<Behaviour>();
            componentManager = new ComponentManager();
            
            tags = new TagContainer();

            string stateName = game.CurrentGameState == null ? "" : game.CurrentGameState.Name;
            tags.AddTag(stateName);

            OnDestroy += delegate { };
            OnDisable += delegate { };
            OnEnable += delegate { };
            OnShow += delegate { };
            OnHide += delegate { };

            body = new Body(this, new BoxShape(size.X, size.Y), Vector2.Zero);
            childs = new List<GameObject>();

            enabled = true;
            visible = true;

            name = this.GetType().Name;
        }
        public GameObject(BeatEmUpGame game, string behaviourName)
            : this(game)
        {
            behaviours.Add(game.GetScript<Behaviour>(new ScriptBuilder(behaviourName, new object[] { this })));
        }
        public GameObject(BeatEmUpGame game, string[] behaviourNames)
            : this(game)
        {
            for (int i = 0; i < behaviourNames.Length; i++)
            {
                behaviours.Add(game.GetScript<Behaviour>(new ScriptBuilder(behaviourNames[i], new object[] { this })));
            }
        }

        public void Enable()
        {
            if (enabled)
            {
                return;
            }

            enabled = true;

            OnEnable(this, new GameObjectEventArgs());
        }
        public void Disable()
        {
            if (!enabled)
            {
                return;
            }

            enabled = false;

            OnDisable(this, new GameObjectEventArgs());
        }
        public void Show()
        {
            if (visible)
            {
                return;
            }

            visible = true;

            OnShow(this, new GameObjectEventArgs());
        }
        public void Hide()
        {
            if (!visible)
            {
                return;
            }

            visible = false;

            OnHide(this, new GameObjectEventArgs());
        }

        /// <summary>
        /// Lisää uuden behaviourin.
        /// </summary>
        public void AddBehaviour(string name)
        {
            behaviours.Add(game.GetScript<Behaviour>(new ScriptBuilder(name, new object[] { this })));
        }
        /// <summary>
        /// Poistaa behaviourin.
        /// </summary>
        public bool RemoveBehaviour(string name)
        {
            Behaviour behaviour = behaviours.Find(b => b.Name == name);

            return behaviours.Remove(behaviour);
        }
        /// <summary>
        /// Aloittaa behaviourin suorittamisen.
        /// </summary>
        public bool StartBehaviour(string name)
        {
            Behaviour behaviour = behaviours.Find(b => b.Name == name);

            if (behaviour == null)
            {
                // TODO: log warning.

                return false;
            }

            behaviour.Start();

            return true;
        }
        /// <summary>
        /// Lopettaa behaviourin suorittamisen.
        /// </summary>
        public bool StopBehaviour(string name)
        {
            Behaviour behaviour = behaviours.Find(b => b.Name == name);

            if (behaviour == null)
            {
                // TODO: log warning.

                return false;
            }

            behaviour.Stop();

            return true;
        }

        public bool ContainsTag(string tag)
        {
            return tags.ContainsTag(tag);
        }
        public void AddTag(string tag)
        {
            tags.AddTag(tag);
        }
        public bool RemoveTag(string tag)
        {
            return tags.RemoveTag(tag);
        }

        public void AddChild(GameObject child)
        {
            childs.Add(child);
            child.parent = this;
        }
        public bool RemoveChild(GameObject child)
        {
            if (childs.Contains(child))
            {
                childs.Remove(child);
                child.parent = null;

                return true;
            }

            return false;
        }
        public GameObject GetChild(Predicate<GameObject> predicate)
        {
            return childs.Find(c => predicate(c));
        }

        public void AddComponent(GameObjectComponent component)
        {
            componentManager.AddComponent(component);
        }
        public bool RemoveComponent(GameObjectComponent component)
        {
            return componentManager.RemoveComponent(component);
        }
        public bool ContainsComponent(GameObjectComponent component)
        {
            return componentManager.Find<GameObjectComponent>(c => ReferenceEquals(c, component)) != null;
        }
        
        public IEnumerable<GameObjectComponent> Components()
        {
            return componentManager.Components();
        }
        public IEnumerable<T> ComponentsOftype<T>() where T : GameObjectComponent
        {
            return componentManager.ComponentsOfType<T>();
        }

        public T GetComponentOfType<T>() where T : GameObjectComponent
        {
            return componentManager.FirstOfType<T>();
        }
        public T FindComponent<T>(Predicate<T> predicate) where T : GameObjectComponent
        {
            return componentManager.Find<T>(predicate);
        }

        /// <summary>
        /// Alustaa kaikki komponentit.
        /// </summary>
        public void InitializeComponents()
        {
            foreach (GameObjectComponent component in componentManager.Components())
            {
                component.Initialize();
            }
        }
        /// <summary>
        /// Käynnistää kaikki toiminnot.
        /// </summary>
        public void StartBehaviours()
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                behaviours[i].Start();
            }
        }

        public void Destroy()
        {
            if (destroyed)
            {
                return;
            }

            destroyed = true;

            OnDestroy(this, new GameObjectEventArgs());

            for (int i = 0; i < childs.Count; i++)
            {
                childs[i].Destroy();
            }
            
            OnDestroy = null;
        }

        public void Update(GameTime gameTime)
        {
            if (destroyed || !enabled)
            {
                return;
            }

            IEnumerable<ComponentUpdateResults> results = componentManager.Update(gameTime);

            for (int i = 0; i < behaviours.Count; i++)
            {
                behaviours[i].Update(gameTime, results);
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (destroyed || !visible)
            {
                return;
            }

            componentManager.Draw(spriteBatch);

            for (int i = 0; i < behaviours.Count; i++)
            {
                behaviours[i].Draw(spriteBatch);
            }
        }
    }
}
