using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace TrailingWhitespace
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version)]
    [Guid(PackageGuids.guidVSPackageString)]
    //[ProvideAutoLoad(UIContextGuids80.DesignMode, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(Options), "Environment", "Trailing Whitespace", 1208, 1209, false, "", ProvidesLocalizedCategoryName = false)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VSPackage : AsyncPackage
    {
        private static Options _options;
        public static Options Options
        {
            get
            {
                if (_options == null)
                {
                    ForceLoad();
                }

                return _options;
            }
        }

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            _options = (Options)GetDialogPage(typeof(Options));

        }

        private static void ForceLoad()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var shell = ServiceProvider.GlobalProvider.GetService(typeof(SVsShell)) as IVsShell;
            Assumes.Present(shell);

            shell.LoadPackage(ref PackageGuids.guidVSPackage, out IVsPackage package);
        }
    }
}
