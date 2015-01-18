@echo off

#might be a tad unsafe, but checking for the package directories turns out to be faster.

cls
IF NOT EXIST "src\restsharp.portable.hal\packages\FAKE" (
    "src\restsharp.portable.hal\.nuget\NuGet.exe" "Install" "FAKE" "-OutputDirectory" "src\restsharp.portable.hal\packages" "-ExcludeVersion"
)

IF NOT EXIST "src\restsharp.portable.hal\packages\FSharp.Configuration" (
    "src\restsharp.portable.hal\.nuget\NuGet.exe" "Install" "FSharp.Configuration" "-OutputDirectory" "src\restsharp.portable.hal\packages" "-ExcludeVersion"
)

"src\restsharp.portable.hal\packages\FAKE\tools\Fake.exe" "build\build.fsx" %*

