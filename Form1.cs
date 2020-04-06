using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using DiscordRPC;

namespace CDi_Launcher
{
    public partial class MainWindow : Form
    {
        public DiscordRpcClient client;
        Process process = new Process();
        int seconds = 0;
        TimeSpan time;
        string game = "Idle";
        string timeToString;
        bool disablingRPC = false;
        string status = "Idle";

        private int CDiBiosChecker()
        {
            if (File.Exists("setup.txt"))
            {
                string readText = System.IO.File.ReadAllText("setup.txt");
                if (readText.Substring(readText.Length - 3) == "Q==")
                {
                    if (File.Exists("roms/cdimono1.zip") && File.Exists("roms/cdimono1/cdibios.zip"))
                    {
                        return 1;
                    } else
                    {
                        var yes = System.Convert.FromBase64String(readText);
                        JObject o = JObject.Parse(System.Text.Encoding.UTF8.GetString(yes));
                        // o["download1"].ToString();
                        WebClient web = new WebClient();
                        Directory.CreateDirectory("roms");
                        web.DownloadFile(o["download1"].ToString(), "roms/cdimono1.zip");
                        Console.WriteLine(o["download1"].ToString());
                        Directory.CreateDirectory("roms/cdimono1");
                        web.DownloadFile(o["download2"].ToString(), "roms/cdimono1/cdibios.zip");
                        Console.WriteLine(o["download2"].ToString());
                        return 1;
                    }
                } else {
                    System.Windows.Forms.MessageBox.Show("In setup.txt, please add \"Q==\" (without quotes, and in capitals) at the end of the file.");
                }
            } else {
                using (StreamWriter sw = File.AppendText("setup.txt"))
                { // Q==
                    sw.Write("e2Rvd25sb2FkMToiaHR0cHM6Ly9hcmNoaXZlLm9yZy9kb3dubG9hZC9NYW1lMTcyYXJjZGNtcGx0L2NkaW1vbm8xLnppcCIsZG93bmxvYWQyOiJodHRwczovL2FyY2hpdmUub3JnL2Rvd25sb2FkL01hbWUxNzJhcmNkY21wbHQvY2RpYmlvcy56aXAif");
                    System.Windows.Forms.MessageBox.Show("In setup.txt, please add \"Q==\" (without quotes, and in capitals) at the end of the file.");
                    return 0;
                    // new Setup().Show();
                }
            }
            return 0;
        }

        void RPCinit()
        {
            client = new DiscordRpcClient("696311197024256060");
            timeToString = time.ToString(@"mm\:ss");
            client.OnReady += (sender, e) =>
            {
                Console.WriteLine("Received Ready from user {0}", e.User.Username);
            };
            client.OnPresenceUpdate += (sender, e) =>
            {
                // Console.WriteLine("Received Update! {0}", e.Presence);
            };
            client.Initialize();
            client.SetPresence(new RichPresence()
            {
                Details = game,
                State = status,
                Assets = new Assets()
                {
                    LargeImageKey = "rpc-icon"
                }
            });
        }

        public MainWindow()
        {
            InitializeComponent();
            CDiBiosChecker();
            RPCinit();
        }

        private void FileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(1);
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CDiBiosChecker() == 1)
            {
                Process[] pname = Process.GetProcessesByName("mame64");
                if (pname.Length > 0)
                {
                    System.Windows.Forms.MessageBox.Show("Please close other instances of MAME and try again.");
                } else
                {
                    if (File.Exists("mame64.exe"))
                    {
                        using (OpenFileDialog openFileDialog = new OpenFileDialog())
                        {
                            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
                            openFileDialog.Filter = "CHD files (*.chd)|*.chd|All files (*.*)|*.*";
                            openFileDialog.FilterIndex = 1;
                            openFileDialog.RestoreDirectory = true;

                            if (openFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                game = openFileDialog.SafeFileName;
                                status = timeToString + " elapsed";
                                seconds = 0;
                                process.StartInfo.FileName = "mame64.exe";
                                process.StartInfo.Arguments = "cdimono1 -cdrom " + '"' + openFileDialog.FileName + '"';
                                process.StartInfo.UseShellExecute = false;
                                process.StartInfo.CreateNoWindow = true;
                                openToolStripMenuItem.Enabled = false;
                                process.Start();
                            }
                        }
                    } else
                    {
                        System.Windows.Forms.MessageBox.Show("Please add mame64.exe to folder the launcher is in and try again.");
                    }
                }
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            seconds++;
            time = TimeSpan.FromSeconds(seconds);

            if (seconds > 3599)
            {
                timeToString = time.ToString(@"hh\:mm\:ss");
            } else
            {
                timeToString = time.ToString(@"mm\:ss");
            }

            if (client == null) {}
            else
            {
                if (disablingRPC == false)
                {
                    client.SetPresence(new RichPresence()
                    {
                        Details = game,
                        State = status,
                        Assets = new Assets()
                        {
                            LargeImageKey = "rpc-icon"
                        }
                    });
                }
            }
            Process[] pname = Process.GetProcessesByName("mame64");
            if (pname.Length == 0)
            {
                openToolStripMenuItem.Enabled = true;
                game = "Idle";
                status = "Idle";
            }
            if (status != "Idle")
            {
                status = timeToString + " elapsed";
            }
        }

        private void DiscordRichPresenceToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (discordRichPresenceToolStripMenuItem.Checked == false)
            {
                disablingRPC = true;
                client.Dispose();
            } else
            {
                disablingRPC = false;
                RPCinit();
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("CDi-Launcher v1.0\nSupports CHD rom files\nMade by ArcadeGamer1929\nAvailable at: https://GitHub.com/ArcadeGamer1929/CDi-Launcher/releases" +
            "\n\nThe Philips CD-i and MAME trademarks belong to their owners respectively");
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Process[] pname = Process.GetProcessesByName("mame64");
            if (pname.Length == 0) {}
            else
            {
                process.Kill();
            }
            Console.WriteLine("closing");
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

        }
    }
}
