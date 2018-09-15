using MiraiNotes.UWP.Interfaces;
using System;
using Windows.Storage;

namespace MiraiNotes.UWP.Services
{
    public class ApplicationSettingsServiceBase : IApplicationSettingsServiceBase
    {
        public object this[string key]
        {
            get
            {
                try
                {
                    return ApplicationData.Current.LocalSettings.Values[key];
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            set { ApplicationData.Current.LocalSettings.Values[key] = value; }
        }
    }
}
