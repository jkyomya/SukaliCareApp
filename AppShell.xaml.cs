namespace SukaliCareApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(SignupView), typeof(SignupView));
            // Pages opened from other screens rather than from a tab.
            Routing.RegisterRoute(nameof(HealthArticleView), typeof(HealthArticleView));
            Routing.RegisterRoute(nameof(GroupDetailView), typeof(GroupDetailView));
            Routing.RegisterRoute(nameof(LibraryView), typeof(LibraryView));
            Routing.RegisterRoute(nameof(DevicesView), typeof(DevicesView));
        }
    }
}
