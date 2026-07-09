namespace BlazorMvvm.Tests.ViewModel.Collision.NamespaceB
{
    [BlazorMvvmViewModel]
    public partial class CollisionViewModel : BlazorViewModel
    {
        [BlazorObservableProperty]
        private string _valueB = "";
    }
}