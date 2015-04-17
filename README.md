## Trailing Whitespace extension for VS2012/2013/2015

[![Build status](https://ci.appveyor.com/api/projects/status/2n9cfl1lups6o7q4)](https://ci.appveyor.com/project/madskristensen/trailingwhitespace)

This extension will highlight any trailing whitespace on any line
in any editor in Visual Studio.

Download and install the extension from the 
[Visual Studio Gallery](http://visualstudiogallery.msdn.microsoft.com/a204e29b-1778-4dae-affd-209bea658a59) 
or get the 
[nightly build](https://ci.appveyor.com/project/madskristensen/trailingwhitespace/build/artifacts). 

![C# whitespace](https://github.com/madskristensen/TrailingWhitespace/blob/master/artifacts/CSharp.png)

#### Remove trailing whitespace

You can very easily delete all the trailing whitespace in a file by executing the **Delete Horizontal White Space** command
found in **Edit** -> **Advanced** or by using the shortcut key combination Ctrl+K, Ctrl+\

#### Changing the background color

You can change the background color from the **Tools -> Options** dialog under the **Environment -> Fonts and Colors** settings. The setting is for the *Text Editor* and the display item is called *Trailing Whitespace*.

![VisualStudio2013OptionsDialog](artifacts/VisualStudioSettings.png "Visual Studio 2013 Options Dialog")

#### Remove on save
Every time a file is saved, all trailing whitespace is removed. This can be disabled in the
**Tools -> Options** dialog.

![Options dialog](artifacts/OptionsDialog.png)