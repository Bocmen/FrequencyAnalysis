using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FrequencyAnalysisApp.Tools
{
    public static class PrefabsDialog
    {
        public const string ContentDialogNoGeneratedResult = "ContentDialogNoGeneratedResult";
        public const string GlobalContentDialog = "GlobalContentDialog";

        private static readonly ContentDialog ContentDialogWaitTask = new ContentDialog()
        {
            Style = (Style)Application.Current.Resources[GlobalContentDialog],
            Content = new ProgressRing()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsActive = true
            }
        };

        public static async Task ShowErrorNotReult()
        {
            ContentDialog contentDialog = new ContentDialog()
            {
                Style = (Style)Application.Current.Resources[ContentDialogNoGeneratedResult]
            };
            await contentDialog.ShowAsync();
        }
        public static async Task ViewWaitTask(Func<Task> startTask, string message = null)
        {
            ContentDialogWaitTask.Title = message ?? string.Empty;
            _ = ContentDialogWaitTask.ShowAsync();
            await startTask();
            ContentDialogWaitTask.Hide();
        }
    }
}
