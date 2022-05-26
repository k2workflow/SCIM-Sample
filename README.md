# Nintex K2 Samples for SCIM
Samples repository for integrating with the K2 SCIM 2.0 Server API. 

Please refer to [K2 Identity Providers](https://help.nintex.com/en-US/K2Cloud/userguide/current/default.htm#../Subsystems/Default/Content/IdProviders/IdProviders.htm) for an overview of the K2 SCIM API and related documentation.

For more information on the SCIM specification, please see http://www.simplecloud.info/ for details and additional resources.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

These samples were adapted from the [IdentityModel.OidcClient.Samples](https://github.com/IdentityModel/IdentityModel.OidcClient/tree/main/clients/ConsoleClientWithBrowser) repository.

## ConsoleClientWithBrowser

A .NET Core console application using the Authorization Code Grant Flow with PKCE.

### Overview

This sample obtains a Bearer Token using the OpenID Connect Authorization Code Grant Flow from the [K2 Identity Provider](https://login.onk2.com).


### Getting Started

To use this sample you need to obtain your K2 Tenant Identifier, ClientId, and SCIM Token for your SCIM instance.

## ConsoleClientWithClientCredentials

A .NET Core console application using the Client Credentials Grant Flow.

### Overview

This sample obtains a Bearer Token using the Client Credentials Grant Flow from the [K2 Identity Provider](https://login.onk2.com).

### Getting Started

In order to use Client Credentials your K2 tenant needs to be [onboarded](https://help.nintex.com/en-US/k2cloud/userguide/current/default.htm#../Subsystems/Default/Content/IdProviders/ClientCredentials.htm).
To use this sample you need to obtain your K2 Tenant Identifier, ClientId, Client Secret, and SCIM Token for your SCIM instance.

## License

MIT, found in the [LICENSE](./LICENSE) file.

[Nintex K2](https://www.nintex.com/process-automation/k2-software/)

