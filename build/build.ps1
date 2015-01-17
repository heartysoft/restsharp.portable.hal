Framework('4.0')
Include .\version.ps1

$prod = 'RestSharp.Portable.Hal'
$prodProj = $prod
$solution = "$prod.sln"

properties {
    $config= if($config -eq $null) {'Debug' } else {$config}
    $base_dir = resolve-path .\..
    $source_dir = "$base_dir\src"
    $tools_dir = "$base_dir\tools"
    $env = "local"
    $out_dir = "$base_dir\out\$config"
    $prod_dir = "$source_dir\$prod"
    $prod_artefacts_dir="$prod_dir\$prod\bin\$config"
    $prod_test_dir = "$prod_dir\$tests\bin\$config"
    $test_results_dir="$base_dir\test-results"
    $package_dir = "$base_dir\deploy"
    $test_dir = "$out_dir\tests"
}

task local -depends compile
task package -depends compile
task default -depends local


task clean {
    #code
    rd $prod_artefacts_dir -recurse -force  -ErrorAction SilentlyContinue | out-null
    mkdir $prod_artefacts_dir  -ErrorAction SilentlyContinue  | out-null
    
    #out dirs
    rd $out_dir -recurse -force  -ErrorAction SilentlyContinue | out-null
    mkdir "$out_dir" -ErrorAction SilentlyContinue  | out-null
}

task version -depends clean {
	 $commitHashAndTimestamp = Get-GitCommitHashAndTimestamp
     $commitHash = Get-GitCommitHash
     $timestamp = Get-GitTimestamp
     $branchName = Get-GitBranchOrTag
	 
	 $assemblyInfos = Get-ChildItem -Path $base_dir -Recurse -Filter AssemblyInfo.cs

	 $assemblyInfo = gc "$base_dir\AssemblyInfo.pson" | Out-String | iex
	 $version = $assemblyInfo.Version
	 #$productName = $assemblyInfo.ProductName
	 $companyName = $assemblyInfo.CompanyName
	 $copyright = $assemblyInfo.Copyright

	 try {
        foreach ($assemblyInfo in $assemblyInfos) {
            $path = Resolve-Path $assemblyInfo.FullName -Relative
            #Write-Host "Patching $path with product information."
            Patch-AssemblyInfo $path $Version $Version $branchName $commitHashAndTimestamp $companyName $copyright
        }         
    } catch {
        foreach ($assemblyInfo in $assemblyInfos) {
            $path = Resolve-Path $assemblyInfo.FullName -Relative
            Write-Host "Reverting $path to original state."
            & { git checkout --quiet $path }
        }
    }	
}

task compile -depends version {
	try{
		exec { msbuild $prod_dir\$solution /t:Clean /t:Build /p:Configuration=$config /p:VisualStudioVersion=12.0 /v:q /nologo }
	} finally{
		$assemblyInfos = Get-ChildItem -Path $base_dir -Recurse -Filter AssemblyInfo.cs
		foreach ($assemblyInfo in $assemblyInfos) {
            $path = Resolve-Path $assemblyInfo.FullName -Relative
            Write-Verbose "Reverting $path to original state."
            & { git checkout --quiet $path }
        }
	}
}

task package-web {
}