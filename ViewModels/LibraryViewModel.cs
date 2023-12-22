using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace SukaliCareApp.ViewModels
{
    public partial class LibraryViewModel(HttpProxyService proxyService) : BaseViewModel
    {
        private readonly HttpProxyService proxyService = proxyService;
        [ObservableProperty]
        ObservableCollection<LibResource> resources = [];

        [RelayCommand]
        public void OnRefreshing()
        {
            try
            {
                
                Observable.FromAsync(() => proxyService.GetLibResourcesAsync())
                    .Subscribe(async result =>
                    {
                        Resources.Clear();
                        foreach (var item in result)
                        {
                            Resources.Add(item);
                        }
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            IsBusy = false;
                        });
                    });
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task ReadMoreAsync(LibResource resource)
        {
            await Browser.OpenAsync(resource.Url ?? "http://sukalicare.nexus-healthcare.org");
        }
        public void OnAppearing()
        {
            IsBusy = true;
        }
    }
}
