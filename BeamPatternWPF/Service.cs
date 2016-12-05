using Microsoft.Win32;

namespace BeamPatternWPF
{
    internal static class Service
    {
    }

    internal static class FileDialogEx
    {
        public static string Open(string Title, string Filter)
        {
            var dlg = new OpenFileDialog { Title = Title, Filter = Filter };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }
    }
}