using BlazorMvvm.Demo.JsHandlers;
using BlazorMvvm.Demo.Pages.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorMvvm.Demo.Pages
{
    public class SvgWrapperViewModel : BlazorViewModel
    {
        public const int SvgMaxX = 1000;
        public const int SvgMaxY = 1000;
        public ElementReference SvgRef;
        public readonly SvgHandler SvgHandler;
        public readonly List<RectangleViewModel> Rectangles = [];

        public RectangleViewModel ActiveDragRect { get; set; }

        public readonly IBlazorAsyncCommand CreateRectanglesCommand;
        public readonly IBlazorAsyncRelayCommand<MouseEventArgs> OnSvgMouseMoveCommand;
        public readonly IBlazorAsyncRelayCommand<MouseEventArgs> OnSvgMouseUpCommand;
        public readonly IBlazorAsyncRelayCommand<MouseEventArgs> OnSvgMouseLeaveCommand;

        public SvgWrapperViewModel(SvgHandler SvgHandler)
        {
            this.SvgHandler = SvgHandler;
            CreateRectanglesCommand = new BlazorAsyncCommand(CreateRectangles) { ContinueOnCapturedContext = false };
            OnSvgMouseMoveCommand = new BlazorAsyncRelayCommand<MouseEventArgs>(OnSvgMouseMove);
            OnSvgMouseUpCommand = new BlazorAsyncRelayCommand<MouseEventArgs>(OnSvgMouseUp);
            OnSvgMouseLeaveCommand = new BlazorAsyncRelayCommand<MouseEventArgs>(OnSvgMouseLeave);
        }

        private async Task CreateRectangles()
        {
            await Task.Yield();
            Rectangles.Clear();
            for (int i = 0; i < 10000; i++)
            {
                Rectangles.Add(new RectangleViewModel(this));
            }
            OnPropertyChanged();
        }

        private async Task OnSvgMouseMove(MouseEventArgs e)
        {
            if (ActiveDragRect != null)
            {
                await ActiveDragRect.HandleMouseMove(e);
            }
        }

        private async Task OnSvgMouseUp(MouseEventArgs e)
        {
            if (ActiveDragRect != null)
            {
                await ActiveDragRect.HandleMouseUp(e);
                ActiveDragRect = null;
            }
        }

        private Task OnSvgMouseLeave(MouseEventArgs e)
        {
            ActiveDragRect?.CancelDrag();
            ActiveDragRect = null;
            return Task.CompletedTask;
        }
    }
}
