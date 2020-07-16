using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Text;
using Android.Views;
using AndroidX.Core.Content;
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

        private Func<int, string> _getText;
        private Func<int, Color> _getBgColor;

        private int _iconWidth;
        private int _iconHeight;

        public int Id { get; }
        public string Text
            => _getText(_pos);
        public int ImageResId { get; set; }
        public Color BackgroundColor
            => _getBgColor(_pos);
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

            _getText = (_) => text;
            _getBgColor = (_) => backgroundColor;
            Id = id;
            ImageResId = imageResId;
            IconColor = iconColor;
            TextColor = textColor;
            TextSize = textSize;
            Position = position;
            Listener = listener;

            Init();
        }

        public SwipeButton(
            Context context,
            int id,
            Func<int, string> text,
            int imageResId,
            Func<int, Color> backgroundColor,
            Color iconColor,
            Color textColor,
            int textSize = 14,
            UnderlayButtonPosition position = UnderlayButtonPosition.Right,
            ISwipeButtonClickListener listener = null)
        {
            _context = context;
            _getBgColor = backgroundColor;
            _getText = text;

            Id = id;
            ImageResId = imageResId;
            IconColor = iconColor;
            TextColor = textColor;
            TextSize = textSize;
            Position = position;
            Listener = listener;

            Init();
        }

        private void Init()
        {
            _iconWidth = (int)AndroidUtils.ToPixel(48, _context);
            _iconHeight = (int)AndroidUtils.ToPixel(48, _context);
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

            float cHeight = rect.Height();
            float cWidth = rect.Width();

            // Draw background
            DrawBackground(c, rect);

            var r = new Rect();
            var textPaint = new TextPaint
            {
                TextSize = AndroidUtils.ToPixel(TextSize, _context),
                Color = TextColor
            };

            textPaint.GetTextBounds(Text, 0, Text.Length, r);

            int box = (int)(r.Height() + cHeight * 0.05 + _iconHeight);
            var extraTop = GetExtraTop(cHeight, box);

            //int iconTop = (int)(rect.Top + GetTop(cHeight, _iconHeight));
            int iconTop = (int)(rect.Top + extraTop);
            int iconBottom = iconTop + _iconHeight;
            int iconLeft = 0;
            int iconRight = 0;

            if (dX > 0)
            {
                iconLeft = (int)(rect.Left + cWidth / 2 - _iconWidth / 2);
                iconRight = iconLeft + _iconWidth;
            }
            else if (dX < 0)
            {
                iconLeft = (int)(rect.Left + cWidth / 2 - _iconWidth / 2);
                iconRight = iconLeft + _iconWidth;
            }

            //Draw Text
            bool textWasDrawed = DrawText(c, rect, textPaint, r, iconBottom);

            // Draw Icon
            if (textWasDrawed)
                DrawIcon(c, dX, iconLeft, iconTop, iconRight, iconBottom);
        }

        private void DrawBackground(Canvas c, RectF rect)
        {
            using (var colorDrawable = new ColorDrawable(BackgroundColor))
            {
                colorDrawable.SetBounds((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);
                colorDrawable.Draw(c);
            }
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

        private bool DrawText(Canvas c, RectF rect, TextPaint textPaint, Rect r, int iconBottom)
        {
            c.Save();

            StaticLayout sl;
            if (Build.VERSION.SdkInt > BuildVersionCodes.M)
            {
                sl = StaticLayout.Builder.Obtain(Text, 0, Text.Length, textPaint, Math.Abs((int)rect.Width()))
                    .SetMaxLines(2)
                    .SetLineSpacing(1, 1)
                    .SetIncludePad(false)
                    .SetEllipsize(TextUtils.TruncateAt.End)
                    .SetAlignment(Layout.Alignment.AlignCenter)
                    .Build();
            }
            else
            {
                sl = new StaticLayout(Text, textPaint, Math.Abs((int)rect.Width()), Layout.Alignment.AlignCenter, 1, 1, false);
            }

            using (sl)
            {
                //float y = rect.Top + rect.Height() / 2 + GetTop(rect.Height(), r.Height()) - sl.Height / 2;
                float y = iconBottom;
                if (r.Width() / 2 > rect.Width())
                    return false;

                c.Translate(rect.Left, y);
                sl.Draw(c);
                c.Restore();
            }

            return true;
        }

        private float GetExtraTop(float rectHeight, float itemHeight)
        {
            var diff = rectHeight - itemHeight;
            return Math.Abs(diff / 2);
        }
    }
}