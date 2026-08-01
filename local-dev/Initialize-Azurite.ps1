[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$connectionString = 'DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;'
$containers = @('question-imports', 'profile-pictures')

if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    throw 'Azure CLI is required to initialize Azurite. Install it or create the containers in Azure Storage Explorer.'
}

foreach ($container in $containers) {
    az storage container create `
        --name $container `
        --connection-string $connectionString `
        --only-show-errors `
        --output none

    if ($LASTEXITCODE -ne 0) {
        throw "Could not create the '$container' container in Azurite. Ensure the azurite Docker service is running."
    }
}

Write-Output 'Azurite containers are ready: question-imports, profile-pictures.'
