@echo off
cls
"src\restsharp.portable.hal\.nuget\NuGet.exe" "Install" "FAKE" "-OutputDirectory" "src\restsharp.portable.hal\packages" "-ExcludeVersion"
"src\restsharp.portable.hal\packages\FAKE\tools\Fake.exe" "build\build.fsx" %*

