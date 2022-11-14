// See https://aka.ms/new-console-template for more information

using System.Xml;
using CliWrap;
using CliWrap.Buffered;

Console.WriteLine("Hello, World!");
 
Command git = Cli.Wrap("git")
	.WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine));

var rootDirectory = SetRootDirectory();


Console.WriteLine($"Working Directory: {rootDirectory}");


var versionInfoFilePath = Path.Combine(rootDirectory, "version.build");

Console.WriteLine($"Loading Version Info file: {versionInfoFilePath}");
var versionFileInfo = new FileInfo(versionInfoFilePath);
if (!versionFileInfo.Exists)
	throw new FileNotFoundException(versionInfoFilePath);

var versionFileXml = new XmlDocument();
versionFileXml.Load(versionFileInfo.FullName);

Console.WriteLine("Reading dll.version attribute");
var versionString = (versionFileXml.DocumentElement ?? throw new NullReferenceException("DocumentElement")) 
	.ChildNodes
	.OfType<XmlElement>()
	.Where(e => string.Equals(e.Name, "property", StringComparison.OrdinalIgnoreCase))
	.Single(e => e.HasAttributes
	             && e.HasAttribute("name")
	             && string.Equals(e.GetAttribute("name"), "dll.version", StringComparison.OrdinalIgnoreCase)
	)
	.GetAttribute("value");


var versionTagCurrent = new Version(versionString);
var versionTagNext = new Version(versionTagCurrent.Major, versionTagCurrent.Minor, versionTagCurrent.Build + 1);

Console.WriteLine($"Current Version Tag: {versionTagCurrent}");
Console.WriteLine($"Next Version Tag: {versionTagNext}");


//create new release branch but do not check it out
var releaseBranch = $"release/{versionTagCurrent.Major}.{versionTagCurrent.Minor}-Release-{versionTagCurrent.Build}";
Console.WriteLine($"Creating new branch '{releaseBranch}'");
await git
	.WithArguments($"branch {releaseBranch}")
	.ExecuteBufferedAsync();
	

//create new branch for version bump and check it out
Console.WriteLine("Creating branch for version bump");
await git
	.WithArguments($"checkout -b bump/to-version-{versionTagNext}")
	.ExecuteBufferedAsync();
	

Console.WriteLine($"Update version info from {versionTagCurrent} to {versionTagNext}");
versionFileXml.DocumentElement
	.ChildNodes
	.OfType<XmlElement>()
	.Where(e => string.Equals(e.Name, "property", StringComparison.OrdinalIgnoreCase))
	.Single(e => e.HasAttributes
	             && e.HasAttribute("name")
	             && string.Equals(e.GetAttribute("name"), "dll.version", StringComparison.OrdinalIgnoreCase)
	)
	.Attributes["value"]!.Value = versionTagNext.ToString();
versionFileXml.Save(versionFileInfo.FullName);


Console.WriteLine("Commit changes");
await git
	.WithArguments("commit -a -m \"bump version\"") //https://git-scm.com/docs/git-commit#_options
	.ExecuteBufferedAsync();


//push
await git
	.WithArguments("push --all")
	.ExecuteBufferedAsync();


 string SetRootDirectory()
 {
	 const int maxDepth = 5;
	 var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);

	 var depth = 0;
	 do
	 {
		 if (currentDirectory!.EnumerateDirectories(".git").Any())
		 {
			 Environment.CurrentDirectory = currentDirectory.FullName;
			 return Environment.CurrentDirectory;
		 }

		 currentDirectory = currentDirectory.Parent;
		 depth++;
	 } while (depth < maxDepth);

	 throw new InvalidOperationException("root directory not found");
 }
 

	  