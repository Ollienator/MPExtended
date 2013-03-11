﻿#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Linq;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.Development.DevTool.WixFS
{
    internal abstract class WixFSGenerator
    {
        protected string[] forbiddenFiles;
        protected string[] forbiddenExtensions;
        protected string[] forbiddenDirectories;

        protected void RunFromInput(string inputDir, string outputFile, string name, string parentName, bool skipImmediate)
        {
            // setup XML file
            XNamespace ns = "http://schemas.microsoft.com/wix/2006/wi";
            XElement componentGroup = new XElement(ns + "ComponentGroup", new XAttribute("Id", "Component_" + name));
            XElement directoryRef = new XElement(ns + "DirectoryRef", new XAttribute("Id", parentName));

            // build content
            AddDirectory(inputDir, "Component_Generated_" + name, "Dir_Generated_" + name, ns, directoryRef, componentGroup, skipImmediate);

            // save
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", ""),
                new XElement(ns + "Wix",
                    new XAttribute("xmlns", ns.NamespaceName),
                    new XComment(String.Format(
                        "Autogenerated at {0} for directory {1}",
                        DateTime.Now.ToString("dd MMM yyy HH:mm", System.Globalization.CultureInfo.InvariantCulture),
                        inputDir
                    )),
                    new XElement(ns + "Fragment", componentGroup),
                    new XElement(ns + "Fragment", directoryRef)
                )
            );
            doc.Save(outputFile);
        }

        private bool AddDirectory(string path, string baseComponent, string basePrefix, XNamespace ns, XElement dirNode, XElement componentGroup, bool skipImmediate = false)
        {
            bool didAdd = false;
            string fullPath = Path.Combine(Installation.GetSourceRootDirectory(), path);
            var files = Directory.GetFiles(fullPath)
                .Select(x => Path.GetFileName(x))
                .Where(x => !forbiddenExtensions.Contains(Path.GetExtension(x)) && !forbiddenFiles.Contains(x));
            if (files.Count() > 0 && !skipImmediate)
            {
                // create stable GUIDs
                MD5 provider = MD5.Create();
                byte[] bytes = provider.ComputeHash(Encoding.UTF8.GetBytes("MPExtended_Hash_Guid_" + baseComponent));
                int byteIndex = BitConverter.IsLittleEndian ? 7 : 1;
                bytes[byteIndex] = (byte)(0x40 | (0x0F & bytes[byteIndex])); // pretend this are random GUIDs

                XElement filesComponent = new XElement(ns + "Component",
                    new XAttribute("Id", baseComponent),
                    new XAttribute("Guid", new Guid(bytes).ToString("D"))
                );

                foreach (var file in files)
                {
                    string id = Regex.Replace(path + "_" + file, "[^a-zA-Z0-9_]+", "_", RegexOptions.Compiled);
                    filesComponent.Add(new XElement(ns + "File",
                        new XAttribute("Source", @"$(var.SolutionDir)\" + path + @"\" + file),
                        new XAttribute("Id", "FileGen_" + (id.Length > 60 ? id.Substring(id.Length - 60) : id))
                    ));
                }

                dirNode.Add(filesComponent);
                componentGroup.Add(new XElement(ns + "ComponentRef", new XAttribute("Id", baseComponent)));
                didAdd = true;
            }

            var dirs = Directory.GetDirectories(fullPath)
                .Select(x => Path.GetFileName(x))
                .Where(x => !forbiddenDirectories.Contains(x));
            if (dirs.Count() > 0)
            {
                foreach(var dir in dirs)
                {
                    XElement xmlDir = new XElement(ns + "Directory",
                        new XAttribute("Id", basePrefix + "_" + dir),
                        new XAttribute("Name", dir)
                    );
                    if (AddDirectory(Path.Combine(path, dir), baseComponent + "_" + dir, basePrefix + "_" + dir, ns, xmlDir, componentGroup))
                    {
                        didAdd = true;
                        dirNode.Add(xmlDir);
                    }
                }
            }

            return didAdd;
        }
    }
}
