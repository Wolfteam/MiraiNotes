using System;
using System.Threading.Tasks;

namespace MiraiNotes.UWP.Interfaces
{
    public interface ICustomDialogService
    {
        /// <summary>
        /// Displays information about an error.
        /// </summary>
        /// <param name="message">The message to be shown to the user.</param>
        /// <param name="title">The title of the dialog box. This may be null.</param>
        /// <returns>A Task allowing this async method to be awaited.</returns>
        Task ShowErrorMessageDialogAsync(Exception error, string title);

        /// <summary>
        /// Opens a modal message dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        Task ShowMessageDialogAsync(string title, string message);

        /// <summary>
        /// Opens a modal message dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <param name="buttonText">The button text.</param>
        /// <returns>Task.</returns>
        Task ShowMessageDialogAsync(string title, string message, string buttonText);

        /// <summary>
        /// Opens a modal confirmation dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <returns>Task&lt;System.Nullable&lt;System.Boolean&gt;&gt;.</returns>
        Task<bool?> ShowConfirmationDialogAsync(string title, string message);

        /// <summary>
        /// Opens a modal confirmation dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <param name="yesButtonText">The 'Yes' button text.</param>
        /// <param name="noButtonText">The 'No' button text.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        Task<bool> ShowConfirmationDialogAsync(string title, string message, string yesButtonText, string noButtonText);

        /// <summary>
        /// Opens a modal confirmation dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <param name="yesButtonText">The 'Yes' button text.</param>
        /// <param name="noButtonText">The 'No' button text.</param>
        /// <param name="cancelButtonText">The cancel button text.</param>
        /// <returns>Task&lt;System.Nullable&lt;System.Boolean&gt;&gt;.</returns>
        Task<bool?> ShowConfirmationDialogAsync(string title, string message, string yesButtonText, string noButtonText, string cancelButtonText);

        /// <summary>
        /// Opens a modal input dialog for a string.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> ShowInputStringDialogAsync(string title);

        /// <summary>
        /// Opens a modal input dialog for a string.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="defaultText">The default response text.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> ShowInputStringDialogAsync(string title, string defaultText);

        /// <summary>
        /// Opens a modal input dialog for a string.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="defaultText">The default response text.</param>
        /// <param name="okButtonText">The 'OK' button text.</param>
        /// <param name="cancelButtonText">The 'Cancel' button text.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> ShowInputStringDialogAsync(string title, string defaultText, string okButtonText, string cancelButtonText);

        /// <summary>
        /// Opens a modal input dialog for a multi-line text.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> ShowInputTextDialogAsync(string title);

        /// <summary>
        /// Opens a modal input dialog for a multi-line text.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="defaultText">The default text.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> ShowInputTextDialogAsync(string title, string defaultText);

        /// <summary>
        /// Opens a modal input dialog for a multi-line text.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="defaultText">The default text.</param>
        /// <param name="okButtonText">The 'OK' button text.</param>
        /// <param name="cancelButtonText">The 'Cancel' button text.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> ShowInputTextDialogAsync(string title, string defaultText, string okButtonText, string cancelButtonText);
    }
}
