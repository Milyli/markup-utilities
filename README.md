# markup-utilities
Project Champion - [Milyli](https://www.milyli.com/):  This Relativity Application allows the end user to import and export redactions into a Workspace.  The export functionality writes redactions to a file which can then be used by the import functionality to import redactions into the same workspace (for a different Markup Set), a different Workspace in the same environment or a Workspace in a completely different environment.

While this is an open source project on the kCura GitHub account, support is only available through through the Relativity developer community. You are welcome to use the code and solution as you see fit within the confines of the license it is released under. However, if you are looking for support or modifications to the solution, we suggest reaching out to the Project Champion listed below.

# Project Champion 
<a href="https://www.milyli.com/"><img src="https://i.postimg.cc/85DMhW2c/Milyli-Logo-Color-Tagline.png" width="250"></a>

[Milyli](https://www.milyli.com/) is a major contributor to this project.  If you are interested in having modifications made to this project, please reach out to [Milyli](http://ww.milyli.com/) for an estimate. 

# Project Setup
This project requires references to kCura's Relativity® SDK dlls.  These dlls are not part of the open source project and can be obtained by contacting support@kCura.com.  In the "packages" folder under "Source" you will need to create a "Relativity" folder if one does not exist.  You will need to add the following dlls:

- kCura.Agent.dll
- kCura.EventHandler.dll
- kCura.Relativity.Client.dll
- Relativity.API.dll
- Relativity.CustomPages.dll
