using Microsoft.Extensions.Hosting;
using RestrictedAppLauncherV2.Win32;


using var tokenProvider = TokenProvider
    .OpenCurrentProcessToken()
    .RestrictToken();

//var applicationPath = @$"{AppDomain.CurrentDomain.BaseDirectory}AppToLaunch\AppToLaunch.exe";
//var applicationPath = @$"{AppDomain.CurrentDomain.BaseDirectory}WebViewApp\WebViewApp.exe";
var applicationPath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
//C:\Program Files(x86)\Microsoft\Edge\Application
//var workingDirectory = Directory.GetParent(applicationPath).FullName;
var workingDirectory = @"C:\Program Files (x86)\Microsoft\Edge\Application";

ProcessExtensions.CreateProcessAsUser(
    tokenProvider.GetToken(),
    applicationPath,
    "",
    workingDirectory,
    true,
    false);

Console.ReadLine();