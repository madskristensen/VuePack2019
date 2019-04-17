using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Events;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace VuePack
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideAutoLoad(UIContextGuids80.SolutionHasSingleProject, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionHasMultipleProjects, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid("89f8c2df-69c6-4510-a09d-0980e1b5762b")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VuePackage : AsyncPackage
    {
        protected override System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            SolutionEvents.OnAfterCloseSolution += delegate { DirectivesCache.Clear(); };

            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
