using Microsoft.AspNetCore.Components;
using RlssCandidateDetails.Client.Shared;

namespace RlssCandidateDetails.Client.Classes.RenderFragments
{
    public class MainLayoutFragments : ComponentBase, IDisposable
    {
        [CascadingParameter]
        // To get this value set we must in MainLayout.razor surround the @body call with a <CascadingValue/>
        // tag and pass in an instance of MainlayoutFragments. If we don't do this mainLayout will not get set.
        public MainLayout mainLayout { get; set; }

        [Parameter]
        // This will be the fragment we want to render to the screen
        public RenderFragment HeaderTitle { get; set; }

        /// <summary>
        /// When this gets called by Blazor we inishalize the MainLayoutFragments parameter in Mainlayout.razor (_mainLayoutFragment)
        /// </summary>
        protected override void OnInitialized()
        {
            // call a function we made in MainLayout.razor that will inishlize the
            // MainLayoutFragments parameter (_mainLayoutFragment) to an instance of this class
            // to use on that layout.
            mainLayout.InishalizeLayoutFragment(this);
            base.OnInitialized();
            // call the update layout method (this will force a UI update)
            this.UpdateLayout();

        }

        protected override bool ShouldRender()
        {
            var shouldRender = base.ShouldRender();
            if (shouldRender)
            {
                this.UpdateLayout();
            }
            return base.ShouldRender();
        }

        /// <summary>
        /// Calling this method will call the mainLayout.StateHasChanged() method
        /// </summary>
        public void UpdateLayout()
        {
            // calls a method we made on MainLayout.razor that calls the mainLayout.StateHasChanged().
            // We can't call the method directly because StateHasChanged is a protected method.
            mainLayout.UpddateUI();
            
        }
        public void Dispose()
        {
            // pass in null to the method which sets mainLayout._mainLayoutFragment to null
            mainLayout?.InishalizeLayoutFragment(null);
        }
    }
}
