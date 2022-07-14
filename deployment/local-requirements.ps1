$ErrorActionPreference = "Stop"

Install-Module CredentialManager
Install-Module -Name Az -Scope CurrentUser -Repository PSGallery -Force -AllowClobber