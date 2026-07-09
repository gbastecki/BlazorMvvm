using System;

namespace BlazorMvvm.Demo.Pages
{
    public class DebounceExampleViewModel : BlazorViewModel, IDisposable
    {
        private string _searchText = string.Empty;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;

                // Debounce the full refresh for 400ms to update character count and results
                DebounceRefresh(400, null);
            }
        }

        public int CharacterCount => _searchText.Length;
        public string ReversedText
        {
            get
            {
                char[] arr = _searchText.ToCharArray();
                System.Array.Reverse(arr);
                return new string(arr);
            }
        }

        public void Dispose()
        {
            base.CancelAllDebounces();
            GC.SuppressFinalize(this);
        }
    }
}
