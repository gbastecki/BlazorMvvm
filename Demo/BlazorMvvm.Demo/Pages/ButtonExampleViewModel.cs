using System.Threading.Tasks;

namespace BlazorMvvm.Demo.Pages
{
    public partial class ButtonExampleViewModel : BlazorViewModel
    {
        [BlazorCommand(autoRefreshOnIsExecutingChanged: true)]
        private async Task DisableButton()
        {
            await Task.Delay(5000);
        }
    }
}
