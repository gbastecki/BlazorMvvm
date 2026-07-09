namespace BlazorMvvm.Tests.ViewModel.Collision.NamespaceA
{
    [BlazorMvvmViewModel]
    public partial class CollisionViewModel : BlazorViewModel
    {
        [BlazorObservableProperty]
        private string _valueA = "";
    }
}
