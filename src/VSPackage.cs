using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace TrailingWhitespace
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Version, IconResourceID = 400)]
    [Guid(VSPackageGuids.PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideOptionPage(typeof(Options), "Environment", "Trailing Whitespace", 1208, 1209, false, "", ProvidesLocalizedCategoryName = false)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VSPackage : Package
    {
        public static Options Options { get; private set; }
        public const string Version = "1.0";
        public const string Title = "Trailing Whitespace Visualizer";

        protected override void Initialize()
        {
            Logger.Initialize(this, Title);
            Telemetry.Initialize(this, Version, "16cf8ed8-7f32-43bf-b14d-669b7cc0b348");

            Options = (Options)GetDialogPage(typeof(Options));
            base.Initialize();
        }
    }
}
