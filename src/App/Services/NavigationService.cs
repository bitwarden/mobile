using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Bit.App.Abstractions;
using Bit.App.Pages;

namespace Bit.App.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IMainPage _presentationRoot;
        private readonly IViewLocator _viewLocator;

        public NavigationService(IMainPage presentationRoot, IViewLocator viewLocator)
        {
            _presentationRoot = presentationRoot;
            _viewLocator = viewLocator;
        }

        private Xamarin.Forms.INavigation Navigator => _presentationRoot.MainPage.Navigation;
        
        public void PresentAsMainPage(BaseViewModel viewModel)
        {
            var page = _viewLocator.CreateAndBindPageFor(viewModel);

            IEnumerable<BaseViewModel> viewModelsToDismiss = FindViewModelsToDismiss(_presentationRoot.MainPage);

            if (_presentationRoot.MainPage is NavigationPage navPage)
            {
                // If we're replacing a navigation page, unsub from events
                navPage.PopRequested -= NavPagePopRequested;
            }

            // viewModel.BeforeFirstShown();

            _presentationRoot.MainPage = page;

            foreach (BaseViewModel toDismiss in viewModelsToDismiss)
            {
                // toDismiss.AfterDismissed();
            }
        }

        public void PresentAsNavigatableMainPage(BaseViewModel viewModel)
        {
            var page = _viewLocator.CreateAndBindPageFor(viewModel);

            NavigationPage newNavigationPage = new NavigationPage(page);

            IEnumerable<BaseViewModel> viewModelsToDismiss = FindViewModelsToDismiss(_presentationRoot.MainPage);

            if (_presentationRoot.MainPage is NavigationPage navPage)
            {
                navPage.PopRequested -= NavPagePopRequested;
            }

            // viewModel.BeforeFirstShown();

            // Listen for back button presses on the new navigation bar
            newNavigationPage.PopRequested += NavPagePopRequested;
            _presentationRoot.MainPage = newNavigationPage;

            foreach (BaseViewModel toDismiss in viewModelsToDismiss)
            {
                // toDismiss.AfterDismissed();
            }
        }

        private IEnumerable<BaseViewModel> FindViewModelsToDismiss(Page dismissingPage)
        {
            var viewmodels = new List<BaseViewModel>();

            if (dismissingPage is NavigationPage)
            {
                viewmodels.AddRange(
                    Navigator
                        .NavigationStack
                        .Select(p => p.BindingContext)
                        .OfType<BaseViewModel>()
                );
            }
            else
            {
                var viewmodel = dismissingPage?.BindingContext as BaseViewModel;
                if (viewmodel != null) viewmodels.Add(viewmodel);
            }

            return viewmodels;
        }

        private void NavPagePopRequested(object sender, NavigationRequestedEventArgs e)
        {
            if (Navigator.NavigationStack.LastOrDefault()?.BindingContext is BaseViewModel poppingPage)
            {
                // poppingPage.AfterDismissed();
            }
        }

        public async Task NavigateTo(BaseViewModel viewModel)
        {
            var page = _viewLocator.CreateAndBindPageFor(viewModel);

            // await viewModel.BeforeFirstShown();

            await Navigator.PushAsync(page);
        }

        public async Task NavigateBack()
        {
            var dismissing = Navigator.NavigationStack.Last().BindingContext as BaseViewModel;

            await Navigator.PopAsync();

            // dismissing?.AfterDismissed();
        }

        public async Task NavigateBackToRoot()
        {
            var toDismiss = Navigator
                .NavigationStack
                .Skip(1)
                .Select(vw => vw.BindingContext)
                .OfType<BaseViewModel>()
                .ToArray();

            await Navigator.PopToRootAsync();

            foreach (BaseViewModel viewModel in toDismiss)
            {
                // viewModel.AfterDismissed().FireAndForget();
            }
        }
    }

    public class ViewLocator : IViewLocator
    {
        public Page CreateAndBindPageFor<TViewModel>(TViewModel viewModel) where TViewModel : BaseViewModel
        {
            var pageType = FindPageForViewModel(viewModel.GetType());

            var page = (Page)Activator.CreateInstance(pageType);

            page.BindingContext = viewModel;

            return page;
        }

        protected virtual Type FindPageForViewModel(Type viewModelType)
        {
            var pageTypeName = viewModelType
                .AssemblyQualifiedName
                .Replace("ViewModel", "");

            var pageType = Type.GetType(pageTypeName);
            if (pageType == null)
                throw new ArgumentException(pageTypeName + " type does not exist");

            return pageType;
        }
    }
}