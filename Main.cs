using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace EarningsPattern2
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        //List<Pattern.Report> reports;
        private void Main_Load(object sender, EventArgs e)
        {
            listView1.Columns.Add("No.");
            listView1.Columns.Add("nEmployees");
            listView1.Columns.Add("nIdle");
            listView1.Columns.Add("nNewProjects");
            listView1.Columns.Add("nProjects");
            listView1.Columns.Add("nProducts");
            listView1.Columns.Add("Cost");
            listView1.Columns.Add("Revenue");
            listView1.Columns.Add("Balance");
            foreach (ColumnHeader hear in listView1.Columns) hear.TextAlign = HorizontalAlignment.Right;
            //Run();
            listViewItems = new List<ListViewItem>();
        }

        Pattern pattern;
        List<ListViewItem> listViewItems;

        void Run()
        {
            //reports = new List<Pattern.Report>();
            //const int nMaxProjects = 9;
            //int n = 0;
            //int[] actions = new int[100];
            //for (int i = 0; i <= 48; i++)
            //{
            //        actions[i] = i;
            //}
            //actions[0] = 1;
            //actions[1] = 0;
            //actions[2] = 0;
            //actions[3] = 0;
            //actions[4] = 0;
            //actions[5] = 0;

            //while (true)
            //{
            //    try
            //    {
            //        Debug.WriteLine("====================================");
            //        Pattern pattern = new Pattern();
            //        pattern.OnMonthlyReport += new MonthlyReportEventHandler(pattern_OnMonthlyReport);
            //        for (int i = 0; i <= 36; i++)
            //        {
            //            n = i;
            //            pattern.MonthID = i;
            //        }
            //        break;
            //    }
            //    catch (MyException)
            //    {
            //        if (actions[n] == 0)
            //        {
            //            while (actions[n] == 0)
            //            {
            //                //actions[n] = nMaxProjects;
            //                n--;
            //                if (n < 0) throw new ApplicationException("No solution!");
            //            }
            //            actions[n]--;
            //        }
            //        else
            //        {
            //            actions[n]--;
            //        }
            //    }
            //}

            //Pattern pattern2 = new Pattern(actions);
            //for (int i = 0; i <= 36; i++)
            //{
            //    pattern2.MonthID = i;
            //    Pattern.Report r = pattern2.MonthlyReport;
            //    ListViewItem item = new ListViewItem(r.MonthID.ToString());
            //    item.SubItems.Add(r.nEmployees.ToString());
            //    item.SubItems.Add(r.nProjects.ToString());
            //    item.SubItems.Add(r.nProducts.ToString());
            //    item.SubItems.Add(String.Format("{0:C}", r.MonthlyCost));
            //    item.SubItems.Add(String.Format("{0:C}", r.MonthlyRevenue));
            //    item.SubItems.Add(String.Format("{0:C}", r.Balance)).ForeColor = r.Balance > 0 ? SystemColors.WindowText : Color.Red;
            //    listView1.Items.Add(item);
            //}
            int nMonths = Int32.Parse(textBox1.Text);
            pattern = new Pattern(nMonths);
            pattern.OnMonthlyReport +=new MonthlyReportEventHandler(pattern_OnMonthlyReport);
            pattern.Run2();
        }

        void pattern_OnMonthlyReport(Report r)
        {
            ListViewItem item = new ListViewItem(r.MonthID.ToString());
            item.SubItems.Add(r.nEmployees.ToString());
            item.SubItems.Add(r.nIdle.ToString());
            if (r.ProductKind >= 0)
            {
                item.SubItems.Add(((ProductKind)(r.ProductKind)).ToString());
            }
            else
            {
                item.SubItems.Add("-");
            }
            item.SubItems.Add(r.nProjects.ToString());
            item.SubItems.Add(r.nProducts.ToString());
            item.SubItems.Add(String.Format("{0:C}", r.MonthlyCost));
            item.SubItems.Add(String.Format("{0:C}", r.MonthlyRevenue));
            item.SubItems.Add(String.Format("{0:C}", r.Balance)).ForeColor = r.Balance > 0 ? SystemColors.WindowText : Color.Red;
            //listView1.Items.Add(item);
            listViewItems.Add(item);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            listViewItems.Clear();
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Run();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (pattern != null && pattern.currentDecisions != null)
            {
                string s = "";
                foreach (int d in pattern.currentDecisions)
                {
                    char c;
                    switch (d)
                    {
                        case -1:
                            c = '_';
                            break;
                        case 0:
                            c = '/';
                            break;
                        case 1:
                            c = 'F';
                            break;
                        case 2:
                            c = 'C';
                            break;
                        case 3:
                            c = 'L';
                            break;
                        default: throw new InvalidOperationException();
                    }
                    s += c + " ";
                }
                label1.Text = s;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            foreach (var item in listViewItems) listView1.Items.Add(item);
        }
    }
}
