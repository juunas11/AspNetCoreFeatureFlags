# Feature flags in an ASP.NET Core app

This is a sample application that shows how feature flags can be used in an ASP.NET Core application running in Azure.
It uses an in-memory user store to store user data, and Azure App Configuration to store feature flags.

## Running the application locally

You can run the application with either Visual Studio or the .NET CLI.
By default, it will not use Azure App Configuration as a configuration source.
To enable Azure App Configuration, add the following to appsettings.json (or appsettings.Development.json/user secrets):

```json
{
  "AppConfig": {
    "Uri": "https://your-app-config-name.azconfig.io"
  }
}
```

The application uses an `AzureCliCredential` to authenticate with Azure App Configuration when running locally.
Make sure you have logged in to the correct account in the Azure CLI.
In case your Azure App Configuration resource is not in your home Azure AD tenant,
you can override the Azure AD tenant used by specifying the following setting in appsettings.json (or appsettings.Development.json/user secrets):

```json
{
  "LocalDevelopmentTenantId": "your-tenant-id"
}
```

## Running the application in Azure

To deploy the application to Azure, first you need to rename deployment/config.sample.json to deployment/config.json and fill in the required values.
Then, you can run the following command to deploy the application to Azure:

```powershell
# Make sure you are in the deployment folder first
.\Deploy.ps1
```

Then setup the initial feature flags in Azure App Configuration by running deployment/Reset-FeatureFlags.ps1.

You can optionally enable a custom domain for the App Service by setting up the required CNAME record and then running deployment/DeployCustomDomain.ps1.
This will setup the domain as well as an Azure managed HTTPS certificate.

## Feature flags in the application

- NewsSummary
  - When enabled, shows a few mock news article titles on the home page
- DarkTheme
  - When enabled, changes the color scheme of the application to a dark theme
- UserGreeting
  - When enabled, shows a greeting to the user on the home page

## Feature filters in the application

### AppVersionFeatureFilter

This filter is used to enable a feature for a specific version of the application.
You can see an example of its usage with the DarkTheme feature flag in appsettings.Development.json:

```json
"DarkTheme": {
    "EnabledFor": [
        {
            "Name": "AppVersion",
            "Parameters": {
                "Versions": {
                    "Stable": "OptIn",
                    "Beta": "Enabled"
                }
            }
        }
    ]
}
```

This enables the feature flag for all users who have selected the Beta version in the UI,
and allows users who have selected the Stable version to opt in to the feature.

### UserPercentageFeatureFilter

This filter is used to enable a feature for a percentage of users.
You can see an example of its usage with the UserGreeting feature flag in appsettings.Development.json:

```json
"UserGreeting": {
    "EnabledFor": [
        {
            "Name": "UserPercentage",
            "Parameters": {
                "Percentage": 30
            }
        }
    ]
}
```

This enables the feature flag for 30% of users.
The way it works is that it calculates what is 30% of 16, and rounds it to the nearest integer.
Then it looks at the last character of the user's GUID, and sees if it is between 0 and the result from the previous step.
If it is, the feature is enabled for that user.

So for example, if the percentage is set to 50, then the feature will be enabled for users whose GUID ends with 0-7.

In most applications when you want to target a subset of users, you would use random users instead of users with GUIDs ending with certain characters.
Because in this approach a user with a GUID ending in 0 will always get all new features enabled first.
This sample application does not have a database of users where we could enable flags for random users,
so we use this approach.
