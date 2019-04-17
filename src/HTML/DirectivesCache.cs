using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VuePack
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("htmlx")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class DirectivesCache : IVsTextViewCreationListener
    {
        private static bool _hasRun, _isProcessing;
        private static readonly ConcurrentDictionary<string, string[]> _elements = new ConcurrentDictionary<string, string[]>();
        private static readonly ConcurrentDictionary<string, string[]> _attributes = new ConcurrentDictionary<string, string[]>();

        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public static Regex ElementRegex { get; } = new Regex("Vue\\.(elementDirective|component)\\(('|\")(?<name>[^'\"]+)\\2", RegexOptions.Compiled);

        public static Regex AttributeRegex { get; } = new Regex("Vue\\.(directive)\\(('|\")(?<name>[^'\"]+)\\2", RegexOptions.Compiled);

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_hasRun || _isProcessing)
            {
                return;
            }

            IWpfTextView textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);


            if (TextDocumentFactoryService.TryGetTextDocument(textView.TextDataModel.DocumentBuffer, out ITextDocument doc))
            {
                if (Path.IsPathRooted(doc.FilePath) && File.Exists(doc.FilePath))
                {
                    _isProcessing = true;
                    var dte = (DTE2)Package.GetGlobalService(typeof(DTE));
                    ProjectItem item = dte.Solution?.FindProjectItem(doc.FilePath);

                    ThreadHelper.JoinableTaskFactory.Run(async () =>
                    {
                        await EnsureInitializedAsync(item);
                        _hasRun = _isProcessing = false;
                    });
                }
            }
        }

        public static void Clear()
        {
            _hasRun = false;
            _elements.Clear();
            _attributes.Clear();
        }

        private async System.Threading.Tasks.Task EnsureInitializedAsync(ProjectItem item)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (item == null || item.ContainingProject == null)
            {
                return;
            }

            try
            {
                string folder = item.ContainingProject.GetRootFolder();

                List<string> vueFiles = GetFiles(folder, "*.vue");
                List<string> jsFiles = GetFiles(folder, "*.js");
                IEnumerable<string> allFiles = vueFiles
                                .Union(jsFiles)
                                .Where(f => !f.Contains(".min.") && !f.EndsWith(".intellisense.js") && !f.EndsWith("-vsdoc.js"));

                ProcessFile(allFiles.ToArray());
            }
            catch (Exception)
            {
                // TODO: Add logging
            }
        }

        public static void ProcessFile(params string[] files)
        {
            foreach (string file in files)
            {
                string content = File.ReadAllText(file);

                // Elements
                IEnumerable<Match> elementMatches = ElementRegex.Matches(content).Cast<Match>();
                _elements[file] = elementMatches.Select(m => m.Groups["name"].Value).ToArray();

                // Attributes
                IEnumerable<Match> attributeMatches = AttributeRegex.Matches(content).Cast<Match>();
                _attributes[file] = attributeMatches.Select(m => m.Groups["name"].Value).ToArray();
            }
        }

        public static List<string> GetValues(DirectiveType type)
        {
            var names = new List<string>();
            ConcurrentDictionary<string, string[]> cache = type == DirectiveType.Element ? _elements : _attributes;

            foreach (string file in cache.Keys)
            {
                foreach (string attr in cache[file])
                {
                    if (!names.Contains(attr))
                    {
                        names.Add(attr);
                    }
                }
            }

            return names;
        }

        private static List<string> GetFiles(string path, string pattern)
        {
            var files = new List<string>();

            if (path.Contains("node_modules"))
            {
                return files;
            }

            try
            {
                files.AddRange(Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly));
                foreach (string directory in Directory.GetDirectories(path))
                {
                    files.AddRange(GetFiles(directory, pattern));
                }
            }
            catch { }

            return files;
        }
    }

    public enum DirectiveType
    {
        Element,
        Attribute
    }
}
