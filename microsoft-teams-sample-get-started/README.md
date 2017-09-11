# Microsoft Teams Get Started Sample
Thank you for your interest in the Microsoft Teams Get Started sample project! This sample exercises a combination of Microsoft Teams platform capabilities, including bots, tabs, connectors, and compose extensions. Additionally, full manifest.json examples and packages are included to facilitate easy modification and sideloading into Microsoft Teams. 

This sample provides projects written in [Node.js](Node) and [C#/.NET](CSharp), mainly to illustrate Bot and Compose Extension differences with the respective Bot Framework SDKs.  For other capabilities, like Tabs and Connectors, there is no Microsoft Teams-related differences in implementation, so for the .NET/C# solution, we provide hosted implementations of those capabilities, built from the Node.js code base.  For more information on each sample, please refer to the appropriate project ReadMe files.

To use this sample, clone our GitHub repository using Git.

```
git clone https://github.com/OfficeDev/microsoft-teams-sample-get-started
cd microsoft-teams-sample-get-started
```
**For more information on developing apps for Microsoft Teams, please review the Microsoft Teams [developer documentation](https://msdn.microsoft.com/en-us/microsoft-teams/index).**

## Sample Highlights

This sample shows a simulated task management Saas, and leverages the following capabilities, in [Node.js](Node) and [C#/.NET](CSharp):

* Bots
* Compose Extension
* Tabs (Configurable and Static) (Node.js only)
* O365 Connectors (Node.js only)

Please note that this sample shows simulated and non-persisted data only.  Creating or assigning a "task" is illustrative only, and results are randomly generated.  E.g. if you create a "Make Coffee" task, the data is not persisted and does not appear in subsequent searches.  Rather, the goal is to show Microsoft Teams-based approaches to the core functionality, not the underlying business logic of a task management solution.
    
## Feedback
We welcome your feedback! [Here's how to send us yours](https://msdn.microsoft.com/en-us/microsoft-teams/feedback).

## Microsoft Open Source Code of Conduct
This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Contributing
Please read [Contributing](contributing.md) for details on the process for submitting pull requests to us.

## License
This project is licensed under the MIT License - see the [License](LICENSE) file for details.

## Copyright
Copyright (c) 2017 Microsoft Corporation. All rights reserved.