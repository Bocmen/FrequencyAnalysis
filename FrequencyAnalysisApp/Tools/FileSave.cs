using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace FrequencyAnalysisApp.Tools
{
    public static class FileSave
    {
        public static readonly Encoding DefaultEncoding = Encoding.UTF8;
        public delegate Task ContentWriter(StorageFile file);
        public static async Task Save(string fileType, ContentWriter contentWriter, string defaultName = null, PickerLocationId startLocation = PickerLocationId.DocumentsLibrary)
        {
            string fullNameFileType = $".{fileType}";
            var savePicker = new FileSavePicker()
            {
                SuggestedStartLocation = startLocation,
                FileTypeChoices =
                {
                    new KeyValuePair<string, IList<string>>(string.IsNullOrWhiteSpace(defaultName) ? $"{fileType}File" : defaultName, new List<string>() { fullNameFileType })
                }
            };
            var file = await savePicker.PickSaveFileAsync();
            if (file != null) await contentWriter(file);
        }
    }
}
