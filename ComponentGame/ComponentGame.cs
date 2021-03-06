﻿using System;
using Common;
using Common.PerformanceMonitoring;
using ComponentGame.Components;
using ComponentGame.Systems;
using ComponentGame.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static Common.Constants;
using Color = Microsoft.Xna.Framework.Color;
using Math = Common.Math;
using MathF = Common.MathF;
using static ComponentGame.Scene;

namespace ComponentGame
{
    public class ComponentGame : Game
    {
        private readonly GraphicsDeviceManager graphics;

        private MoveSystem moveSystem;
        private VelocityModifierSystem velocityModifierSystem;

        private Texture2D[] texture2Ds;
        private SpriteBatch spriteBatch;

        public ComponentGame()
        {
            PerfMon.InitializeStarted();
            
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            var displaySize = this.graphics.GraphicsDevice.DisplayMode.TitleSafeArea;
            this.graphics.PreferredBackBufferHeight = displaySize.Height - 100;
            this.graphics.PreferredBackBufferWidth = displaySize.Width;
            this.graphics.ApplyChanges();

            const int minX = 0;
            var maxX = this.graphics.PreferredBackBufferWidth;
            const int minY = 0;
            var maxY = this.graphics.PreferredBackBufferHeight;
            
            this.moveSystem = new MoveSystem(this.graphics, EntityCount, PositionComponents, VelocityComponents);
            this.velocityModifierSystem = new VelocityModifierSystem(EntityCount, VelocityModifierCount, PositionComponents,
                VelocityConstraintComponents, VelocityComponents, SizeComponents, VelocityModifierComponents);

            var random = new Random();
            
            for(int i = 0; i < EntityCount; i++)
            {
                var position = new PositionComponent();
                position.Value = new Vector2(random.Next(minX, maxX), random.Next(minY, maxY));
                PositionComponents[i] = position;

                var entityTypePredicate = i >= DotCount;
                var velMin = MathF.Select(Dot.MinVelocity, Bubble.MinVelocity, entityTypePredicate);
                var velMax = MathF.Select(Dot.MaxVelocity, Bubble.MaxVelocity, entityTypePredicate);

                VelocityConstraintComponents[i] = new VelocityConstraintComponent(velMin, velMax);

                var velocity = new VelocityComponent();
                velocity.Value = random.NextVelocity(velMin, velMax);
                VelocityComponents[i] = velocity;
                
                var velocityModifierValue = (float)random.NextDouble() * (Bubble.MaxModifier - Bubble.MinModifier) + Bubble.MinModifier;
                var scaleMax = MathF.Select(Bubble.MinModifier, Bubble.MaxModifier, velocityModifierValue >= 0.0f);
                var bubbleAlpha = (byte)(int)(128 * (velocityModifierValue / scaleMax));
                var bubbleColorR = MathB.Select(0, byte.MaxValue, velocityModifierValue < 0.0f);
                var bubbleColorG = MathB.Select(0, byte.MaxValue, velocityModifierValue >= 0.0f);

                var dotColors = new byte[3];
                random.NextBytes(dotColors);
                var colorR = MathB.Select(dotColors[0], bubbleColorR, entityTypePredicate);
                var colorG = MathB.Select(dotColors[1], bubbleColorG, entityTypePredicate);
                var colorB = MathB.Select(dotColors[2], byte.MinValue, entityTypePredicate);
                var alpha = MathB.Select(byte.MaxValue, bubbleAlpha, entityTypePredicate);
                var index = Math.Select(Sprites.Dot, Sprites.Bubble, entityTypePredicate);
                SpriteComponents[i] = new SpriteComponent(colorR, colorG, colorB, alpha, index);
                
                if(entityTypePredicate)
                {
                    var modifierIndex = i - DotCount;
                    
                    SizeComponents[modifierIndex] = new SizeComponent(64);
                    
                    VelocityModifierComponents[modifierIndex] = new VelocityModifierComponent(velocityModifierValue);
                }
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            var perfSpriteBatch = new SpriteBatch(this.GraphicsDevice);
            var font = this.Content.Load<SpriteFont>(nameof(Fonts.Consolas));

            this.spriteBatch = new SpriteBatch(GraphicsDevice);

            this.texture2Ds = new Texture2D[2];
            this.texture2Ds[Sprites.Dot] = Content.Load<Texture2D>(nameof(Sprites.Dot));
            this.texture2Ds[Sprites.Bubble] = Content.Load<Texture2D>(nameof(Sprites.Bubble));
            
            PerfMon.InitializeFinished(perfSpriteBatch, font);
        }

        protected override void Update(GameTime gameTime)
        {
            PerfMon.UpdateStarted();
            
            if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            this.moveSystem.Update(deltaTime);
            this.velocityModifierSystem.Update(deltaTime);

            base.Update(gameTime);
            
            PerfMon.UpdateFinished();
        }

        protected override void Draw(GameTime gameTime)
        {
            PerfMon.DrawStarted();
            
            GraphicsDevice.Clear(Color.CornflowerBlue);

            this.spriteBatch.Begin(SpriteSortMode.FrontToBack);

            var sprites = new Span<SpriteComponent>(SpriteComponents);
            var positions = new Span<PositionComponent>(PositionComponents);

            for(int i = 0; i < EntityCount; i++)
            {
                var texture2D = this.texture2Ds[sprites[i].Index];
                var color = new Color(sprites[i].ColorR, sprites[i].ColorG, sprites[i].ColorB, sprites[i].Alpha);
                var origin = new Vector2(texture2D.Width / 2, texture2D.Height / 2);
                this.spriteBatch.Draw(texture2D, positions[i].Value, null, color, 0.0f, origin, Vector2.One, SpriteEffects.None, sprites[i].Index);
            }
            
            this.spriteBatch.End();

            base.Draw(gameTime);
            
            PerfMon.DrawFinished();
        }
    }
}
