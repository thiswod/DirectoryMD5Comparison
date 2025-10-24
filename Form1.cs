using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DirectoryMD5Comparison
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // 浏览按钮1点击事件
        private void btnBrowse1_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtDirectory1.Text = fbd.SelectedPath;
                }
            }
        }

        // 浏览按钮2点击事件
        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtDirectory2.Text = fbd.SelectedPath;
                }
            }
        }

        // 计算文件MD5值
        private string CalculateMD5(string filePath)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        byte[] hashBytes = md5.ComputeHash(stream);
                        StringBuilder sb = new StringBuilder();
                        foreach (byte b in hashBytes)
                        {
                            sb.Append(b.ToString("x2"));
                        }
                        return sb.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return "计算失败: " + ex.Message;
            }
        }

        // 比较按钮点击事件
        private void btnCompare_Click(object sender, EventArgs e)
        {
            string dir1 = txtDirectory1.Text;
            string dir2 = txtDirectory2.Text;

            // 验证目录是否存在
            if (!Directory.Exists(dir1))
            {
                MessageBox.Show("源目录不存在，请重新选择！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Directory.Exists(dir2))
            {
                MessageBox.Show("目标目录不存在，请重新选择！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 清空之前的结果
            lstResults.Items.Clear();

            // 获取两个目录中的所有文件
            string[] files1 = Directory.GetFiles(dir1, "*.*", SearchOption.AllDirectories);
            string[] files2 = Directory.GetFiles(dir2, "*.*", SearchOption.AllDirectories);

            // 创建文件名到路径的映射
            Dictionary<string, string> fileMap1 = new Dictionary<string, string>();
            foreach (string file in files1)
            {
                string relativePath = file.Substring(dir1.Length).TrimStart('\\');
                fileMap1[relativePath] = file;
            }

            Dictionary<string, string> fileMap2 = new Dictionary<string, string>();
            foreach (string file in files2)
            {
                string relativePath = file.Substring(dir2.Length).TrimStart('\\');
                fileMap2[relativePath] = file;
            }

            // 获取所有文件名
            HashSet<string> allFiles = new HashSet<string>(fileMap1.Keys);
            allFiles.UnionWith(fileMap2.Keys);

            // 比较每个文件
            int differentFiles = 0;
            foreach (string fileName in allFiles)
            {
                string md5_1 = "文件不存在";
                string md5_2 = "文件不存在";

                if (fileMap1.ContainsKey(fileName))
                {
                    md5_1 = CalculateMD5(fileMap1[fileName]);
                }

                if (fileMap2.ContainsKey(fileName))
                {
                    md5_2 = CalculateMD5(fileMap2[fileName]);
                }

                // 添加到结果列表
                ListViewItem item = new ListViewItem(fileName);
                item.SubItems.Add(md5_1);
                item.SubItems.Add(md5_2);

                // 如果MD5不同，高亮显示
                if (md5_1 != md5_2 && md5_1 != "文件不存在" && md5_2 != "文件不存在")
                {
                    item.BackColor = Color.Yellow;
                    differentFiles++;
                }
                // 如果文件只在一个目录中存在，显示为红色
                else if (md5_1 == "文件不存在" || md5_2 == "文件不存在")
                {
                    item.BackColor = Color.LightPink;
                    differentFiles++;
                }

                lstResults.Items.Add(item);
            }

            // 显示比较结果统计
            if (differentFiles == 0)
            {
                MessageBox.Show("两个目录中的文件MD5值完全相同！", "比较结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"发现 {differentFiles} 个不同的文件！", "比较结果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // 清空按钮点击事件
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtDirectory1.Clear();
            txtDirectory2.Clear();
            lstResults.Items.Clear();
        }
    }
}
