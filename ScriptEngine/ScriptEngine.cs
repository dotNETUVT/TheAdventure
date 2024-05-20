using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TheAdventure
{
    public class ScriptEngine : IDisposable
    {
        private readonly PortableExecutableReference[] _scriptReferences;
        private readonly ConcurrentDictionary<string, IScript> _scripts = new ConcurrentDictionary<string, IScript>();
        private FileSystemWatcher? _watcher;
        private bool _disposed;

        public ScriptEngine()
        {
            var rtPath = Path.GetDirectoryName(typeof(object).Assembly.Location) +
                         Path.DirectorySeparatorChar;

            var references = new[]
            {
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
            _scriptReferences = references.Select(x => MetadataReference.CreateFromFile(x)).ToArray();
        }

        public void AttachWatcher(string path)
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
            }

            _watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.Size,
                EnableRaisingEvents = true
            };
            _watcher.Changed += OnScriptChanged;
            _watcher.Deleted += OnScriptChanged;
        }

        private void OnScriptChanged(object source, FileSystemEventArgs e)
        {
            if (_scripts.ContainsKey(e.FullPath))
            {
                Console.WriteLine($"Change detected for: {e.FullPath}");
                switch (e.ChangeType)
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

        public void RemoveScript(string path)
        {
            _scripts.TryRemove(path, out _);
        }

        public void LoadAll(string scriptFolder)
        {
            AttachWatcher(scriptFolder);
            var dirInfo = new DirectoryInfo(scriptFolder);
            if (dirInfo.Exists)
            {
                foreach (var file in dirInfo.GetFiles("*.script"))
                {
                    try
                    {
                        Load(file.FullName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception trying to load {file.FullName}");
                        Console.WriteLine(ex);
                    }
                }
            }
        }

        public void ExecuteAll(Engine engine)
        {
            foreach (var script in _scripts.Values)
            {
                script.Execute(engine);
            }
        }

        public IScript? Load(string file)
        {
            Console.WriteLine($"Loading script {file}");
            var fileInfo = new FileInfo(file);
            var fileOutput = fileInfo.FullName.Replace(fileInfo.Extension, ".dll");
            var code = File.ReadAllText(fileInfo.FullName);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(fileInfo.Name.Replace(fileInfo.Extension, string.Empty),
                new[] { syntaxTree },
                _scriptReferences,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var compiledScriptAssembly = new FileStream(fileOutput, FileMode.OpenOrCreate))
            {
                var result = compilation.Emit(compiledScriptAssembly);
                if (!result.Success)
                {
                    foreach (var diag in result.Diagnostics.Where(diag => diag.Severity == DiagnosticSeverity.Error))
                    {
                        Console.WriteLine($"{diag.GetMessage()} at {diag.Location}");
                    }
                    throw new FileLoadException($"Failed to load script: {file}");
                }
            }

            var assembly = Assembly.LoadFile(fileOutput);
            foreach (var type in assembly.GetTypes().Where(type => typeof(IScript).IsAssignableFrom(type)))
            {
                if (Activator.CreateInstance(type) is IScript instance)
                {
                    instance.Initialize();
                    _scripts[file] = instance;
                    return instance;
                }
            }

            return null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _watcher?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
