using Microsoft.Maui.Media;
using System;
using System.Threading.Tasks;

namespace Coach_app.Services.Files
{
    public interface IFileService
    {
        Task<string> PickPhotoAsync();
    }

    public class FileService : IFileService
    {
        public async Task<string> PickPhotoAsync()
        {
            try
            {
                var result = await MediaPicker.Default.PickPhotoAsync();
                return result?.FullPath;
            }
            catch (Exception)
            {
                // On pourrait logger l'erreur ici
                return null;
            }
        }
    }
}