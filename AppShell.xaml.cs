namespace SukaliCareApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(SignupView), typeof(SignupView)); 
        }
    }
}
