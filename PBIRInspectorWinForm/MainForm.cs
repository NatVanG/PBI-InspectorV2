using PBIRInspectorWinLibrary;
using PBIRInspectorWinLibrary.Utils;

namespace PBIRInspectorWinForm
{
    public partial class MainForm : Form
    {
        
        public MainForm()
        {
            InitializeComponent();
            this.Text = AppUtils.About();
            this.FormClosing += MainForm_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtConsoleOutput.Clear();
            Main.WinMessageIssued += Main_MessageIssued;
            UseSamplePBIFileStateCheck();
            UseBaseRulesCheck();
            UseTempFilesStateCheck();
            txtPBIDesktopFile.Focus();
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            Clear();
        }

        private void Main_MessageIssued(object? sender, PBIRInspectorLibrary.MessageIssuedEventArgs e)
        {
            if (e.MessageType == PBIRInspectorLibrary.MessageTypeEnum.Dialog)
            {
                var dr = MessageBox.Show(e.Message, "Delete directory?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    e.DialogOKResponse = true;
                }
            }
            else
            {
                txtConsoleOutput.AppendText(string.Concat(e.MessageType.ToString(), ": ", e.Message, "\r\n"));
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btnBrowsePBIDesktopFile_Click(object sender, EventArgs e)
        {
            this.openPBIDesktopFileDialog.ShowDialog(this);
        }

        private void btnBrowseRulesFile_Click(object sender, EventArgs e)
        {
            this.openRulesFileDialog.ShowDialog(this);
        }

        private void btnBrowseOutputDir_Click(object sender, EventArgs e)
        {
            if (this.outputFolderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                this.txtOutputDirPath.Text = this.outputFolderBrowserDialog.SelectedPath;
            }
        }

        private void UseSamplePBIFileStateCheck()
        {
            var enabled = !this.chckUseSamplePBIFile.Checked;
            if (!enabled) { this.txtPBIDesktopFile.Text = Constants.SamplePBIPReportFolderPath; } else { this.txtPBIDesktopFile.Clear(); };
            this.txtPBIDesktopFile.Enabled = enabled;
            this.btnBrowsePBIDesktopFile.Enabled = enabled;
            //this.chckVerbose.Checked = !enabled;
        }

        private void chckUseSamplePBIFile_CheckedChanged(object sender, EventArgs e)
        {
            UseSamplePBIFileStateCheck();
        }


        private void UseBaseRulesCheck()
        {
            var enabled = !this.chkUseBaseRules.Checked;
            if (!enabled) { this.txtRulesFilePath.Text = Constants.SampleRulesFilePath; } else { this.txtRulesFilePath.Clear(); }
            this.txtRulesFilePath.Enabled = enabled;
            this.btnBrowseRulesFile.Enabled = enabled;
        }

        private void chkUseBaseRules_CheckedChanged(object sender, EventArgs e)
        {
            UseBaseRulesCheck();
        }

        private void UseTempFilesStateCheck()
        {
            var enabled = !this.chckUseTempFiles.Checked;
            this.txtOutputDirPath.Clear();
            this.txtOutputDirPath.Enabled = enabled;
            this.btnBrowseOutputDir.Enabled = enabled;
        }

        private void chckUseTempFiles_CheckedChanged(object sender, EventArgs e)
        {
            UseTempFilesStateCheck();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            Clear();

            btnRun.Enabled = false;

            var pbiFilePath = this.txtPBIDesktopFile.Text;
            var rulesFilePath = this.txtRulesFilePath.Text;
            var outputPath = this.txtOutputDirPath.Text;
            var verbose = this.chckVerbose.Checked;
            var jsonOutput = this.chckJsonOutput.Checked;
            var htmlOutput = this.chckHTMLOutput.Checked;

            Main.Run(pbiFilePath, rulesFilePath, outputPath, verbose, jsonOutput, htmlOutput);

            btnRun.Enabled = true;
        }

        internal void Clear()
        {
            txtConsoleOutput.Clear();
            Main.CleanUp();
        }

        private void openPBIDesktopFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.txtPBIDesktopFile.Text = this.openPBIDesktopFileDialog.FileName;
        }

        private void openRulesFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.txtRulesFilePath.Text = this.openRulesFileDialog.FileName;
        }

        private void outputFolderBrowserDialog_HelpRequest(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void txtOutputDirPath_TextChanged(object sender, EventArgs e)
        {
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void lnkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                AppUtils.WinOpen(Constants.ReadmePageUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open link that was clicked.");
            }
        }

        private void lnkLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                AppUtils.WinOpen(Constants.LicensePageUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open link that was clicked.");
            }
        }

        private void lnkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(AppUtils.About());
        }

        private void lnkLatestRelease_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                AppUtils.WinOpen(Constants.LatestReleasePageUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open link that was clicked.");
            }
        }

        private void lnkReportIssue_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                AppUtils.WinOpen(Constants.IssuesPageUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open link that was clicked.");
            }
        }
    }
}