using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExtensionsFinder
{
    public partial class MainForm : Form
    {
        //bytes in GB. Used to limit files that can be opened by user
        private long GB = 1073741824; //bytes in GB

        private FolderBrowserDialog FolderBrowser = null;
        private ExtensionFinder Finder = null;
        private List<KeyValuePair<string, List<string>>> MultipleExtensionsList = null;
        private List<string> NullExtensionsList = null;

        private string _ExtensionsDataBaseFile = null;
        private string ExtensionsDataBaseFile
        {
            get { return _ExtensionsDataBaseFile; }
            set { _ExtensionsDataBaseFile = value; }
        }

        public MainForm()
        {
            InitializeComponent();
            FolderBrowser = new FolderBrowserDialog
            {
                // Set the help text description for the FolderBrowserDialog.
                Description =
                "Select the directory that you want to use.",
            };

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            IdentifyButton.Enabled = false;
            OpenSelectedFolder.Enabled = false;
            openToolStripMenuItem.Enabled = false;
            showExtensionsDatabaseToolStripMenuItem.Enabled = false;
            FolderButton.Enabled = false;
            ExtensionsDataBaseFile = Path.Combine(Application.StartupPath, "Content", "FilesExtensionsDatabase.txt");
            try
            {
                Finder = new ExtensionFinder(ExtensionsDataBaseFile);
                FolderButton.Enabled = true;
                openToolStripMenuItem.Enabled = true;
                showExtensionsDatabaseToolStripMenuItem.Enabled = true;

            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }

        private void IdentifyButton_Click(object sender, EventArgs e)
        {
            MultipleExtensionsList = new List<KeyValuePair<string, List<string>>>();
            NullExtensionsList = new List<string>();

            foreach (var Item in FilesListBox.Items)
            {
                string FileWithoutExtension = Path.Combine(FolderBrowser.SelectedPath, Item.ToString());
                var Extension = IdentifyFileExtension(Item, FileWithoutExtension);
                if (Extension.Value.Count > 0)
                {
                    if (Extension.Value.Count > 1)
                        MultipleExtensionsList.Add(Extension);
                    else
                    {
                        File.Move(FileWithoutExtension,
                            Path.ChangeExtension(FileWithoutExtension, Extension.Value.First()));
                    }
                }
                else
                    NullExtensionsList.Add(FileWithoutExtension);
            }

            FillFilesList();

            if (MultipleExtensionsList.Count >= 1 || NullExtensionsList.Count >= 1)
                ProcessSuggestions();
        }

        private void FolderButton_Click(object sender, EventArgs e)
        {
            DialogResult result = FolderBrowser.ShowDialog();
            if (result == DialogResult.OK)
            {
                IdentifyButton.Enabled = true;
                OpenSelectedFolder.Enabled = true;

                FillFilesList();
            }
        }


        private void FilesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string FilePath = Path.Combine(FolderBrowser.SelectedPath, FilesListBox.SelectedItem.ToString());
            FileInfo Info = new FileInfo(FilePath);
            FillFileInfoViewer(Info);
        }

        //process suggested extensions for files which have multiple extension byte signature 
        private void ProcessSuggestions()
        {
            string OutputLogString = null;
            OutputLogString = "Some files has been marked as \"unresolved\" because each of them have no extension signature" +
                " or have multiple extension signatures." + Environment.NewLine +
                "List of unresolved files listed below." + Environment.NewLine + Environment.NewLine;

            if (NullExtensionsList.Count > 0)
            {
                OutputLogString += "No extension signature found for files: " + Environment.NewLine;
                foreach (string Item in NullExtensionsList)
                {
                    if (Item != null)
                    {
                        OutputLogString += "\t" + Item + Environment.NewLine;
                    }
                }
            }

            if (MultipleExtensionsList.Count > 0)
            {
                OutputLogString += "Multiple extension signatures found for files: " + Environment.NewLine;
                foreach (var ExtensionsList in MultipleExtensionsList)
                {
                    OutputLogString += "\t" + FolderBrowser.SelectedPath + "\\" +
                               ExtensionsList.Key + " | Suggested extension: ";
                    if (ExtensionsList.Value != null)
                        foreach (var Item in ExtensionsList.Value)
                        {
                            OutputLogString += Item + " ";
                        }
                    OutputLogString += Environment.NewLine;
                }
                OutputLogString += Environment.NewLine
                    + "You can manually change file extension with suggested one to see which of are suitable.";
            }

            OutputLogString += Environment.NewLine + "List of extension signatures located here: " + ExtensionsDataBaseFile;
            string OutputLogFilePath = Path.Combine(Application.StartupPath, "ErrorLog.txt");
            File.WriteAllText(OutputLogFilePath, OutputLogString);

            DialogResult Result = MessageBox.Show("Errors occured while indentify files extensions. Open Error log?", "Error",
                MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            if (Result == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start("notepad.exe", OutputLogFilePath);
            }
        }

        private KeyValuePair<string, List<string>> IdentifyFileExtension(object Item, string FileWithoutExtension)
        {
            KeyValuePair<string, List<string>> Extension = new KeyValuePair<string, List<string>>();
            if (Finder != null)
            {
                Extension = Finder.FindExtensionForFile(FileWithoutExtension);
                return Extension;
            }
            return Extension;
        }

        private void FillFilesList()
        {
            if (FilesListBox.Items.Count > 0)
                FilesListBox.Items.Clear();

            var Files = Directory.GetFiles(FolderBrowser.SelectedPath);

            foreach (string File in Files)
            {
                int LastSlashIndex = File.LastIndexOf("\\") + 1;
                string FileName = null;
                for (int Index = LastSlashIndex; Index < File.Length; Index++)
                {
                    FileName += File[Index];
                }
                long Length = new FileInfo(File).Length;
                string Ext = new FileInfo(File).Extension;
                if (Length > 0 && Length <= GB && Ext.Length == 0)
                {
                    FilesListBox.Items.Add(FileName);
                    IdentifyButton.Enabled = true;
                    OpenSelectedFolder.Enabled = true;
                }

            }
            if (FilesListBox.Items.Count == 0)
            {
                IdentifyButton.Enabled = false;
                OpenSelectedFolder.Enabled = false;
                MessageBox.Show("No files with empty extension found in folder.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void FillFileInfoViewer(FileInfo Info)
        {
            if (FileInfoViewer.TextLength > 0)
                FileInfoViewer.Clear();

            FileInfoViewer.Text += "Attributes: " + Info.Attributes + "\n";
            FileInfoViewer.Text += "Creation Time: " + Info.CreationTime + "\n";
            FileInfoViewer.Text += "Directory: " + Info.DirectoryName + "\n";
            FileInfoViewer.Text += "Is Read Only: " + (Info.IsReadOnly ? "True" : "False") + "\n";
            FileInfoViewer.Text += "Last Access Time: " + Info.LastAccessTime + "\n";
            FileInfoViewer.Text += "Last Write Time: " + Info.LastWriteTime + "\n";
            FileInfoViewer.Text += "Length(bytes): " + Info.Length + "\n";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = FolderBrowser.ShowDialog();
            if (result == DialogResult.OK)
            {
                FillFilesList();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void showExtensionsDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe", ExtensionsDataBaseFile);
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe", Path.Combine(Application.StartupPath, "Content", "Information.txt"));
        }

        private void OpenSelectedFolder_Click(object sender, EventArgs e)
        {
            if (FolderBrowser.SelectedPath.Length > 0)
            {
                System.Diagnostics.Process.Start(FolderBrowser.SelectedPath);
            }
        }
    }
}
