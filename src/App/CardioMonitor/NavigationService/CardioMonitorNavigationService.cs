using System.Linq;
using Markeli.Storyboards;

namespace CardioMonitor.NavigationService
{
    public class CardioMonitorNavigationService : StoryboardsNavigationService
    {
        protected override InnerStoryboardPageInfo GetLastPageFromStoryboard()
        {
            return Journal.LastOrDefault(x =>
                x.StoryboardId == ActiveInnerStoryboardPageInfo.StoryboardId
                && x.PageId != ActiveInnerStoryboardPageInfo.PageId);
        }
        
        public override bool CanGoBack()
        {
            return Invoker.Invoke(() => {
                if (ActiveStoryboard?.ActivePage == null) return false;
                if (Storyboards.Count < 0) return false;
                var pagesCount = Journal.Count(x => x.StoryboardId == ActiveStoryboard.StoryboardId);
                return pagesCount >= 2;
            });
        }
    }
}