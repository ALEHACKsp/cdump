using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace cdump
{
    public partial class Form1 : Form
    {
        [DllImport("pd.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern string doShit(string exe, string path);

        public Form1()
        {
            InitializeComponent();
            processView.Columns.Add("Process Name", -1, HorizontalAlignment.Left);
            processView.Columns.Add("Window Name", -2, HorizontalAlignment.Right);
            RefreshProcesses();
        }

        void RefreshProcesses()
        {
            string search = processBox.Text;
            processView.Items.Clear();
            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    if (process.MainWindowTitle.Contains(search) || process.MainModule.ModuleName.Contains(search))
                    {
                        var item1 = new ListViewItem(new[] {process.MainModule.ModuleName, process.MainWindowTitle});
                        processView.Items.Add(item1);
                    }
                } catch (Exception) { }
            }
            this.ResizeColumnHeaders();
            CheckProcess();
        }

        void CheckProcess()
        {
            
            this.checkBox1.Checked = this.dumpButton.Enabled = (processBox.Text.Length > 4) && (Process.GetProcessesByName(processBox.Text.Substring(0, processBox.Text.Length-4)).Length > 0);
            processBox.BackColor = (this.checkBox1.Checked) ? Color.LightGreen : Color.LightCoral;
            statusLabel.Text = (this.checkBox1.Checked) ? "Ready to Dump" : "Select a Process";
        }

        private void processBox_TextChanged(object sender, EventArgs e)
        {
            RefreshProcesses();
        }

        private void processView_ItemActivate(object sender, EventArgs e)
        {
            if (processView.SelectedItems.Count == 1)
            {
                processBox.Text = processView.SelectedItems[0].Text;
            }
        }

        private void ResizeColumnHeaders()
        {
            for (int i = 0; i < this.processView.Columns.Count - 1; i++) this.processView.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
            this.processView.Columns[this.processView.Columns.Count - 1].Width = -2;
        }

        public void InvokeUI(Action a)
        {
            this.BeginInvoke(new MethodInvoker(a));
        }

        void doDump(string procName)
        {
            if (!File.Exists("pd.dll"))
            {
                File.WriteAllBytes("pd.dll", cdump.Properties.Resources.pd);
            }

            string currentPath = Path.GetDirectoryName(Application.ExecutablePath);
            string dumpPath = currentPath + @"\dumps";
            string dumpProcPath = dumpPath + @"\" + procName.Substring(0, procName.Length - 4);
            string dumpPathFull = dumpProcPath + @"\" + DateTime.Now.ToString("yyyy-M-dd HH-mm-ss");

            if (!Directory.Exists(dumpPath))
            {
                Directory.CreateDirectory(dumpPath);
            }
            if (!Directory.Exists(dumpProcPath))
            {
                Directory.CreateDirectory(dumpProcPath);
            }

            if (!Directory.Exists(dumpPathFull))
            {
                Directory.CreateDirectory(dumpPathFull);
            }

            doShit(procName, dumpPathFull);
            InvokeUI(() => {
                dumpButton.Enabled = processBox.Enabled = processView.Enabled = true;
                statusLabel.Text = "Dump Complete";
                processBox.BackColor = Color.LightGreen;
            });
        }

        private void dumpButton_Click(object sender, EventArgs e)
        {
            dumpButton.Enabled = processBox.Enabled = processView.Enabled = false;
            processBox.BackColor = Color.LightBlue;
            statusLabel.Text = "Dumping, please wait . . .";
            Thread thread = new Thread(() => doDump(processBox.Text));
            thread.Start();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            this.ResizeColumnHeaders();
        }
    }
}
