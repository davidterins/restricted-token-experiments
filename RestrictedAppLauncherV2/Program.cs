using Microsoft.Extensions.Hosting;
using RestrictedAppLauncherV2.Win32;


using var tokenProvider = TokenProvider
    .OpenCurrentProcessToken()
    .RestrictToken();

//var applicationPath = @$"{AppDomain.CurrentDomain.BaseDirectory}AppToLaunch\AppToLaunch.exe";
var applicationPath = @$"{AppDomain.CurrentDomain.BaseDirectory}WebViewApp\WebViewApp.exe";
var workingDirectory = Directory.GetParent(applicationPath).FullName;

ProcessExtensions.CreateProcessAsUser(
    tokenProvider.GetToken(),
    applicationPath,
    "",
    workingDirectory,
    true,
    false);

Console.ReadLine();