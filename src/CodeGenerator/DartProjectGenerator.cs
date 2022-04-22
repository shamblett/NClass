﻿// NClass - Free class diagram editor
// Copyright (C) 2006-2009 Balazs Tihanyi
// Copyright (C) 2016-2018 Georgi Baychev
// 
// This program is free software; you can redistribute it and/or modify it under 
// the terms of the GNU General Public License as published by the Free Software 
// Foundation; either version 3 of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT 
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with 
// this program; if not, write to the Free Software Foundation, Inc., 
// 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.IO;
using System.Windows.Forms;
using NClass.Core;
using System.Text.RegularExpressions;
using NClass.Core.Models;

namespace NClass.CodeGenerator
{
    internal sealed class DartProjectGenerator : ProjectGenerator
    {
        SolutionType solutionType;

        /// <exception cref="ArgumentNullException">
        /// <paramref name="model"/> is null.
        /// </exception>
        public DartProjectGenerator(ClassModel model, SolutionType solutionType) : base(model)
        {
            this.solutionType = solutionType;
        }

        public override string RelativeProjectFileName
        {
            get
            {
                string fileName = ProjectName + ".csproj";
                string directoryName = ProjectName;

                return Path.Combine(directoryName, fileName);
            }
        }

        protected override SourceFileGenerator CreateSourceFileGenerator(TypeBase type)
        {
            return new DartSourceFileGenerator(type, RootNamespace);
        }

        protected override bool GenerateProjectFiles(string location)
        {
            try
            {
                string templateDir = Path.Combine(Application.StartupPath, "Templates");
                string templateFile = Path.Combine(templateDir, "csproj.template");
                string projectFile = Path.Combine(location, RelativeProjectFileName);

                using (StreamReader reader = new StreamReader(templateFile))
                using (StreamWriter writer = new StreamWriter(
                    projectFile, false, reader.CurrentEncoding))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        line = line.Replace("${RootNamespace}", RootNamespace);
                        line = line.Replace("${AssemblyName}", ProjectName);

                        if (line.Contains("${VS2017:"))
                        {
                            if (solutionType == SolutionType.VisualStudio2017)
                                line = Regex.Replace(line, @"\${VS2017:(?<content>.+?)}", "${content}");
                            else
                                line = Regex.Replace(line, @"\${VS2017:(?<content>.+?)}", "");

                            if (line.Length == 0)
                                continue;
                        }
                        if (line.Contains("${VS2019:"))
                        {
                            if (solutionType == SolutionType.VisualStudio2019)
                                line = Regex.Replace(line, @"\${VS2019:(?<content>.+?)}", "${content}");
                            else
                                line = Regex.Replace(line, @"\${VS2019:(?<content>.+?)}", "");

                            if (line.Length == 0)
                                continue;
                        }

                        if (line.Contains("${SourceFile}"))
                        {
                            foreach (string fileName in FileNames)
                            {
                                string newLine = line.Replace("${SourceFile}", fileName);
                                writer.WriteLine(newLine);
                            }
                        }
                        else
                        {
                            writer.WriteLine(line);
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
