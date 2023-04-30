using libTeensySharp;
using lunOptics.libTeensySharp;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace TeensyInstall
{
    public partial class frmFirmware : Form
    {
        // modified from https://github.com/luni64/TeensySharp/tree/master/src/Examples/07_WinForms%20Uploader
        // requires the NuGet package lunOptics.libTeensySharp
        // right click on project name in the solution explorer and select "Manage NuGet Packages..."
        // teensy uploader and serial monitor need to be closed to free the serial port

        public clsTools Tls;
        private TeensyWatcher watcher;

        public frmFirmware()
        {
            InitializeComponent();

            Tls = new clsTools(this);
            watcher = new TeensyWatcher(SynchronizationContext.Current);
            watcher.ConnectedTeensies.CollectionChanged += ConnectedTeensiesChanged;
            foreach (var teensy in watcher.ConnectedTeensies)
            {
                lbTeensies.Items.Add(teensy);
            }
            if (lbTeensies.Items.Count > 0) lbTeensies.SelectedIndex = 0;
        }

        private void bntOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            tbHexfile.Text = "";
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "hex files (*.hex)|*.hex|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 0;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    tbHexfile.Text = openFileDialog.FileName;
                }
            }
        }

        private void btnBrowse_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            string Message = "Search for new firmware (hex) files.";

            Tls.ShowHelp(Message, "Browse");
            hlpevent.Handled = true;
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                string filename = "";
                var teensy = lbTeensies.SelectedItem as ITeensy;
                if (teensy != null)
                {
                    filename = tbHexfile.Text;

                    if (File.Exists(filename))
                    {
                        var progress = new Progress<int>(v => progressBar.Value = v);
                        progressBar.Visible = true;
                        var result = await teensy.UploadAsync(filename, progress);
                        Tls.ShowHelp(result.ToString(), "Message", 3000);
                        progressBar.Visible = false;
                        progressBar.Value = 0;
                    }
                    else
                    {
                        Tls.ShowHelp("File does not exist", "Error", 3000, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Tls.ShowHelp(ex.Message, this.Text, 3000, true);
            }
        }

        private void btnUpload_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            string Message = "Upload new firmware to the Teensy.";

            Tls.ShowHelp(Message, "Upload");
            hlpevent.Handled = true;
        }

        private void ConnectedTeensiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var teensy in e.NewItems)
                    {
                        lbTeensies.Items.Add(teensy);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var teensy in e.OldItems)
                    {
                        lbTeensies.Items.Remove(teensy);
                    }
                    break;
            }
            if (lbTeensies.SelectedIndex == -1 && lbTeensies.Items.Count > 0) lbTeensies.SelectedIndex = 0;
        }

        private void frmFirmware_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                Tls.SaveFormData(this);
            }
            Tls.SaveProperty("LastFile", tbHexfile.Text);
        }

        private void frmFirmware_Load(object sender, EventArgs e)
        {
            Tls.LoadFormData(this);
            this.BackColor = TeensyInstall.Properties.Settings.Default.DayColour;

            string LastFile = Tls.LoadProperty("LastFile");
            if (File.Exists(LastFile)) tbHexfile.Text = LastFile;
        }

        private void lbTeensies_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            string Message = "Teensies connected to a serial (usb) port. " +
                "Select the Teensy to update.";

            Tls.ShowHelp(Message, "Connected Teensies");
            hlpevent.Handled = true;
        }

        private void tbHexfile_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            string Message = "Filename of firmware to upload to the Teensy.";

            Tls.ShowHelp(Message, "Firmware");
            hlpevent.Handled = true;
        }
    }
}