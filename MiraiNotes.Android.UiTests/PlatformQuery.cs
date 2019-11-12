﻿using System;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace MiraiNotes.Android.UiTests
{
    public class PlatformQuery
    {
        public Func<AppQuery, AppQuery> Android
        {
            set
            {
                if (AppManager.Platform == Platform.Android)
                {
                    current = value;
                }
            }
        }

        public Func<AppQuery, AppQuery> IOS
        {
            set
            {
                if (AppManager.Platform == Platform.iOS)
                {
                    current = value;
                }
            }
        }

        private Func<AppQuery, AppQuery> current;

        public Func<AppQuery, AppQuery> Current
        {
            get
            {
                if (current == null)
                {
                    throw new NullReferenceException("Trait not set for current platform");
                }

                return current;
            }
        }
    }
}
