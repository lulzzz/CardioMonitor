using System;

namespace Markeli.Storyboards
{
    public class InnerStoryboardPageInfo
    {
        /// <summary>
        /// Unique Id for page within all storyboards
        /// </summary>
        public Guid PageUniqueId { get; set; }

        public Guid PageId { get; set; }

        public Guid StoryboardId { get; set; }

        public bool IsStartPage { get; set; }
    }
}