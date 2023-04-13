namespace BlazorBLE.Services
{
    internal interface IPromptService
    {
        Task ShowAlertAsync(string title, string message, string cancel = "OK");
        Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No");
        Task<string> ShowPromptAsync(string title, string message, string accept = "Yes", string cancel = "No");

        void ShowAlert(string title, string message, string cancel = "OK");
        void ShowConfirmation(string title, string message, Action<bool> callback, string accept = "Yes", string cancel = "No");
    }
}
