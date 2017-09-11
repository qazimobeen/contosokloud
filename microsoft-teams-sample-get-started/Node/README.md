# Teams Sample app for Node.js

This app simulates a web-based task management SaaS, and will simulate allowing users to create, manage and search tasks.  All content is randomly generated - no data in the service is actually persisted or consistant from view to view.  

**For more information on developing apps for Microsoft Teams, please review the Microsoft Teams [developer documentation](https://msdn.microsoft.com/en-us/microsoft-teams/index).**

## Prerequisites
The minimum prerequisites to run this sample are:
* Latest Node.js with NPM. Download it from [here](https://nodejs.org/en/download/).
* An Office 365 account with access to Microsoft Teams, with [sideloading enabled](https://msdn.microsoft.com/en-us/microsoft-teams/setup).
* [Optional] The Bot Framework Emulator. To install the Bot Framework Emulator, download it from [here](https://emulator.botframework.com/). Please refer to [this documentation article](https://github.com/microsoft/botframework-emulator/wiki/Getting-Started) to know more about the Bot Framework Emulator.
* **[Recommended]** Visual Studio Code for IntelliSense and debugging.  Download it from [here](https://code.visualstudio.com/) for free.

>**Note**: some features in the sample require that you [enable Public Developer Preview mode](https://msdn.microsoft.com/en-us/microsoft-teams/publicpreview) in Microsoft Teams.

For more information about how to configure and test our samples, see [Sample applications for the Microsoft Teams Developer Platform](https://msdn.microsoft.com/en-us/microsoft-teams/samples).

## Code Highlights

### Tabs
The Sample app demonstrates simple Configurable Tabs in channels and Static Tabs in personal view.  Add "auth=1" as a query parameter to tab URLs to turn on the authentication flow, which signs you in to Azure AD and gets your display name from Microsoft Graph.  Note that we have provided an alternate manifest.json with this auth flag set.

### Bots
The simple bot implementation demonstrates a bot in both teams and personal scope.  It will respond to three commands: "create", "find", and "link", and will generate a Welcome message when added to a team.

### Compose Extensions
The same bot implementation shows an example of a simple compose extension, which will allow you to generate random sample tasks in the conversation pane.  Note that "search" is not really enabled since tasks are not persisted.

### Connectors
The sample shows a simple implementation of a Connector registration implementation, and a sample of sending a Connector Card to the registered Connector via a process triggered "externally."

To simply illustrate the Connector functionality, you can utilize the built-in Incoming Webhook connector:
1) Select a channel in Teams you'd like to receive the messages
2) On the channel options, select Connectors, and add the Incoming Webhook Connector
3) Name it anything you wish, and get the resulting URI, the channel webhook URI used in the testing flow per below.
>Note that this process does not leverage the setup or registration flow in the connector.js code.

Alternately, you can go through the full process to register a new Connector, which will trigger the setup and registration flows in the connector.js file:
1) You'll need to register a new connector in the Connector Developer Portal, Follow the steps here: [Registering your connector](https://msdn.microsoft.com/en-us/microsoft-teams/connectors#registering-your-connector)
2) Ensure you have both Teams and Groups checkboxes selected.
3) For the Landing page for groups during registration, you'll use our sample code's setup endpoint: `https://[BASE_URI]/connector/setup`
4) For the Redirect URL during registration, you'll use our sample code's registration endpoint:  `https://[BASE_URI]/api/message/connector/register`
* In both steps 3 & 4, `[BASE_URI]` is the full URI for your running sample which will be the same Ngrok endpoint used for the rest of your sample, if running locally.
5) In the manifest.json file, update: `connectors.connectorId` to use your new Connector ID, which you can retrieve via the Connector Developer Portal's Copy Code or Download Manifest buttons.
6) In your launch.json / debug configuration, set the `CONNECTOR_APP_ID` environment variable to be your new Connector ID.



To test the Connector Card functionality:
1) For a registered Connector:  the registration results page creates a link you can click on or copy and paste in your browser
2) For a Incoming Webhook connector:  in your browser, enter: `https://[BASE_URI]/api/message/connector/send?webhook_url=[channel_webhook_uri]`
* `[BASE_URI]` is the full URI for your running sample
* `[channel_webhook_uri]` is the URI from Incoming Webhook setup from step #3 above.

## More Information

To get more information about how to get started with Teams App development, please review the following resources:
* [Apps in Microsoft Teams](https://msdn.microsoft.com/en-us/microsoft-teams/teamsapps)
* [Getting started with tabs for Microsoft Teams](https://msdn.microsoft.com/en-us/microsoft-teams/tabs)
* [Getting started with Bots for Microsoft Teams](https://msdn.microsoft.com/en-us/microsoft-teams/bots)
* [Getting started with Connectors for Microsoft Teams](https://msdn.microsoft.com/en-us/microsoft-teams/connectors)
* [Compose Extensions](https://msdn.microsoft.com/en-us/microsoft-teams/composeextensions)
* [Packaging](https://msdn.microsoft.com/en-us/microsoft-teams/createpackage) and [Sideloading](https://msdn.microsoft.com/en-us/microsoft-teams/submission) Microsoft Teams apps
* [Microsoft Teams Samples](https://msdn.microsoft.com/en-us/microsoft-teams/samples)

