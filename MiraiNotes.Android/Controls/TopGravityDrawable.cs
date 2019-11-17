using Android.Graphics;
using Android.Graphics.Drawables;

namespace MiraiNotes.Android.Controls
{
    public class TopGravityDrawable : Drawable
    {
        private readonly Drawable _drawable;

        public override int Opacity => _drawable.Opacity;
        public override int IntrinsicHeight => _drawable.IntrinsicHeight;
        public override int IntrinsicWidth => _drawable.IntrinsicWidth;

        public TopGravityDrawable(Drawable drawable)
        {
            _drawable = drawable;
        }

        public override void Draw(Canvas canvas)
        {
            int halfCanvas = canvas.Height / 2;
            int halfDrawable = _drawable.IntrinsicHeight / 2;

            // align to top
            canvas.Save();
            canvas.Translate(0, -halfCanvas + halfDrawable);
            _drawable.Draw(canvas);
            canvas.Restore();
        }

        public override void SetAlpha(int alpha)
        {
            _drawable.SetAlpha(alpha);
        }

        public override void SetColorFilter(ColorFilter colorFilter)
        {
            _drawable.SetColorFilter(colorFilter);
        }

    }
}