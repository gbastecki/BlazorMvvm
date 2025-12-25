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

        public RectangleViewModel(SvgWrapperViewModel ParentViewModel)
        {
            this.ParentViewModel = ParentViewModel;
            OnMouseDownCommand = new BlazorAsyncRelayCommand<MouseEventArgs>(OnMouseDown);
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

        private bool IsDragging;
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
            IsDragging = true;
            ParentViewModel.ActiveDragRect = this;
        }

        public async Task HandleMouseMove(MouseEventArgs e)
        {
            if (!IsDragging) return;
            (int x, int y) = await GetClickCoordinates(e);
            X = InitialX + (int)(x - MouseDownX);
            Y = InitialY + (int)(y - MouseDownY);
        }

        public async Task HandleMouseUp(MouseEventArgs e)
        {
            if (!IsDragging) return;
            (int x, int y) = await GetClickCoordinates(e);
            X = InitialX + (int)(x - MouseDownX);
            Y = InitialY + (int)(y - MouseDownY);
            IsDragging = false;
        }

        public void CancelDrag()
        {
            IsDragging = false;
        }

        private async Task<(int x, int y)> GetClickCoordinates(MouseEventArgs e)
        {
            var result = await ParentViewModel.SvgHandler.GetSvgClickCoordinates(ParentViewModel.SvgRef, e);
            return ((int)result.X, (int)result.Y);
        }
    }
}
