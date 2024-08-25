using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security;
using System.Net;
using System.Runtime.InteropServices;
//using System.Management.Automation;
//using System.Management.Automation.Runspaces;
using System.Security.Cryptography;
using System.Diagnostics;

namespace RDP_Utility
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            RefreshVMList();
            //MessageBox.Show(CheckFileExit("E:\\RDP_Utility\\RDP_Utility\\bin\\Debug\\data\\test\\VMInformation1.txt", 1).ToString(),"Warning");
        }
        //define currentpath
        readonly string currentpath = Directory.GetCurrentDirectory();
        //define if vmlist is refreshing, this value is 1.
        int vmlistrefreshing = 1;
        //create list to store VMinformation
        List<VMInformation> vminformations = new List<VMInformation>();
        //refreshVMlist when page load or any data modified
        readonly string logfilePath = Directory.GetCurrentDirectory() + "\\data\\log.txt";
        private void RefreshVMList()
        {
            //make sure value changed sub didn't work
            vmlistrefreshing = 1;
            //check information file exist
            CheckFileExit(currentpath + "\\data\\VMInformation.txt", 1) ;
            vminformations.Clear();
            
            try
            {
                StreamReader sr = new StreamReader(currentpath + "\\data\\VMInformation.txt");
                string line = sr.ReadLine();
                while (line != null)
                {
                    VMInformation vminformation = new VMInformation();
                    vminformation.Hostname = "";
                    vminformation.Comment = "";
                    vminformation.Hostname = line.Substring(0, line.IndexOf("#"));
                    vminformation.Comment = line.Substring(line.IndexOf("#") +1);
                    vminformations.Add(vminformation);
                    line = sr.ReadLine();
                }
                sr.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Read file: "+ currentpath + "\\data\\VMInformation.txt failed", "Warning");
            }
            dataGridViewVMList.Rows.Clear();
            foreach (VMInformation vm in vminformations)
            {
                string[] str = new string[] { vm.Hostname, vm.Comment };
                dataGridViewVMList.Rows.Add(str);
            }
            vmlistrefreshing = 0;
        }
        //write current VMlist information to file
        private void WriteVMInformationToFile()
        {
            try
            {
                StreamWriter sw = new StreamWriter(currentpath + "\\data\\VMInformation.txt", false);
                foreach (VMInformation vm in vminformations)
                {
                    sw.WriteLine(vm.Hostname + "#" + vm.Comment);
                }
                sw.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Edit file: " + currentpath + "\\data\\VMInformation.txt failed", "Warning");
            }
        }
            //checkmode: 0-> only check if file exit; 1-> if not exit, create it.
            //return value result: 1-> exit; 2-> not exit and didn't create; 3-> not exit and created; 4-> not exit and created failed.
            private int CheckFileExit(string path, int checkmode)
        {
            int result=0;
            switch (checkmode)
            {
                case 0:
                    if(File.Exists(path))
                    {
                        result = 1;
                    }
                    else
                    {
                        result = 2;
                    }
                    break;
                case 1:
                    if (File.Exists(path))
                    {
                        result = 1;
                    }
                    else
                    {
                        try
                        {
                            if(!Directory.Exists(currentpath + "\\data"))
                            {
                                Directory.CreateDirectory(currentpath + "\\data");
                            }
                            File.Create(path).Close();
                            File.Create(logfilePath).Close();
                            result = 3;
                        }catch (Exception)
                        {
                            result = 4;
                            MessageBox.Show("Crete file: " + path + " failed!", "Warning");
                        }
                    }
                    break;
            }
            return result;
        }
        public void Backfunction()
        {
            RefreshVMList();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            Detail detail = new Detail(null);
            detail.backfunction += Backfunction;
            detail.Show();

        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            ConnectVM();
        }
        
        private void ConnectVM()
        {
            //string hostname = vminformations[dataGridViewVMList.CurrentRow.Index].Hostname;
            string hostname = dataGridViewVMList.Rows[dataGridViewVMList.CurrentRow.Index].Cells[0].Value.ToString();
            string rdpfilename = hostname.Replace(":", "-"); // ":" can not use as file name
            string path = currentpath + "\\data\\rdp files\\" + rdpfilename + ".rdp";
            try
            {
                Process.Start(path);

            }
            catch (Exception)
            {
                MessageBox.Show("Open file: " + path + " failed", "Warning");
            }

        }

        private void dataGridViewVMList_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (vmlistrefreshing==0)
            {
                //if(string.IsNullOrEmpty(dataGridViewVMList.CurrentCell.Value.ToString()))
                //{
                //    vminformations[dataGridViewVMList.CurrentRow.Index].Comment = dataGridViewVMList.CurrentCell.Value.ToString();
                //}
                //else
                //{
                //    vminformations[dataGridViewVMList.CurrentRow.Index].Comment = "";
                //}
                vminformations[dataGridViewVMList.CurrentRow.Index].Comment = Convert.ToString(dataGridViewVMList.CurrentCell.Value);
                WriteVMInformationToFile();
                WriteLog(vminformations[dataGridViewVMList.CurrentRow.Index].Hostname, $"Change comment to: [{Convert.ToString(dataGridViewVMList.CurrentCell.Value)}]");
                MessageBox.Show("Comment was changed successfully.", "Information");
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            Detail detail = new Detail(vminformations[dataGridViewVMList.CurrentRow.Index].Hostname);
            detail.backfunction += Backfunction;
            detail.Show();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            string hostname = vminformations[dataGridViewVMList.CurrentRow.Index].Hostname;
            DialogResult messageboxResult = MessageBox.Show($"Are you sure you want to delete the RDP of: [{hostname}]. ?", "Confirm", MessageBoxButtons.YesNo);
            if (messageboxResult == System.Windows.Forms.DialogResult.No)
            {
                return;
            }
            foreach (VMInformation vm in vminformations)
            {
                if (vm.Hostname == hostname)
                {
                    vminformations.Remove(vm);
                    break;
                }
            }
            string rdpfilename = hostname.Replace(":", "-"); // ":" can not use as file name
            try
            {
                File.Delete(currentpath + "\\data\\rdp files\\" + rdpfilename + ".rdp");
            }
            catch(Exception)
            {
                MessageBox.Show("Delete file: \" + currentpath + \"\\\\data\\\\rdp files\\\\\" + rdpfilename + \".rdp failed", "Warning");
            }
            WriteVMInformationToFile();
            RefreshVMList();
            WriteLog(hostname, $"Deleted.");

        }

        public string PowerShellExc(string command)
        {
            try
            {
                Process mProcess = new Process();
                mProcess.StartInfo.UseShellExecute = false;
                mProcess.StartInfo.RedirectStandardOutput = true;
                mProcess.StartInfo.FileName = @"powershell.exe";
                mProcess.StartInfo.Arguments = command;
                mProcess.Start();

                string sResult = mProcess.StandardOutput.ReadToEnd();
                mProcess.WaitForExit();

                return sResult;
            }
            catch (Exception e)
            {
                using (StreamWriter outfile = new StreamWriter("PowerShellResult.txt", true))
                {
                    return "Error: " + e.Message;
                }
            }

        }

        private void pictureBoxLogFile_Click(object sender, EventArgs e)
        {
            if (!File.Exists(logfilePath))
            {
                File.Create(logfilePath).Close();
            }
            try
            {
                Process.Start(logfilePath);
            }
            catch (Exception)
            {
                MessageBox.Show("Open log file failed.", "Warning");
            }
        }

        public void WriteLog(string hostName,string message)
        {
            string time = System.DateTime.Now.ToString("yyyy-MM-dd HH：mm：ss");
            string s = File.ReadAllText(logfilePath);
            string logline = $"{time}\t{hostName}\t{message}";
            s = s.Insert(0, logline + "\r\n");
            File.WriteAllText(logfilePath, s);
        }

        private void pictureBoxUp_Click(object sender, EventArgs e)
        {
            pictureBoxUp.Enabled = false;
            pictureBoxDown.Enabled = false;
            pictureBoxTop.Enabled = false;
            pictureBoxBottom.Enabled = false;
            int selectIndex = dataGridViewVMList.CurrentRow.Index;
            int scrollIndex = dataGridViewVMList.FirstDisplayedScrollingRowIndex;
            if (selectIndex <= 0) 
            {
                pictureBoxUp.Enabled = true;
                pictureBoxDown.Enabled = true;
                pictureBoxTop.Enabled = true;
                pictureBoxBottom.Enabled = true;
                return;
            }
            string hostnameChange = vminformations[dataGridViewVMList.CurrentRow.Index].Hostname;
            string path = currentpath + "\\data\\VMInformation.txt";
            List<string> readFile = File.ReadAllLines(path).ToList();
            List<string> writeFile = new List<string>();
            foreach (string line in readFile)
            {
                string currenthostname = line.Substring(0, line.IndexOf("#"));
                if(currenthostname==hostnameChange)
                {
                    string cachline = writeFile.Last();
                    writeFile.RemoveAt(writeFile.Count - 1);
                    writeFile.Add(line);
                    writeFile.Add(cachline);
                }
                else
                {
                    writeFile.Add(line);
                }
            }
            File.WriteAllLines(path, writeFile);
            RefreshVMList();
            dataGridViewVMList.FirstDisplayedScrollingRowIndex = scrollIndex;
            dataGridViewVMList.CurrentCell = dataGridViewVMList.Rows[selectIndex-1].Cells[0];
            pictureBoxUp.Enabled = true;
            pictureBoxDown.Enabled = true;
            pictureBoxTop.Enabled = true;
            pictureBoxBottom.Enabled = true;

        }

        private void pictureBoxDown_Click(object sender, EventArgs e)
        {
            pictureBoxUp.Enabled = false;
            pictureBoxDown.Enabled = false;
            pictureBoxTop.Enabled = false;
            pictureBoxBottom.Enabled = false;
            int selectIndex = dataGridViewVMList.CurrentRow.Index;
            int scrollIndex = dataGridViewVMList.FirstDisplayedScrollingRowIndex;
            if (selectIndex >= dataGridViewVMList.Rows.Count - 1) 
            {
                pictureBoxUp.Enabled = true;
                pictureBoxDown.Enabled = true;
                pictureBoxTop.Enabled = true;
                pictureBoxBottom.Enabled = true;
                return;
            }
            string hostnameChange = vminformations[dataGridViewVMList.CurrentRow.Index].Hostname;
            string path = currentpath + "\\data\\VMInformation.txt";
            List<string> readFile = File.ReadAllLines(path).ToList();
            List<string> writeFile = new List<string>();
            string cachline = "";
            foreach (string line in readFile)
            {
                string currenthostname = line.Substring(0, line.IndexOf("#"));

                if (currenthostname == hostnameChange)
                {
                    cachline = line;
                }
                else
                {
                    writeFile.Add(line);
                    if (cachline != "")
                    {
                        writeFile.Add(cachline);
                        cachline = "";
                    }
                }
            }
            File.WriteAllLines(path, writeFile);
            RefreshVMList();
            dataGridViewVMList.FirstDisplayedScrollingRowIndex = scrollIndex;
            dataGridViewVMList.CurrentCell = dataGridViewVMList.Rows[selectIndex + 1].Cells[0];
            pictureBoxUp.Enabled = true;
            pictureBoxDown.Enabled = true;
            pictureBoxTop.Enabled = true;
            pictureBoxBottom.Enabled = true;
        }

        private void pictureBoxTop_Click(object sender, EventArgs e)
        {
            pictureBoxUp.Enabled = false;
            pictureBoxDown.Enabled = false;
            pictureBoxTop.Enabled = false;
            pictureBoxBottom.Enabled = false;
            int selectIndex = dataGridViewVMList.CurrentRow.Index;
            int scrollIndex = dataGridViewVMList.FirstDisplayedScrollingRowIndex;
            if (selectIndex <= 0)
            {
                pictureBoxUp.Enabled = true;
                pictureBoxDown.Enabled = true;
                pictureBoxTop.Enabled = true;
                pictureBoxBottom.Enabled = true;
                return;
            }
            string hostnameChange = vminformations[dataGridViewVMList.CurrentRow.Index].Hostname;
            string path = currentpath + "\\data\\VMInformation.txt";
            List<string> readFile = File.ReadAllLines(path).ToList();
            List<string> writeFile = new List<string>();
            string cachline = "";
            foreach (string line in readFile)
            {
                string currenthostname = line.Substring(0, line.IndexOf("#"));
                if (currenthostname == hostnameChange)
                {
                    cachline = line;
                }
            }
            writeFile.Add(cachline);
            foreach (string line in readFile)
            {
                string currenthostname = line.Substring(0, line.IndexOf("#"));
                if (currenthostname != hostnameChange)
                {
                    writeFile.Add(line);
                }
            }
            File.WriteAllLines(path, writeFile);
            RefreshVMList();
            dataGridViewVMList.FirstDisplayedScrollingRowIndex = scrollIndex;
            dataGridViewVMList.CurrentCell = dataGridViewVMList.Rows[0].Cells[0];
            pictureBoxUp.Enabled = true;
            pictureBoxDown.Enabled = true;
            pictureBoxTop.Enabled = true;
            pictureBoxBottom.Enabled = true;
        }

        private void pictureBoxBottom_Click(object sender, EventArgs e)
        {
            pictureBoxUp.Enabled = false;
            pictureBoxDown.Enabled = false;
            pictureBoxTop.Enabled = false;
            pictureBoxBottom.Enabled = false;
            int selectIndex = dataGridViewVMList.CurrentRow.Index;
            int scrollIndex = dataGridViewVMList.FirstDisplayedScrollingRowIndex;
            if (selectIndex >= dataGridViewVMList.Rows.Count - 1)
            {
                pictureBoxUp.Enabled = true;
                pictureBoxDown.Enabled = true;
                pictureBoxTop.Enabled = true;
                pictureBoxBottom.Enabled = true;
                return;
            }
            string hostnameChange = vminformations[dataGridViewVMList.CurrentRow.Index].Hostname;
            string path = currentpath + "\\data\\VMInformation.txt";
            List<string> readFile = File.ReadAllLines(path).ToList();
            List<string> writeFile = new List<string>();
            string cachline = "";
            foreach (string line in readFile)
            {
                string currenthostname = line.Substring(0, line.IndexOf("#"));
                if (currenthostname == hostnameChange)
                {
                    cachline = line;
                }
                else
                {
                    writeFile.Add(line);
                }
            }
            writeFile.Add(cachline);
            File.WriteAllLines(path, writeFile);
            RefreshVMList();
            dataGridViewVMList.FirstDisplayedScrollingRowIndex = scrollIndex;
            dataGridViewVMList.CurrentCell = dataGridViewVMList.Rows[dataGridViewVMList.Rows.Count - 1].Cells[0];
            pictureBoxUp.Enabled = true;
            pictureBoxDown.Enabled = true;
            pictureBoxTop.Enabled = true;
            pictureBoxBottom.Enabled = true;

        }

        private void buttonDebug_Click(object sender, EventArgs e)
        {
            int scrollIndex = dataGridViewVMList.FirstDisplayedScrollingRowIndex;
            MessageBox.Show(scrollIndex.ToString());
            dataGridViewVMList.FirstDisplayedScrollingRowIndex = 10;

        }

        private void dataGridViewVMList_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0) 
            {
                ConnectVM();
            }
        }
    }

    //Define class for VM information
    public class VMInformation
    {
        //VM basic information
        private string hostname;
        private string username;
        private string password;
        private string screenmode;
        private string desktopwidth;
        private string desktopheight;
        //encryptpassword by cmd command
        private string encryptpassword;
        //comment is input by user
        private string comment;
        public string Hostname
        {
            get { return hostname; }
            set { hostname = value; }
        }
        public string Username
        {
            get { return username; }
            set { username = value; }
        }
        public string Screenmode
        {
            get { return screenmode; }
            set { screenmode = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }
        public string Encryptpassword
        {
            get { return encryptpassword; }
            set { encryptpassword = value; }
        }
        public string Comment
        {
            get { return comment; }
            set { comment = value; }
        }
        public string Desktopwidth
        {
            get { return desktopwidth; }
            set { desktopwidth = value; }
        }
        public string Desktopheight
        {
            get { return desktopheight; }
            set { desktopheight = value; }
        }
    }
}
