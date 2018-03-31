using System;

namespace Markeli.Storyboards
{
    public class StoryboardPageInfo
    {
        public StoryboardPageInfo(
            Guid pageId, 
            bool isStartPage, 
            Type view, 
            Type viewModel)
        {
            PageId = pageId;
            IsStartPage = isStartPage;
            View = view;
            ViewModel = viewModel;
        }

        public Guid PageId { get; }

        public bool IsStartPage { get;  }

        public Type View { get; }

        public Type ViewModel { get;  }
    }
}