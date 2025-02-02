using BlazorMvvm;
using System.Threading.Tasks;

namespace BlazorMvvm.Demo.Pages
{
    public class ButtonExampleViewModel : BlazorViewModel
    {
        public readonly IBlazorAsyncCommand DisableButtonCommand;
        public ButtonExampleViewModel()
        {
            DisableButtonCommand = new BlazorAsyncCommand(DisableButton);
            DisableButtonCommand.OnIsExecutingChanged += DisableButtonCommand_OnIsExecutingChanged;
        }
        ~ButtonExampleViewModel()
        {
            DisableButtonCommand.OnIsExecutingChanged -= DisableButtonCommand_OnIsExecutingChanged;
        }
        private void DisableButtonCommand_OnIsExecutingChanged(bool isExecuting)
        {
            base.OnPropertyChanged();
        }

        private async Task DisableButton()
        {
            await Task.Delay(5000);
        }
    }
}
