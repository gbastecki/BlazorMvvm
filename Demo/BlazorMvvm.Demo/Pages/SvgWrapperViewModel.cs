using BlazorMvvm.Demo.JsHandlers;
using BlazorMvvm.Demo.Pages.Components;
using Microsoft.AspNetCore.Components;
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

        public readonly IBlazorAsyncCommand CreateRectanglesCommand;
        public SvgWrapperViewModel(SvgHandler SvgHandler)
        {
            this.SvgHandler = SvgHandler;
            CreateRectanglesCommand = new BlazorAsyncCommand(CreateRectangles);
        }
        private Task CreateRectangles()
        {
            Rectangles.Clear();
            for (int i = 0; i < 10000; i++)
            {
                Rectangles.Add(new RectangleViewModel(this));
            }
            OnPropertyChanged();
            return Task.CompletedTask;
        }
    }
}
