# Introduction

Minimal repo to reproduce issue https://github.com/dotnet/runtime/issues/102692

## Issue reproduction steps

1. Build all projects
2. Start `WindowsAuthWebApi` without debugger
3. Launch `RestrictedAppLauncherV2`
   * This project will launch the executable created from `AppToLaunch` running with a restricted token, which will then try to do a [Get] request to the web api.
