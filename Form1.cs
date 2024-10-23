using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MouseMover
{
    public partial class Form1 : Form
    {
		private const int MouseLeftDownCode = 0x02;
		private const int MouseLeftUpCode = 0x04;
		//private const int MouseRightDownCode = 0x08;
		//private const int MouseRightUpCode = 0x10;

		public Form1()
		{
			InitializeComponent();

			progressBar1.Minimum = 0;
			progressBar1.Maximum = 100;
			progressBar1.Step = 1;
			//ShowInTaskbar = false;
			notifyIcon1.Text = "Открыть настройки";
			notifyIcon1.ContextMenuStrip = new ContextMenuStrip();
			var mi1 = new ToolStripMenuItem("Настройки");
			mi1.Click += (o, e) =>
			{
				WindowState = FormWindowState.Normal;
				Show();
			};
			var mi3 = new ToolStripMenuItem("Старт");
			mi3.Click += (o, e) =>
			{
				int interval = (int)numericUpDown1.Value * 60000 + (int)numericUpDown2.Value * 1000;
				if (interval > 0)
				{
					timer1.Interval = interval;

				}
				else
				{
					timer1.Interval = 10000;
					numericUpDown2.Value = 10;
				}
				timer2.Interval = timer1.Interval / 100;
				progressBar1.Value = 0;
				timer1.Start();
				timer2.Start();
				numericUpDown1.Enabled = false;
				numericUpDown2.Enabled = false;
				label4.Text = "Запущено";
				notifyIcon1.ContextMenuStrip.Items[0].Enabled = false;
				notifyIcon1.ContextMenuStrip.Items[1].Enabled = true;
			};
			var mi4 = new ToolStripMenuItem("Стоп");
			mi4.Click += (o, e) =>
			{
				timer1.Stop();
				timer2.Stop();
				progressBar1.Value = 0;
				numericUpDown1.Enabled = true;
				numericUpDown2.Enabled = true;
				label4.Text = "Остановлено";
				notifyIcon1.ContextMenuStrip.Items[1].Enabled = false;
				notifyIcon1.ContextMenuStrip.Items[0].Enabled = true;
			};
			var mi2 = new ToolStripMenuItem("Выход");
			mi2.Click += (o, e) =>
			{
				notifyIcon1.Visible = false;
				notifyIcon1.Dispose();
				Application.Exit();
			};

			notifyIcon1.ContextMenuStrip.Items.Add(mi3);
			notifyIcon1.ContextMenuStrip.Items.Add(mi4);
			notifyIcon1.ContextMenuStrip.Items.Add(mi1);
			notifyIcon1.ContextMenuStrip.Items.Add(mi2);
			notifyIcon1.ContextMenuStrip.Items[1].Enabled = false;
			GetMonitors();
		}

		[DllImport("user32")]
		private static extern int SetCursorPos(int x, int y);
		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

		private void GetMonitors()
		{
			Screen[] allMonitors = Screen.AllScreens;
			allMonitors = allMonitors.OrderBy(x => x.Bounds.Left).ToArray();
			lb_monitorsValue.Text = allMonitors.Length.ToString();
			gB_Monitors.Visible = true;
			Size = MaximumSize;
			MinimumSize = MaximumSize;
			string monName = "Монитор ";
			int counter = 1;

			//Заполняем таблицу мониторов
			foreach (var item in allMonitors)
			{
				monName = "Монитор " + counter.ToString();
				if (item.Primary)
				{
					monName += " (основной)";
				}
				counter++;
				Opacity = 0;
				Rectangle bounds = item.Bounds;
				Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
				using (Graphics gr = Graphics.FromImage(bitmap))
				{
					gr.CopyFromScreen(bounds.X,
						bounds.Y,
						0,
						0,
						bounds.Size,
						CopyPixelOperation.SourceCopy);
					//gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
					//int imageHeight = (int)Math.Round(Math.Abs((double)bounds.Height / (double)bounds.Width * 150));
					//gr.DrawImage(bitmap, 0, 0, 150, imageHeight);
					gr.Dispose();
				}

				monName += "\nКоординаты:";
				monName += $"\nX:    {bounds.Left}:{bounds.Left + bounds.Width}";
				monName += $"\nY:    {bounds.Y}:{bounds.Bottom}";
				Opacity = 100;
				dataGridView1.Rows.Add(bitmap, monName, true, bounds.Left.ToString(), bounds.Y.ToString(), bounds.Width.ToString(), bounds.Bottom.ToString());
			}

		}
        public void DoMouseClick()
        {
            uint X = (uint)Cursor.Position.X;
            uint Y = (uint)Cursor.Position.Y;
            mouse_event(MouseLeftDownCode | MouseLeftUpCode, X, Y, 0, 0);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridView1.Rows)
            {
                string chk = item.Cells[2].Value.ToString();
                if (chk == "True")
                {
                    int XCor = Int32.Parse(item.Cells[3].Value.ToString());
                    int YCor = Int32.Parse(item.Cells[4].Value.ToString());
                    int MonWidth = Int32.Parse(item.Cells[5].Value.ToString());
                    int MonHeight = Int32.Parse(item.Cells[6].Value.ToString());
                    var x = Cursor.Position.X;
                    var y = Cursor.Position.Y;

                    //Переносим курсор на текущий экран, если он находится на другом
                    if (!(XCor <= x & x <= XCor + MonWidth))
                    {
                        x = XCor + 15;
                        if (!(YCor <= y & y <= YCor + MonHeight))
                        {
                            y = YCor + 15;
                        }
                    }


                    if (x == 0 & y == 0)
                    {
                        x++;
                        y++;
                    }
                    SetCursorPos(x + 1, y + 1);
                    SetCursorPos(x, y);
                    if (chkB_click.Checked)
                        DoMouseClick();

                }
            }

            progressBar1.Value = 0;
        }
        private void Timer2_Tick(object sender, EventArgs e)
        {
            progressBar1.PerformStep();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            notifyIcon1.ContextMenuStrip.Items[0].PerformClick();
            button2.Enabled = true;
            button1.Enabled = false;
            dataGridView1.Enabled = false;
            dataGridView1.DefaultCellStyle.BackColor = Color.LightGray;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.LightGray;
        }
        private void Button2_Click(object sender, EventArgs e)
        {
            notifyIcon1.ContextMenuStrip.Items[1].PerformClick();
            button1.Enabled = true;
            button2.Enabled = false;
            dataGridView1.Enabled = true;
            dataGridView1.DefaultCellStyle.BackColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.White;
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = true;
            Hide();
        }
    }
}
