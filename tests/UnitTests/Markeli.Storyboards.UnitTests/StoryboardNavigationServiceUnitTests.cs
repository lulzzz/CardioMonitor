using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Markeli.Storyboards.UnitTests
{
    public class StoryboardNavigationServiceUnitTests
    {
        private readonly Storyboard _patientStoryboard;
        private readonly Storyboard _sessionsStoryboard;
        private readonly Storyboard _settingsStoryboard;

        private static readonly Guid FirstPageId = Guid.Parse("ea5a0e35-1a67-4d91-8242-1c5e04b47f4d");
        private static readonly Guid SecondPageId = Guid.Parse("ea5a0e35-2a67-4d91-8242-1c5e04b47f4d");
        private static readonly Guid ThirdPageId = Guid.Parse("ea5a0e35-3a67-4d91-8242-1c5e04b47f4d");
        private static readonly Guid ForuthPageId = Guid.Parse("ea5a0e35-4a67-4d91-8242-1c5e04b47f4d");
        private static readonly Guid FifthPageId = Guid.Parse("ea5a0e35-5a67-4d91-8242-1c5e04b47f4d");
        private static readonly Guid SixthPageId = Guid.Parse("ea5a0e35-6a67-4d91-8242-1c5e04b47f4d");


        private class TestInvoker : IUiInvoker
        {
            public void Invoke(Action action)
            {
                action.Invoke();
            }
        }

        public StoryboardNavigationServiceUnitTests()
        {
            _patientStoryboard = new Storyboard(Guid.Parse("1a5a0e35-1a67-4d91-8242-1c5e04b47f4d"));
            _sessionsStoryboard = new Storyboard(Guid.Parse("2a5a0e35-1a67-4d91-8242-1c5e04b47f4d"));
            _settingsStoryboard = new Storyboard(Guid.Parse("3a5a0e35-1a67-4d91-8242-1c5e04b47f4d"));
            
            _patientStoryboard.RegisterPage(
                FirstPageId,
                view: typeof(TestView),
                viewModel: typeof(TestViewModel),
                isStartPage: true);
            _patientStoryboard.RegisterPage(
                ForuthPageId,
                view: typeof(TestView),
                viewModel: typeof(TestViewModel),
                isStartPage: false);
            _patientStoryboard.RegisterPage(
                FifthPageId,
                view: typeof(TestView),
                viewModel: typeof(TestViewModel),
                isStartPage: false);

            _sessionsStoryboard.RegisterPage(
                SecondPageId,
                view: typeof(TestView),
                viewModel: typeof(TestViewModel),
                isStartPage: true);
            _sessionsStoryboard.RegisterPage(
                SixthPageId,
                view: typeof(TestView),
                viewModel: typeof(TestViewModel),
                isStartPage: false);

            _settingsStoryboard.RegisterPage(
                ThirdPageId,
                view: typeof(TestView),
                viewModel: typeof(TestViewModel),
                isStartPage: true);
        }

        [Fact]
        public void RegisterStoryboards_Ok()
        {
            var service = new StoryboardsNavigationService();
            service.SetUiInvoker(new TestInvoker());
            service.RegisterStoryboard(_patientStoryboard);
            service.RegisterStoryboard(_sessionsStoryboard);
            service.RegisterStoryboard(_settingsStoryboard);
            Assert.True(true);
        }

        [Fact]
        public void CreateStartPages_Ok()
        {
            var pageCreator = new Mock<IStoryboardPageCreator>();
            pageCreator
                .Setup(x => x.CreateView(It.IsAny<Type>()))
                .Returns(new TestView());
            pageCreator
                .Setup(x => x.CreateViewModel(It.IsAny<Type>()))
                .Returns(new TestViewModel());

            var service = new StoryboardsNavigationService();
            service.SetUiInvoker(new TestInvoker());
            service.SetStoryboardPageCreator(pageCreator.Object);
            service.RegisterStoryboard(_patientStoryboard);
            service.RegisterStoryboard(_sessionsStoryboard);
            service.RegisterStoryboard(_settingsStoryboard);
            service.CreateStartPages();
            Assert.True(true);
        }

        [Fact]
        public async Task GoToStoryboard_Ok()
        {
            var pageCreator = new Mock<IStoryboardPageCreator>();
            pageCreator
                .Setup(x => x.CreateView(It.IsAny<Type>()))
                .Returns(new TestView());
            pageCreator
                .Setup(x => x.CreateViewModel(It.IsAny<Type>()))
                .Returns(new TestViewModel());

            var service = new StoryboardsNavigationService();
            service.SetUiInvoker(new TestInvoker());
            var currentStoryboardId = Guid.Empty;
            service.ActiveStoryboardChanged += (sender, guid) => currentStoryboardId = guid;

            service.SetStoryboardPageCreator(pageCreator.Object);
            service.RegisterStoryboard(_patientStoryboard);
            service.RegisterStoryboard(_sessionsStoryboard);
            service.RegisterStoryboard(_settingsStoryboard);
            service.CreateStartPages();

            await service.GoToStoryboardAsync(_patientStoryboard.StoryboardId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);

            await service.GoToStoryboardAsync(_sessionsStoryboard.StoryboardId);
            Assert.Equal(_sessionsStoryboard.StoryboardId, currentStoryboardId);

            await service.GoToStoryboardAsync(_settingsStoryboard.StoryboardId);
            Assert.Equal(_settingsStoryboard.StoryboardId, currentStoryboardId);

            await service.GoToStoryboardAsync(_patientStoryboard.StoryboardId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
        }

        [Fact]
        public async Task GoBackOnSameStoryboard_Ok()
        {
            var pageCreator = new Mock<IStoryboardPageCreator>();
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
            service.SetUiInvoker(new TestInvoker());
            var currentStoryboardId = Guid.Empty;
            service.ActiveStoryboardChanged += (sender, guid) => currentStoryboardId = guid;

            service.SetStoryboardPageCreator(pageCreator.Object);
            service.RegisterStoryboard(_patientStoryboard);
            service.RegisterStoryboard(_sessionsStoryboard);
            service.RegisterStoryboard(_settingsStoryboard);
            service.CreateStartPages();

            await service.GoToPageAsync(FirstPageId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, FirstPageId);
            
            await service.GoToPageAsync(ForuthPageId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, ForuthPageId);
            Assert.True(service.CanGoBack());


            await service.GoToPageAsync(FifthPageId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, FifthPageId);

            Assert.True(service.CanGoBack());
            await  service.GoBackAsync();
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, ForuthPageId);

            Assert.True(service.CanGoBack());
            await service.GoBackAsync();
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, FirstPageId);

            Assert.False(service.CanGoBack());

        }

        [Fact]
        public async Task GoBackOnDifferentStoryboards_Ok()
        {
            var pageCreator = new Mock<IStoryboardPageCreator>();
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
            service.SetUiInvoker(new TestInvoker());
            var currentStoryboardId = Guid.Empty;
            service.ActiveStoryboardChanged += (sender, guid) => currentStoryboardId = guid;

            service.SetStoryboardPageCreator(pageCreator.Object);
            service.RegisterStoryboard(_patientStoryboard);
            service.RegisterStoryboard(_sessionsStoryboard);
            service.RegisterStoryboard(_settingsStoryboard);
            service.CreateStartPages();

            await service.GoToPageAsync(FirstPageId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, FirstPageId);

            await service.GoToPageAsync(ForuthPageId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, ForuthPageId);
            Assert.True(service.CanGoBack());


            await service.GoToPageAsync(SecondPageId);
            Assert.Equal(_sessionsStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_sessionsStoryboard.ActivePage.ViewModel.PageId, SecondPageId);


            await service.GoToPageAsync(SixthPageId);
            Assert.Equal(_sessionsStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_sessionsStoryboard.ActivePage.ViewModel.PageId, SixthPageId);

            await service.GoToPageAsync(ThirdPageId);
            Assert.Equal(_settingsStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_settingsStoryboard.ActivePage.ViewModel.PageId, ThirdPageId);

            
            await service.GoToPageAsync(FifthPageId);
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, FifthPageId);
            
            Assert.True(service.CanGoBack());
            await service.GoBackAsync();
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, ForuthPageId);

            Assert.True(service.CanGoBack());
            await service.GoBackAsync();
            Assert.Equal(_patientStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_patientStoryboard.ActivePage.ViewModel.PageId, FirstPageId);

            Assert.True(service.CanGoBack());
            await service.GoBackAsync();
            Assert.Equal(_settingsStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_settingsStoryboard.ActivePage.ViewModel.PageId, ThirdPageId);


            Assert.True(service.CanGoBack());
            await service.GoBackAsync();
            Assert.Equal(_sessionsStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_sessionsStoryboard.ActivePage.ViewModel.PageId, SixthPageId);
            
            Assert.True(service.CanGoBack());
            await service.GoBackAsync();
            Assert.Equal(_sessionsStoryboard.StoryboardId, currentStoryboardId);
            Assert.Equal(_sessionsStoryboard.ActivePage.ViewModel.PageId, SecondPageId);

            Assert.False(service.CanGoBack());

        }

        private class TestView : IStoryboardPageView
        {
            public IStoryboardPageViewModel ViewModel { get; set; }
        }

        private class TestViewModel : IStoryboardPageViewModel
        {
            public void Dispose()
            {
            }

            public Guid PageId { get; set; }
            public Guid StoryboardId { get; set; }

            public Task OpenAsync(IStoryboardPageContext context)
            {
                return Task.CompletedTask;
            }

            public Task<bool> CanLeaveAsync()
            {
                return Task.FromResult(true);
            }

            public Task LeaveAsync()
            {
                return Task.CompletedTask;
            }

            public Task ReturnAsync(IStoryboardPageContext context)
            {
                return Task.CompletedTask;
            }

            public Task<bool> CanCloseAsync()
            {
                return Task.FromResult(true);
            }

            public Task CloseAsync()
            {
                return Task.CompletedTask;
            }

            public event Func<object, Task> PageCanceled;

            public event Func<object, Task> PageCompleted;

            public event Func<object, Task> PageBackRequested;

            public event Func<object, TransitionRequest, Task> PageTransitionRequested;

            public override string ToString()
            {
                if (FirstPageId == PageId) { 
                        return "first";
                }
                if (SecondPageId == PageId)
                {
                    return "second";
                }
                if (ThirdPageId == PageId)
                {
                    return "third";
                }
                if (ForuthPageId == PageId)
                {
                    return "fourth";
                }
                if (FifthPageId == PageId)
                {
                    return "fifth";
                }
                if (SixthPageId == PageId)
                {
                    return "sixth";
                }
                return "unknown";
            }
        }
    }
}