using System.Threading;
using System.Windows.Forms;

namespace LinearOptimization
{
    public partial class SplashForm : Form
    {
        private delegate void CloseDelegate();
        private static SplashForm splashForm;

        public SplashForm()
        {
            InitializeComponent();
        }

        static public void ShowSplashScreen()
        {
            if (splashForm != null)
                return;
            Thread thread = new Thread(new ThreadStart(SplashForm.ShowForm));
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        static private void ShowForm()
        {
            splashForm = new SplashForm();
            Application.Run(splashForm);
        }

        static public void CloseForm()
        {
            splashForm.Invoke(new CloseDelegate(SplashForm.CloseFormInternal));
        }

        static private void CloseFormInternal()
        {
            splashForm.Close();
            splashForm = null;
        }
    }
}
