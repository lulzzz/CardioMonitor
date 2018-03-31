using System;
using Moq;
using Xunit;

namespace Markeli.Storyboards.UnitTests
{
    public class StoryboardNavigationServiceUnitTests
    {
        private  readonly Storyboard _patientStoryboard;
        private readonly Storyboard _sessionsStoryboard;
        private readonly Storyboard _settingsStoryboard;

        private static readonly Guid _firstPageId = Guid.Parse("ea5a0e35-1a67-4d91-8242-1c5e04b47f4d");
        private static readonly Guid _secondPageId = Guid.Parse("ea5a0e35-2a67-4d91-8242-1c5e04b47f4d");
        private static readonly Guid _thirdPageId = Guid.Parse("ea5a0e35-3a67-4d91-8242-1c5e04b47f4d");
        private static readonly Guid _foruthPageId = Guid.Parse("ea5a0e35-4a67-4d91-8242-1c5e04b47f4d");
        private static readonly Guid _fifthPageId = Guid.Parse("ea5a0e35-5a67-4d91-8242-1c5e04b47f4d");
        private static readonly Guid _sixthPageId = Guid.Parse("ea5a0e35-6a67-4d91-8242-1c5e04b47f4d");

        private Storyboard[] _storyboards;

        public StoryboardNavigationServiceUnitTests()
        {
            _patientStoryboard = new Storyboard(Guid.Parse("1a5a0e35-1a67-4d91-8242-1c5e04b47f4d"));
            _sessionsStoryboard = new Storyboard(Guid.Parse("2a5a0e35-1a67-4d91-8242-1c5e04b47f4d"));
            _settingsStoryboard = new Storyboard(Guid.Parse("3a5a0e35-1a67-4d91-8242-1c5e04b47f4d"));

            _storyboards = new[]
            {
                _patientStoryboard,
                _settingsStoryboard,
                _sessionsStoryboard
            };

            _patientStoryboard.RegisterPage(
                _firstPageId,
                view: typeof(TestView),
                viewModel: typeof(TestViewModel),
                isStartPage: true);
            _patientStoryboard.RegisterPage(
                _foruthPageId,
                view: typeof(TestView),
                viewModel: typeof(TestViewModel),
                isStartPage: false);
            _patientStoryboard.RegisterPage(
                _fifthPageId,
                view: typeof(TestView),
                viewModel: typeof(TestViewModel),
                isStartPage: false);

            _sessionsStoryboard.RegisterPage(
                _secondPageId,
                view: typeof(TestView),
                viewModel: typeof(TestViewModel),
                isStartPage: true);
            _sessionsStoryboard.RegisterPage(
                _sixthPageId,
                view: typeof(TestView),
                viewModel: typeof(TestViewModel),
                isStartPage: false);

            _settingsStoryboard.RegisterPage(
                _thirdPageId,
                view: typeof(TestView),
                viewModel: typeof(TestViewModel),
                isStartPage: true);
        }

        [Fact]
        public void RegisterStoryboards_Ok()
        {
            var service = new StoryboardsNavigationService();
            service.RegisterStoryboard(_patientStoryboard);
            service.RegisterStoryboard(_sessionsStoryboard);
            service.RegisterStoryboard(_settingsStoryboard);
            Assert.True(true);
        }

        [Fact]
        public void CreateStartPages_Ok()
        {
            var pageCreator = new Mock<IStroryboardPageCreator>();
            pageCreator
                .Setup(x => x.CreateView(It.IsAny<Type>()))
                .Returns(new TestView());
            pageCreator
                .Setup(x => x.CreateViewModel(It.IsAny<Type>()))
                .Returns(new TestViewModel());

            var service = new StoryboardsNavigationService();
            service.SetStoryboardPageCreator(pageCreator.Object);
            service.RegisterStoryboard(_patientStoryboard);
            service.RegisterStoryboard(_sessionsStoryboard);
            service.RegisterStoryboard(_settingsStoryboard);
            service.CreateStartPages();
            Assert.True(true);
        }

        [Fact]
        public void GoToStoryboard_Ok()
        {
            var pageCreator = new Mock<IStroryboardPageCreator>();
            pageCreator
                .Setup(x => x.CreateView(It.IsAny<Type>()))
                .Returns(new TestView());
            pageCreator
                .Setup(x => x.CreateViewModel(It.IsAny<Type>()))
                .Returns(new TestViewModel());

            var service = new StoryboardsNavigationService();
            var currentStoryboardId = Guid.Empty;
            service.ActiveStoryboardChanged += (sender, guid) => currentStoryboardId = guid;

            service.SetStoryboardPageCreator(pageCreator.Object);
            service.RegisterStoryboard(_patientStoryboard);
            service.RegisterStoryboard(_sessionsStoryboard);
            service.RegisterStoryboard(_settingsStoryboard);
            service.CreateStartPages();

            service.GoToStoryboard(_patientStoryboard.StoryboardId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);

            service.GoToStoryboard(_sessionsStoryboard.StoryboardId);
            Assert.Equal(_sessionsStoryboard.StoryboardId, currentStoryboardId);

            service.GoToStoryboard(_settingsStoryboard.StoryboardId);
            Assert.Equal(_settingsStoryboard.StoryboardId, currentStoryboardId);

            service.GoToStoryboard(_patientStoryboard.StoryboardId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
        }

        [Fact]
        public void GoBackOnSameStoryboard_Ok()
        {
            var pageCreator = new Mock<IStroryboardPageCreator>();
            pageCreator
                .SetupSequence(x => x.CreateView(It.IsAny<Type>()))
                .Returns(new TestView())
                .Returns(new TestView())
                .Returns(new TestView())
                .Returns(new TestView())
                .Returns(new TestView())
                .Returns(new TestView());
            pageCreator
                .SetupSequence(x => x.CreateViewModel(It.IsAny<Type>()))
                .Returns(new TestViewModel())
                .Returns(new TestViewModel())
                .Returns(new TestViewModel())
                .Returns(new TestViewModel())
                .Returns(new TestViewModel())
                .Returns(new TestViewModel());

            var service = new StoryboardsNavigationService();
            var currentStoryboardId = Guid.Empty;
            service.ActiveStoryboardChanged += (sender, guid) => currentStoryboardId = guid;

            service.SetStoryboardPageCreator(pageCreator.Object);
            service.RegisterStoryboard(_patientStoryboard);
            service.RegisterStoryboard(_sessionsStoryboard);
            service.RegisterStoryboard(_settingsStoryboard);
            service.CreateStartPages();

            service.GoToPage(_firstPageId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, _firstPageId);
            
            service.GoToPage(_foruthPageId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, _foruthPageId);
            Assert.True(service.CanGoBack());


            service.GoToPage(_fifthPageId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, _fifthPageId);

            Assert.True(service.CanGoBack());
            service.GoBack();
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, _foruthPageId);

            Assert.True(service.CanGoBack());
            service.GoBack();
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, _firstPageId);

            Assert.False(service.CanGoBack());

        }

        [Fact]
        public void GoBackOnDifferentStoryboards_Ok()
        {
            var pageCreator = new Mock<IStroryboardPageCreator>();
            pageCreator
                .SetupSequence(x => x.CreateView(It.IsAny<Type>()))
                .Returns(new TestView())
                .Returns(new TestView())
                .Returns(new TestView())
                .Returns(new TestView())
                .Returns(new TestView())
                .Returns(new TestView());
            pageCreator
                .SetupSequence(x => x.CreateViewModel(It.IsAny<Type>()))
                .Returns(new TestViewModel())
                .Returns(new TestViewModel())
                .Returns(new TestViewModel())
                .Returns(new TestViewModel())
                .Returns(new TestViewModel())
                .Returns(new TestViewModel());

            var service = new StoryboardsNavigationService();
            var currentStoryboardId = Guid.Empty;
            service.ActiveStoryboardChanged += (sender, guid) => currentStoryboardId = guid;

            service.SetStoryboardPageCreator(pageCreator.Object);
            service.RegisterStoryboard(_patientStoryboard);
            service.RegisterStoryboard(_sessionsStoryboard);
            service.RegisterStoryboard(_settingsStoryboard);
            service.CreateStartPages();

            service.GoToPage(_firstPageId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, _firstPageId);

            service.GoToPage(_foruthPageId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, _foruthPageId);
            Assert.True(service.CanGoBack());


            service.GoToPage(_secondPageId);
            Assert.Equal(_sessionsStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_sessionsStoryboard.ActivePage.ViewModel.PageId, _secondPageId);


            service.GoToPage(_sixthPageId);
            Assert.Equal(_sessionsStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_sessionsStoryboard.ActivePage.ViewModel.PageId, _sixthPageId);

            service.GoToPage(_thirdPageId);
            Assert.Equal(_settingsStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_settingsStoryboard.ActivePage.ViewModel.PageId, _thirdPageId);

            
            service.GoToPage(_fifthPageId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, _fifthPageId);
            
            Assert.True(service.CanGoBack());
            service.GoBack();
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, _foruthPageId);

            Assert.True(service.CanGoBack());
            service.GoBack();
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, _firstPageId);

            Assert.True(service.CanGoBack());
            service.GoBack();
            Assert.Equal(_settingsStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_settingsStoryboard.ActivePage.ViewModel.PageId, _thirdPageId);


            Assert.True(service.CanGoBack());
            service.GoBack();
            Assert.Equal(_sessionsStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_sessionsStoryboard.ActivePage.ViewModel.PageId, _sixthPageId);
            
            Assert.True(service.CanGoBack());
            service.GoBack();
            Assert.Equal(_sessionsStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_sessionsStoryboard.ActivePage.ViewModel.PageId, _secondPageId);

            Assert.False(service.CanGoBack());

        }

        private class TestView : IStoryboardPageView
        {
            public IStoryboardPageViewModel ViewModel { get; set; }
        }

        private class TestViewModel : IStoryboardPageViewModel
        {
            public string Name { get; set; }

            public void Dispose()
            {
            }

            public Guid PageId { get; set; }
            public Guid StoryboardId { get; set; }

            public void Open(IStoryboardPageContext context)
            {
            }

            public bool CanLeave()
            {
                return true;
            }

            public void Leave()
            {
            }

            public void Return(IStoryboardPageContext context)
            {
            }

            public bool CanClose()
            {
                return true;
            }

            public void Close()
            {
            }

            public event EventHandler PageCanceled;
            public event EventHandler PageCompleted;
            public event EventHandler PageBackRequested;
            public event EventHandler<TransitionRequest> PageTransitionRequested;

            public override string ToString()
            {
                if (_firstPageId == PageId) { 
                        return "first";
                }
                if (_secondPageId == PageId)
                {
                    return "second";
                }
                if (_thirdPageId == PageId)
                {
                    return "third";
                }
                if (_foruthPageId == PageId)
                {
                    return "fourth";
                }
                if (_fifthPageId == PageId)
                {
                    return "fifth";
                }
                if (_sixthPageId == PageId)
                {
                    return "sixth";
                }
                return "unknown";
            }
        }
    }
}