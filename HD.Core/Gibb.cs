using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HD.Core
{
    public class Gibb
    {

        //public override void Draw(SpriteBatch spriteBatch, Point screenOffset)
        //{
        //    if (Type.Texture != null)
        //    {
        //        var currentFrame = GetCurrentFrame();
        //        var sourceRectangle = Type.GetFrameSourceRectangle(currentFrame);
        //        spriteBatch.Draw(Type.Texture, new Rectangle((int)Position.X - screenOffset.X, (int)Position.Y - screenOffset.Y, Type.SpriteWidth, Type.SpriteHeight), sourceRectangle, Color.White, Type.RotateRender ? Direction : 0, new Vector2(Type.SpriteWidth / 2, Type.SpriteHeight / 2), IsFacingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
        //    }
        //    else
        //    {
        //        var color = IsBlocked ? Color.Cyan : isOnEdge ? Color.Red : Color.Orange;
        //        if (IsFlying)
        //            color = Color.Pink;
        //        spriteBatch.DrawRectangle(new Rectangle((int)Position.X - screenOffset.X - Type.SpriteWidth / 2, (int)Position.Y - screenOffset.Y - Type.SpriteHeight / 2, Type.SpriteWidth, Type.SpriteHeight), color);
        //        spriteBatch.DrawString(Utility.SmallFont, Type.Name, new Vector2(Position.X - screenOffset.X, Position.Y - screenOffset.Y), Color.White);
        //        spriteBatch.DrawRectangle(new Rectangle((int)Position.X - screenOffset.X, OffsetBoundingBox.Bottom - screenOffset.Y, 1, 33), Color.Red);
        //    }
        //}
    }
}
