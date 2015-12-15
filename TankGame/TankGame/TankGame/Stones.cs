﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankGame
{
    class Stones :Block
    {
        public Stones()
            : base()
        {

        }

        public Stones(Vector2 position)
            : base(position)
        {
        }

        public override void loadContent(ContentManager content)
        {
            base.texture = content.Load<Texture2D>("Sprites/stone");
        }
    }
}
