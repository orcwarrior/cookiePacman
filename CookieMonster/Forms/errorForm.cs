using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace CookieMonster.Forms
{
    public partial class errorForm : Form
    {
        public errorForm(UnhandledExceptionEventArgs e)
        : this()
        {
            if (e != null && e.ExceptionObject != null)
            {
                errorInfos.Text += e.GetType() + ">> " + ((Exception)e.ExceptionObject).Message;
                System.IO.StreamWriter errorStackTrace = new System.IO.StreamWriter("error_stackTrace.txt", true);
                errorStackTrace.WriteLine("---");
                errorStackTrace.WriteLine("THROWED Exception Data:");
                errorStackTrace.WriteLine(((Exception)e.ExceptionObject));
                errorStackTrace.WriteLine("***");
            }
            else
                errorInfos.Text += "[UNKNOWN]";
        }
        public errorForm()
        {
            InitializeComponent();
            // Fill Stack List:
            // (ommit 2 frames)
            StackTrace stackTrace = new StackTrace(2,true);           // get call stack with filenames infos too
            if (stackTrace != null)
            {
                foreach (StackFrame f in stackTrace.GetFrames())
                {
                    textBox1.Text += f + "\n";
                }
                System.IO.StreamWriter errorStackTrace = new System.IO.StreamWriter("error_stackTrace.txt", true);

                // Write stack to file:
                errorStackTrace.WriteLine("***");
                errorStackTrace.WriteLine("ERROR STACK TRACE");
                errorStackTrace.WriteLine("Date: " + DateTime.Now);
                errorStackTrace.WriteLine("***");
                //foreach (string s in listBox1.Items)
                    errorStackTrace.Write(textBox1.Text);
                errorStackTrace.Close();

                // Show cursor again:
                System.Windows.Forms.Cursor.Show();
            /*    string filename = underlyingStackFrame.GetFileName();
                if (filename != null)
                    filename = filename.Substring(filename.LastIndexOf('\\') + 1);
                return "[" + filename + "->" + underlyingStackFrame.GetMethod().Name + ":" + underlyingStackFrame.GetFileLineNumber() + "]";
            */
            }
        }

        /// Kopiuj do schowka:
        private void button1_Click(object sender, EventArgs e)
        {
            string buf = "";
            //foreach (string s in listBox1.Items)
            //    buf += s;
            Clipboard.SetText(textBox1.Text);

        }

    }
}
