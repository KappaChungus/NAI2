using System;
using System.Windows.Forms;
using Perceptron;

namespace SimpleGUI
{
    public class Form1 : Form
    {
        private Button startButton {get;}
        public Form1()
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            this.Size = new Size(400, 300);
            
            ButtonFactory buttonFactory = new ButtonFactory(() => startButton);
            
            Button trainButton = buttonFactory.GetButton("train",buttonFactory.BtnBrowseFileClick);
            Button testButton = buttonFactory.GetButton("test",buttonFactory.BtnBrowseFileClick);
            Button alphaButton = buttonFactory.GetButton("alpha",buttonFactory.SetButtonClick);
            Button thetaButton = buttonFactory.GetButton("theta",buttonFactory.SetButtonClick);
            startButton = buttonFactory.GetButton("start",buttonFactory.StartButtonClick);
            startButton.Enabled = false;

            panel.Controls.Add(trainButton);
            panel.Controls.Add(testButton);
            panel.Controls.Add(alphaButton);
            panel.Controls.Add(thetaButton);
            panel.Controls.Add(startButton);

            this.Controls.Add(panel);
            this.Text = "Simple GUI";
            this.Size = new System.Drawing.Size(300, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
