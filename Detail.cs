using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RDP_Utility
{
    public partial class Detail : Form
    {
        string hostname;
        VMInformation vmInformation = new VMInformation();
        //define currentpath
        readonly string currentpath = Directory.GetCurrentDirectory();
        string savemode; //define which mode, add or edit
        public delegate void BackFunction();
        public event BackFunction backfunction;
        public Detail(string hostname)
        {
            InitializeComponent();
            this.hostname = hostname;
            vmInformation.Hostname = hostname;
            savemode = "Add";
            if (hostname != null)
            {
                textBoxHostName.ReadOnly = true;
                savemode = "Edit";
                LoadInformationFromFile();
            }
        }
        private void LoadInformationFromFile()
        {
            string rdpfilename = hostname.Replace(":", "-"); // ":" can not use as file name
            try
            {
                StreamReader sr = new StreamReader(currentpath + "\\data\\rdp files\\"+rdpfilename+".rdp");
                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.StartsWith("screen mode"))
                    {
                        vmInformation.Screenmode = line.Substring(line.IndexOf("i:") + 2);
                    }
                    if (line.StartsWith("desktopwidth"))
                    {
                        vmInformation.Desktopwidth = line.Substring(line.IndexOf("i:") + 2);
                    }
                    if (line.StartsWith("desktopheight"))
                    {
                        vmInformation.Desktopheight = line.Substring(line.IndexOf("i:") + 2);
                    }
                    if (line.StartsWith("username"))
                    {
                        vmInformation.Username = line.Substring(line.IndexOf("s:") + 2);
                    }
                    if (line.StartsWith("password 51"))
                    {
                        vmInformation.Encryptpassword = line.Substring(line.IndexOf("b:") + 2);
                    }
                    if (line.StartsWith("noneprotectedpassword"))
                    {
                        vmInformation.Password = line.Substring(line.IndexOf(":") + 1);
                    }
                    line = sr.ReadLine();
                }
                sr.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Read file: " + currentpath + "\\data\\rdp files\\" + hostname + ".rdp failed.", "Warning");
                buttonSave.Enabled = false;
            }
            this.textBoxHostName.Text = vmInformation.Hostname;
            this.textBoxUser.Text= vmInformation.Username;
            this.textBoxPassword.Text = vmInformation.Password;
            if (vmInformation.Screenmode == "2")
            {
                radioButtonFull.Select();
            }
            else if(vmInformation.Desktopwidth=="1400" & vmInformation.Desktopheight=="1050")
            {
                radioButton1400.Select();
            }
            else
            {
                radioButtonCustom.Select();
                textBoxWidth.Text = vmInformation.Desktopwidth;
                textBoxHeight.Text = vmInformation.Desktopheight;  
            }

        }

        private void Detail_Load(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (textBoxHostName.Text == "" | textBoxUser.Text == "" | textBoxPassword.Text == "")
            {
                MessageBox.Show("Didn't input all necessary data. Save failed.","Warning");
                return;
            }
            if (radioButtonCustom.Checked)
            {
                if(textBoxWidth.Text=="" | textBoxHeight.Text =="")
                {
                    MessageBox.Show("Didn't input all necessary data. Save failed.", "Warning");
                    return;
                }
            }
            string rdpfilename = textBoxHostName.Text.Replace(":", "-"); // ":" can not use as file name
            if (savemode=="Add")
            {
                if(File.Exists(currentpath + "\\data\\rdp files\\" + rdpfilename + ".rdp"))
                {
                    MessageBox.Show("The HostName you input is already exist.", "Warning");
                    return;
                }
            }
            if(textBoxPassword.Text != vmInformation.Password)
            {
                vmInformation.Password = textBoxPassword.Text;
                vmInformation.Encryptpassword = PowerShellExc("('"+vmInformation.Password+"' | ConvertTo-SecureString -AsPlainText -Force) | ConvertFrom-SecureString");
            }
            vmInformation.Hostname = textBoxHostName.Text;
            vmInformation.Username = textBoxUser.Text;
            vmInformation.Desktopwidth = "";
            vmInformation.Desktopheight = "";
            if (radioButtonFull.Checked)
            {
                vmInformation.Screenmode = "2";
            }
            else
            {
                vmInformation.Screenmode="1";
            }
            if(radioButton1400.Checked)
            {
                vmInformation.Desktopwidth = "1400";
                vmInformation.Desktopheight = "1050";

            }
            if (radioButtonCustom.Checked)
            {
                vmInformation.Desktopwidth = textBoxWidth.Text;
                vmInformation.Desktopheight = textBoxHeight.Text;
            }
            try
            {
                StreamWriter sw = new StreamWriter(currentpath + "\\data\\rdp files\\" + rdpfilename + ".rdp", false);

                sw.Write("screen mode id:i:" + vmInformation.Screenmode + "\r\n" +
                        "use multimon:i:0\r\n" +
                        "desktopwidth:i:" + vmInformation.Desktopwidth + "\r\n" +
                        "desktopheight:i:" + vmInformation.Desktopheight + "\r\n" +
                        "session bpp:i:32\r\n" +
                        "winposstr:s:0,3,0,0,1126,836\r\n" +
                        "compression:i:1\r\n" +
                        "keyboardhook:i:2\r\n" +
                        "audiocapturemode:i:0\r\n" +
                        "videoplaybackmode:i:1\r\n" +
                        "connection type:i:7\r\n" +
                        "networkautodetect:i:1\r\n" +
                        "bandwidthautodetect:i:1\r\n" +
                        "displayconnectionbar:i:1\r\n" +
                        "enableworkspacereconnect:i:0\r\n" +
                        "disable wallpaper:i:0\r\n" +
                        "allow font smoothing:i:0\r\n" +
                        "allow desktop composition:i:0\r\n" +
                        "disable full window drag:i:1\r\n" +
                        "disable menu anims:i:1\r\n" +
                        "disable themes:i:0\r\n" +
                        "disable cursor setting:i:0\r\n" +
                        "bitmapcachepersistenable:i:1\r\n" +
                        "full address:s:" + vmInformation.Hostname + "\r\n" +
                        "audiomode:i:0\r\n" +
                        "redirectprinters:i:1\r\n" +
                        "redirectcomports:i:0\r\n" +
                        "redirectsmartcards:i:1\r\n" +
                        "redirectclipboard:i:1\r\n" +
                        "redirectposdevices:i:0\r\n" +
                        "autoreconnection enabled:i:1\r\n" +
                        "authentication level:i:2\r\n" +
                        "prompt for credentials:i:0\r\n" +
                        "negotiate security layer:i:1\r\n" +
                        "remoteapplicationmode:i:0\r\n" +
                        "alternate " + "she" + "ll:s:\r\n" +
                        "shell working directory:s:\r\n" +
                        "gatewayhostname:s:\r\n" +
                        "gatewayusagemethod:i:4\r\n" +
                        "gatewaycredentialssource:i:4\r\n" +
                        "gatewayprofileusagemethod:i:0\r\n" +
                        "promptcredentialonce:i:0\r\n" +
                        "gatewaybrokeringtype:i:0\r\n" +
                        "use redirection server name:i:0\r\n" +
                        "rdgiskdcproxy:i:0\r\n" +
                        "kdcproxyname:s:\r\n" +
                        "drivestoredirect:s:\r\n" +
                        "smart sizing:i:1\r\n" +
                        "username:s:" + vmInformation.Username + "\r\n" +
                        "password 51:b:" + vmInformation.Encryptpassword + "\r\n" +
                        "noneprotectedpassword:" + vmInformation.Password + "\r\n");
                sw.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Edit file: " + currentpath + "\\data\\rdp files\\" + rdpfilename + ".rdp failed", "Warning");
            }
            if (savemode == "Add")
            {
                try
                {
                    StreamWriter sw = new StreamWriter(currentpath + "\\data\\VMInformation.txt", true);
                    sw.WriteLine(vmInformation.Hostname + "#" + "");
                    sw.Close();
                }
                catch (Exception)
                {
                    MessageBox.Show("Edit file: " + currentpath + "\\data\\VMInformation.txt failed", "Warning");
                }

                this.backfunction();

            }


            this.Close();


        }
        public string PowerShellExc(string command) //function for PowerShell Exculte
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

    }
}
