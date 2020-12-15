param(
    [Parameter(Mandatory = $True)]
    [String] $EndPoint,
    [Parameter(Mandatory = $True)]
    [String] $FileName,
    [Int32] $Quantity = 1,
    [Boolean] $UseOriginalFileName = $True
)

Write-Host "End Point               : $($EndPoint)"
Write-Host "File Name               : $($FileName)"
Write-Host "Quantity                : $($Quantity)"
Write-Host "Use Original File Name  : $($UseOriginalFileName)"
Write-Host ""

For ($Index = 0; $Index -lt $Quantity; $Index++) 
{
    $MultipartFormDataContent = [System.Net.Http.MultipartFormDataContent]::new()
    $MultipartFile = $FileName

    $FileStream = [System.IO.FileStream]::new($MultipartFile, [System.IO.FileMode]::Open)

    $FileHeader = [System.Net.Http.Headers.ContentDispositionHeaderValue]::new("form-data")
    $FileHeader.Name = "file1"
    $FileHeader.FileName = Split-Path $FileName -leaf

    If ($UseOriginalFileName -ne $True) 
    {
        $FileHeader.FileName = "$([guid]::NewGuid())$([System.IO.Path]::GetExtension($FileName))"
    }

    Write-Host "Sending request $($Index + 1) of $($Quantity) for $($FileHeader.FileName)..."

    $FileContent = [System.Net.Http.StreamContent]::new($FileStream)
    $FileContent.Headers.ContentDisposition = $FileHeader

    $MultipartFormDataContent.Add($FileContent)

    $Body = $MultipartFormDataContent

    Invoke-RestMethod $EndPoint -Method 'POST' -Body $Body -UseDefaultCredentials -AllowUnencryptedAuthentication

    Write-Host "Request sent."
}