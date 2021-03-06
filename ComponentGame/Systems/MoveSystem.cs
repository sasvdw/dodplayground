using System;
using Common;
using ComponentGame.Components;
using Microsoft.Xna.Framework;
using MathF = Common.MathF;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace ComponentGame.Systems
{
    public class MoveSystem
    {
        private readonly int minX, maxX, minY, maxY;
        private readonly int length;
        private readonly PositionComponent[] positionComponents;
        private readonly VelocityComponent[] velocityComponents;

        public MoveSystem(
            GraphicsDeviceManager graphics,
            int length,
            PositionComponent[] positionComponents,
            VelocityComponent[] velocityComponents)
        {
            this.minX = 0;
            this.maxX = graphics.PreferredBackBufferWidth;
            this.minY = 0;
            this.maxY = graphics.PreferredBackBufferHeight;
            this.length = length;
            this.positionComponents = positionComponents;
            this.velocityComponents = velocityComponents;
        }

        public void Update(float deltaTime)
        {
            var positions = new Span<PositionComponent>(this.positionComponents);
            var velocities = new Span<VelocityComponent>(this.velocityComponents);
            for(int i = 0; i < this.length; i++)
            {
                var position = positions[i];

                position.Value += velocities[i].Value * deltaTime;

                var velocity = velocities[i];

                var x = MathF.Select(1, -1, position.Value.X < this.minX || position.Value.X > this.maxX);
                var y = MathF.Select(1, -1, position.Value.Y < this.minY || position.Value.Y > this.maxY);
                var multiply = new Vector2(x, y);

                velocity.Value *= multiply;

                position.Value = new Vector2(position.Value.X.Clamp(this.minX, this.maxX), position.Value.Y.Clamp(this.minY, this.maxY));

                positions[i] = position;
                velocities[i] = velocity;
            }
        }
    }
}
