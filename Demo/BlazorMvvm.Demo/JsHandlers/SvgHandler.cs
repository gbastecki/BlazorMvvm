using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using System;

namespace BlazorMvvm.Demo.JsHandlers
{
    public class SvgHandler : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public SvgHandler(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/SvgHandler.js").AsTask());
        }
        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }

        public async ValueTask<(double X, double Y)> GetSvgClickCoordinates(ElementReference element, MouseEventArgs e)
        {
            var module = await moduleTask.Value;
            var result = await module.InvokeAsync<double[]>("GetSvgClickCoordinates", element, e.ClientX, e.ClientY);
            return (result[0], result[1]);
        }
    }
}
