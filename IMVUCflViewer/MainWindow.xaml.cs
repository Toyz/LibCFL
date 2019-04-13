using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using LibCFL;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace IMVUCflViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string selectedPath = "";
        private CFLLoader _cflFile { get; set; }
        private List<CFLEntry> _cflItems { get; set; }
            
        public MainWindow()
        {
            InitializeComponent();
            Title = "CFL Viewer";
        }

        private async void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "IMVU CFL (*.cfl)|*.cfl";
            openFileDialog.Title = "Load IMVU CFL file";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                _cflFile = new CFLLoader(openFileDialog.FileName);

                await Task.Run(async () =>
                {
                    _cflItems = await _cflFile.GetEntries();

                    Dispatcher.Invoke(() => {
                        cflContent.ItemsSource = _cflItems;
                        Title = $"CFL Viewer ({System.IO.Path.GetFileName(openFileDialog.FileName)})";
                    });
                });
            }
        }

        private void MenuExitItem_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ExtractContent_Click(object sender, RoutedEventArgs e)
        {
            var name = (string) ((Button) sender).Tag;

            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.SelectedPath = selectedPath.Length > 0
                ? selectedPath
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            folderDialog.Description = "Select the folder to extract too";

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _cflItems.FirstOrDefault(x => x.Name == name)?.Save(selectedPath = folderDialog.SelectedPath);
                ((Button) sender).Content = "Saved";
                ((Button)sender).IsEnabled = false;
            }
        }

        private async void ExportCHKN_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "IMVU Project (*.chkn)|*.chkn";
            saveFileDialog.Title = "Export CFL to CHKN";

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                await Task.Run(() =>
                {
                    using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                        {
                            foreach (var entry in _cflItems)
                            {
                                var zipArchiveEntry = archive.CreateEntry(entry.Name, CompressionLevel.Fastest);
                                using (var zipStream = zipArchiveEntry.Open())
                                    zipStream.Write(entry.FileContents, 0, entry.FileContents.Length);
                            }
                        }
                    }
                });

                MessageBox.Show("Finished exporting CHKN", "Done");

            }
        }

        private void PreviewContent_Click(object sender, RoutedEventArgs e)
        {
            var name = (string)((Button)sender).Tag;
            if (name.EndsWith(".tga"))
            {
                return;
            }

            var v = new Viewer(_cflItems.FirstOrDefault(x => x.Name == name));
            v.Owner = this;
            v.Show();
        }
    }
}
