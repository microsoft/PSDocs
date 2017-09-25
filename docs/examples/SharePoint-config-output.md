# Server1

## Installation

|InstallerPath|OnlineMode|
| --- | --- |
|C:\\binaries\\prerequisiteinstaller.exe|True|

|BinaryDir|
| --- |
|C:\\binaries\\|

## Farm

|DatabaseServer|FarmConfigDatabaseName|AdminContentDatabaseName|
| --- | --- | --- |
|sql.contoso.com|SP_Config|SP_AdminContent|

## Installed services

|Name|Ensure|
| --- | --- |
|Claims to Windows Token Service|Present|
|Secure Store Service|Present|
|SharePoint Server Search|Present|

## Site
See the site configuration below.

|Url|OwnerAlias|Name|Template|
| --- | --- | --- | --- |
|http://sites.contoso.com|CONTOSO\\SP_Admin|DSC Demo Site|STS#0|

### Web applications

|Name|Url|Port|HostHeader|ApplicationPool|AuthenticationMethod|AllowAnonymous|
| --- | --- | --- | --- | --- | --- | --- |
|SharePoint Sites|http://sites.contoso.com|80|sites.contoso.com|SharePoint Sites|NTLM|False|

## Logging

|LogPath|DaysToKeepLogs|LogCutInterval|
| --- | --- | --- |
|C:\\ULS|7|15|
