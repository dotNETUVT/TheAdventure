using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TheAdventure;

public class ScriptEngine
{

    public void ClearScripts()
    {
        _scripts.Clear();
    }

    private PortableExecutableReference[] scriptReferences;

    private Dictionary<string, IScript> _scripts = new Dictionary<string, IScript>();
    private FileSystemWatcher _watcher;

    public ScriptEngine()
    {
        var rtPath = Path.GetDirectoryName(typeof(object).Assembly.Location) +
                 Path.DirectorySeparatorChar;

        var references = new string[] {
            #region .Net SDK
            rtPath + "System.Private.CoreLib.dll",
            rtPath + "System.Runtime.dll",
            rtPath + "System.Console.dll",
            rtPath + "netstandard.dll",

            rtPath + "System.Text.RegularExpressions.dll", // IMPORTANT!
            rtPath + "System.Linq.dll",
            rtPath + "System.Linq.Expressions.dll", // IMPORTANT!

            rtPath + "System.IO.dll",
            rtPath + "System.Net.Primitives.dll",
            rtPath + "System.Net.Http.dll",
            rtPath + "System.Private.Uri.dll",
            rtPath + "System.Reflection.dll",
            rtPath + "System.ComponentModel.Primitives.dll",
            rtPath + "System.Globalization.dll",
            rtPath + "System.Collections.Concurrent.dll",
            rtPath + "System.Collections.NonGeneric.dll",
            rtPath + "Microsoft.CSharp.dll",
            #endregion
            typeof(IScript).Assembly.Location
        };
        scriptReferences = references.Select(x => MetadataReference.CreateFromFile(x)).ToArray();
    }

    public void AttachWatcher(string path){
        _watcher = new FileSystemWatcher(path);
        _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.Size;
        _watcher.Changed += OnScriptChanged;
        _watcher.Deleted += OnScriptChanged;
        _watcher.EnableRaisingEvents = true;
    }

    public void OnScriptChanged(object source, FileSystemEventArgs e){
        if(_scripts.ContainsKey(e.FullPath)){
            Console.WriteLine($"Change detected for: {e.FullPath}");
            switch(e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    RemoveScript(e.FullPath);
                    Load(e.FullPath);
                    break;
                case WatcherChangeTypes.Deleted:
                    RemoveScript(e.FullPath);
                    break;
            }
        }
    }

    public void RemoveScript(string path){
        if(_scripts.TryGetValue(path, out var script)){
            _scripts.Remove(path);
        }
    }

    public void LoadAll(string scriptFolder){
        AttachWatcher(scriptFolder);
        var dirInfo = new DirectoryInfo(scriptFolder);
        if(dirInfo.Exists){
            foreach(var file in dirInfo.GetFiles()){
                if (file.Extension.ToLowerInvariant() == ".script"){
                    try
                    {
                        Load(file.FullName);
                    }
                    catch(Exception ex){
                        Console.WriteLine($"Exception trying to load {file.FullName}");
                        Console.WriteLine(ex);
                    }
                }
            }
        }
    }

    public void ExecuteAll(Engine engine)
    {
        if (engine.GetPlayer().IsDead()) return;

        foreach (var script in _scripts.Values)
        {
            script.Execute(engine);
        }
    }


    public IScript? Load(string file)
    {
        Console.WriteLine($"Loading script {file}");
        FileInfo fileInfo = new FileInfo(file);
        var fileOutput = fileInfo.FullName.Replace(fileInfo.Extension, ".dll");
        var code = File.ReadAllText(fileInfo.FullName);
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create(fileInfo.Name.Replace(fileInfo.Extension, string.Empty), new [] { syntaxTree }, 
           scriptReferences, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using (var compiledScriptAssembly = new FileStream(fileOutput, FileMode.OpenOrCreate)){
            var result = compilation.Emit(compiledScriptAssembly);
            if(!result.Success){
                foreach(var diag in result.Diagnostics){
                    if(diag.Severity == DiagnosticSeverity.Error ){
                        Console.WriteLine(string.Join(";", diag.Descriptor.CustomTags));
                        Console.WriteLine($"{diag.Descriptor.MessageFormat.ToString()} - {code.Substring(diag.Location.SourceSpan.Start, diag.Location.SourceSpan.Length)} - {diag.Descriptor.HelpLinkUri.ToString()} - {diag.Location.ToString()}");
                    }
                }
                throw new FileLoadException(file);
            }
        }
        foreach(var type in Assembly.LoadFile(fileOutput).GetTypes()){
            if (type.IsAssignableTo(typeof(IScript))){
                var instance = (IScript)type.GetConstructor(Type.EmptyTypes)?.Invoke(null);
                instance.Initialize();
                if(instance != null){
                    _scripts.Add(file, instance);
                }
                return instance;
            }
        }
        return null;
    }
}