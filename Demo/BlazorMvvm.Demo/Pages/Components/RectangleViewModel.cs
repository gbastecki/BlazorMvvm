using Microsoft.AspNetCore.Components.Web;
using System.Threading.Tasks;

namespace BlazorMvvm.Demo.Pages.Components
{
    public class RectangleViewModel : BlazorViewModel
    {
        public const int RectWidth = 100;
        public const int RectHeight = 100;
        public const int RectMaxX = SvgWrapperViewModel.SvgMaxX - RectWidth;
        public const int RectMaxY = SvgWrapperViewModel.SvgMaxY - RectHeight;
        private readonly SvgWrapperViewModel ParentViewModel;

        public readonly IBlazorAsyncRelayCommand<MouseEventArgs> OnMouseDownCommand;
        public readonly IBlazorAsyncRelayCommand<MouseEventArgs> OnMouseMoveCommand;
        public readonly IBlazorAsyncRelayCommand<MouseEventArgs> OnMouseUpCommand;
        public readonly IBlazorAsyncRelayCommand<MouseEventArgs> OnMouseEnterCommand;
        public readonly IBlazorAsyncRelayCommand<MouseEventArgs> OnMouseOutCommand;
        public RectangleViewModel(SvgWrapperViewModel ParentViewModel)
        {
            this.ParentViewModel = ParentViewModel;
            OnMouseDownCommand = new BlazorAsyncRelayCommand<MouseEventArgs>(OnMouseDown);
            OnMouseMoveCommand = new BlazorAsyncRelayCommand<MouseEventArgs>(OnMouseMove);
            OnMouseUpCommand = new BlazorAsyncRelayCommand<MouseEventArgs>(OnMouseUp);
            OnMouseEnterCommand = new BlazorAsyncRelayCommand<MouseEventArgs>(OnMouseEnter);
            OnMouseOutCommand = new BlazorAsyncRelayCommand<MouseEventArgs>(OnMouseOut);
        }

        private int _X;
        public int X
        {
            get => _X;
            set
            {
                if (value < 0) value = 0;
                else if (value > RectMaxX) value = RectMaxX;
                Set(ref _X, value);
            }
        }

        private int _Y;
        public int Y
        {
            get => _Y;
            set
            {
                if (value < 0) value = 0;
                else if (value > RectMaxY) value = RectMaxY;
                Set(ref _Y, value);
            }
        }

        private bool IsMouseDown;
        private int InitialX;
        private int InitialY;
        private double MouseDownX;
        private double MouseDownY;
        private async Task OnMouseDown(MouseEventArgs e)
        {
            if (e.Button != 0) return;
            (int x, int y) = await GetClickCoordinates(e);
            MouseDownX = x;
            MouseDownY = y;
            InitialX = X;
            InitialY = Y;
            IsMouseDown = true;
        }
        private async Task OnMouseMove(MouseEventArgs e)
        {
            if (!IsMouseDown) return;
            (int x, int y) = await GetClickCoordinates(e);
            X = InitialX + (int)(x - MouseDownX);
            Y = InitialY + (int)(y - MouseDownY);
        }
        private async Task OnMouseUp(MouseEventArgs e)
        {
            if (!IsMouseDown) return;
            (int x, int y) = await GetClickCoordinates(e);
            X = InitialX + (int)(x - MouseDownX);
            Y = InitialY + (int)(y - MouseDownY);
            IsMouseDown = false;
        }
        private Task OnMouseEnter(MouseEventArgs e)
        {
            if (e.Buttons != 1)
            {
                IsMouseDown = false;
            }
            return Task.CompletedTask;
        }
        private async Task OnMouseOut(MouseEventArgs e)
        {
            if (!IsMouseDown) return;
            (int x, int y) = await GetClickCoordinates(e);
            X = InitialX + (int)(x - MouseDownX);
            Y = InitialY + (int)(y - MouseDownY);
        }


        private async Task<(int x, int y)> GetClickCoordinates(MouseEventArgs e)
        {
            var result = await ParentViewModel.SvgHandler.GetSvgClickCoordinates(ParentViewModel.SvgRef, e);
            return ((int)result.X, (int)result.Y);
        }
    }
}
