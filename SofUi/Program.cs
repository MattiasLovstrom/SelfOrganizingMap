using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SofUi
{
    static class Program
    {
        [STAThread]
        static async Task Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var mainForm = new Form1();
            Application.Run(mainForm);
        }
    }
}
