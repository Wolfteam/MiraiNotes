using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Text;
using Android.Views;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using System;

namespace MiraiNotes.Android.Listeners
{
    public enum UnderlayButtonPosition
    {
        Right,
        Left
    }

    public class SwipeButton
    {
        private readonly Context _context;
        private int _pos;
        private RectF _clickRegion;

        public int Id { get; }
        public string Text { get; set; }
        public int ImageResId { get; set; }
        public Color BackgroundColor { get; set; }
        public Color IconColor { get; set; }
        public Color TextColor { get; set; }
        public int TextSize { get; }
        public UnderlayButtonPosition Position { get; set; }
        public ISwipeButtonClickListener Listener { get; set; }

        public SwipeButton(
            Context context,
            int id,
            string text,
            int imageResId,
            Color backgroundColor,
            Color iconColor,
            Color textColor,
            int textSize = 14,
            UnderlayButtonPosition position = UnderlayButtonPosition.Right,
            ISwipeButtonClickListener listener = null)
        {
            _context = context;
            Id = id;
            Text = text;
            ImageResId = imageResId;
            BackgroundColor = backgroundColor;
            IconColor = iconColor;
            TextColor = textColor;
            TextSize = textSize;
            Position = position;
            Listener = listener;
        }

        public bool OnClick(float x, float y)
        {
            if (_clickRegion != null && _clickRegion.Contains(x, y))
            {
                Listener?.OnClick(Id, _pos);
                return true;
            }
            return false;
        }

        public void OnDraw(Canvas c, RectF rect, int pos, float dX, View itemView)
        {
            _clickRegion = rect;
            _pos = pos;

            //POR ALGUNA RAZON EL COLOR ROJO NO SE VE EN ANDROID 5...
            using (var colorDrawable = new ColorDrawable(BackgroundColor))
            {
                colorDrawable.SetBounds((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);
                colorDrawable.Draw(c);
            }

            // Draw Icon
            float cHeight = rect.Height();
            float cWidth = rect.Width();

            int iconHeight = (int)AndroidUtils.ToPixel(48, _context);
            int iconWidth = (int)AndroidUtils.ToPixel(48, _context);
            int iconTop = (int)(rect.Top + cHeight * 0.1);
            int iconBottom = iconTop + iconHeight;
            int iconLeft = 0;
            int iconRight = 0;

            if (dX > 0)
            {
                iconLeft = (int)(rect.Left + cWidth / 2 - iconWidth / 2);
                iconRight = iconLeft + iconWidth;
            }
            else if (dX < 0)
            {
                iconLeft = (int)(rect.Left + cWidth / 2 - iconWidth / 2);
                iconRight = iconLeft + iconWidth;
            }
            DrawIcon(c, dX, iconLeft, iconTop, iconRight, iconBottom);

            //Draw Text
            var paint = new TextPaint
            {
                AntiAlias = true,
                Color = TextColor,
                TextSize = AndroidUtils.ToPixel(TextSize, _context),
                TextAlign = Paint.Align.Left,
            };
            paint.SetTypeface(Typeface.Create(Typeface.Default, TypefaceStyle.Bold));
            var r = new Rect();
            paint.GetTextBounds(Text, 0, Text.Length, r);

            float x = cWidth / 2f - r.Width() / 2f - r.Left;
            float y = cHeight / 2f + r.Height() / 2f - r.Bottom;
            //c.DrawText(Text, rect.Left + x, rect.Top + y, p);
            c.DrawText(Text, rect.Left + x, (float)(iconBottom + 20), paint);
        }

        private void DrawIcon(Canvas c, float dx, int iconLeft, int iconTop, int iconRight, int iconBottom)
        {
            if (dx > 0 || dx < 0)
            {
                var drawable = ContextCompat.GetDrawable(_context, ImageResId);
                drawable.SetTint(IconColor);
                drawable.SetBounds(iconLeft, iconTop, iconRight, iconBottom);
                drawable.Draw(c);
            }
        }

        private void DrawText(Canvas c, RectF rect)
        {
            // Draw Text
            var textPaint = new TextPaint
            {
                TextSize = AndroidUtils.ToPixel(TextSize, _context),
                Color = TextColor
            };
            c.Save();

            var sl = new StaticLayout(Text, textPaint, Math.Abs((int)rect.Width()), Layout.Alignment.AlignCenter, 1, 1, false);
            var r = new Rect();
            textPaint.GetTextBounds(Text, 0, Text.Length, r);

            float y = (rect.Height() / 2f) + (r.Height() / 2f) - r.Bottom - (sl.Height / 2);
            c.Translate(rect.Left, rect.Top + y);
            sl.Draw(c);
            c.Restore();
        }
    }
}