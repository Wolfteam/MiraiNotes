using System.Drawing;
using System.Linq;
using Xamarin.UITest.Queries;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace MiraiNotes.Android.UiTests.Pages.Dialogs
{
    public abstract class BaseDialog : BasePage
    {
        protected bool IsTextInInputInvalid(string text, AppQuery query, Query okButton, Query input, Color desiredColor)
        {
            App.ClearText(input);
            App.EnterText(input, text);

            //If the text is empty we wont be able to see the button
            if (App.Query(okButton).Any())
                App.Tap(okButton);

            //If the query to check the validator error doesnt return nothing, that means that is valid
            if (!App.Query(x => query).Any())
                return false;

            var warningColor = GetTextColor(query);
            bool areClose = ColorsAreClose(warningColor, desiredColor);

            bool updateButtonIsVisible = App.Query(okButton).Any();

            return !updateButtonIsVisible || !areClose;
        }
    }
}
