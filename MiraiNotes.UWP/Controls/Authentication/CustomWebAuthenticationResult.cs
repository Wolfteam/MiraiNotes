namespace MiraiNotes.UWP.Controls
{
    /// <summary>
    /// Indicates the result of the authentication operation.
    /// </summary>
    public sealed class CustomWebAuthenticationResult
    {
        internal CustomWebAuthenticationResult(string response, uint errorDetail, CustomWebAuthenticationStatus responseStatus)
        {
            ResponseData = response;
            ResponseErrorDetail = errorDetail;
            ResponseStatus = responseStatus;
        }

        /// <summary>
        /// Contains the protocol data when the operation successfully completes.
        /// </summary>
        public string ResponseData { get; private set; }

        /// <summary>
        /// Returns the HTTP error code when <see cref="ResponseStatus"/> is equal to <see cref="CustomWebAuthenticationStatus.ErrorHttp"/>. 
        /// This is only available if there is an error.
        /// </summary>
        public uint ResponseErrorDetail { get; private set; }

        /// <summary>
        /// Contains the status of the asynchronous operation when it completes.
        /// </summary>
        public CustomWebAuthenticationStatus ResponseStatus { get; private set; }
    }
}
