using Mopups.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SukaliCareApp.ViewModels
{
    public partial class AddDeviceViewModel : BaseViewModel
    {
        [RelayCommand]
        public async Task CloseDialog()
        {
            await MopupService.Instance.PopAllAsync();
        }
    }
}
