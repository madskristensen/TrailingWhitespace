using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace TrailingWhitespace
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [Guid(PackageGuids.guidVSPackageString)]
    [ProvideAutoLoad(UIContextGuids80.DesignMode)]
    [ProvideOptionPage(typeof(Options), "Environment", "Trailing Whitespace", 1208, 1209, false, "", ProvidesLocalizedCategoryName = false)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VSPackage : Package
    {
        public static Options Options { get; private set; }
        public static DTE2 Dte;

        protected override void Initialize()
        {
            Dte = (DTE2)GetService(typeof(DTE2));
            Options = (Options)GetDialogPage(typeof(Options));

            base.Initialize();
        }
    }
}
