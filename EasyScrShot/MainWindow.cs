﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace EasyScrShot
{
    public partial class MainWindow : Form
    {
        string[] result;
        List<Frame> fList;
        int N;
        Info fromInfo;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                GetPNG();
                if (N > 0)
                {
                    DecidingInfo();
                    MatchPNG();
                }
                else
                    goButton.Enabled = false;
            } catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            

        }

        // processing methods

        void GetPNG()
        {
            result = Directory.GetFiles(Utility.currentDir, "*.png");
            InfoBoard.Text = InfoBoard.Text + "当前目录有 " + result.Length.ToString() + " 张 PNG 图片。\n";
            if (result.Length % 2 == 1)
            { 
                InfoBoard.Text += "奇数张图没法继续啊" + Utility.GetHelplessEmotion() + "\n";
                N = 0;
            }
            else
            { 
                for (int i=0; i<result.Length; i++)
                    result[i] = result[i].Remove(0,Utility.currentDir.Length);
                N = result.Length / 2;
            }
        }

        void DecidingInfo()
        {
            bool flag = true;
            foreach (string str in result)
                if (str.IndexOf(".vpy") == -1)
                {
                    flag = false;
                    break;
                }
            if (flag) //from vpy
            {
                var popup = new VSInfoWindow();
                popup.ShowDialog();
                fromInfo = (Info) popup.result.Clone();
                popup.Dispose();
            }
            else
            {
                fromInfo = new AVSInfo();
            }
        }

        void MatchPNG()
        {
            fList = new List<Frame>();
            int k = 0;
            bool flag = true;
            for (int i=0; i<result.Length; i++)
                if (fromInfo.IsSource(result[i]))
                {
                    string Id = fromInfo.GetIndex(result[i]);
                    flag = true;
                    for (int j = 0; j < result.Length; j++)
                        if (j!=i && fromInfo.IsRipped(result[j],Id))
                        {
                            fList.Add(new Frame(Id,result[i],result[j]));
                            k++;
                            flag = false;
                            break;
                        }
                    if (flag)
                    {
                        InfoBoard.Text += result[i] + " 没找到对应的成品截图, 帧数检测为: " + Id + "\n";
                        break;
                    }
                }
            if (!flag) foreach (Frame tmp in fList)
                InfoBoard.Text += tmp.srcName + " -> " + tmp.ripName + "\n";
            if (k<N || flag)
            {
                InfoBoard.Text += "没能找到所有 "+N.ToString() + " 组配对...";
                N = 0;
                goButton.Enabled = false;
            }
        }

        void generateCode()
        {
            fList.Sort();
            string path = Utility.currentDir + "url.txt";
            string url = "http://img.2222.moe/images/" + DateTime.Today.ToString("yyyy/MM/dd/");
            using (StreamWriter file = new StreamWriter(path, false))
            {
                file.WriteLine("Comparison (right click on the image and open it in a new tab to see the full-size one)");
                file.WriteLine("Source________________________________________________Encode");
                file.WriteLine();
                for (int i = 0; i < N; i++)
                {
                    String src = url + fList[i].srcName,
                           rip = url + fList[i].ripName,
                           tbl = url + fList[i].frameId + "s.png";
                    file.WriteLine("[URL={1}][IMG]{0}[/IMG][/URL] [URL={2}][IMG]{0}[/IMG][/URL]", tbl, src, rip);
                }
            }
            MessageBox.Show("截图代码已经写在url.txt里", "去丢发布组吧" + Utility.GetHappyEmotion());
        }

        // event definition

        private void goButton_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < N; i++)
                {
                    fList[i].Resize();
                    fList[i].Rename();
                }
                holdButton.Text = "走人";
                MessageBox.Show("你现在可以继续上传图片了","搞定" + Utility.GetHappyEmotion());
                
            }
            catch (Exception ex) {
                MessageBox.Show("把下面这段截图给LP:\n\n"+ex.ToString(), "搞不定啊"+Utility.GetHelplessEmotion());
            }
            goButton.Enabled = false;
            uploadButton.Enabled = true;
        }

        private void holdButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            generateCode();
            uploadButton.Enabled = false;
        }
    }
    
}