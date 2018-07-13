﻿using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MiraiNotes.UWP.Extensions
{
    public static class FocusExtension
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }


        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }


        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
             "IsFocused", typeof(bool), typeof(FocusExtension),
             new PropertyMetadata(false, OnIsFocusedPropertyChanged));


        private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d != null && d is Control)
            {
                var _Control = d as Control;
                if ((bool)e.NewValue)
                {
                    // To set false value to get focus on control. if we don't set value to False then we have to set all binding
                    //property to first False then True to set focus on control.
                    OnLostFocus(_Control, null);
                    _Control.Focus(FocusState.Programmatic); // Don't care about false values.
                    _Control.Focus(FocusState.Keyboard);
                }
            }
        }

        private static void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is Control)
            {
                (sender as Control).SetValue(IsFocusedProperty, false);
            }
        }
    }
}
