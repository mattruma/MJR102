param(
    [Parameter(Mandatory = $true)]
    [String] $EndPoint,
    [Parameter(Mandatory = $true)]
    [String] $FileName
)

Write-Host "End Point           : $($EndPoint)"
Write-Host "File Name           : $($FileName)"

Invoke-RestMethod -Uri $EndPoint -Method Post -InFile $FileName -UseDefaultCredentials