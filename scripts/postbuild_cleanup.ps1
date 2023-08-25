param([string]$TargetDir);

Get-ChildItem -Path $TargetDir *.pdb | foreach { Remove-Item -Path $_.FullName }
Get-ChildItem -Path $TargetDir *.xml | foreach { Remove-Item -Path $_.FullName }