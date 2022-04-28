﻿// NClass - Free class diagram editor
// Copyright (C) 2006-2009 Balazs Tihanyi
// Copyright (C) 2020 Georgi Baychev

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
using System.Collections.Specialized;
using NClass.Core;
using NClass.Dart;

namespace NClass.CodeGenerator
{
    internal sealed class DartSourceFileGenerator : SourceFileGenerator
    {
        /// <exception cref="NullReferenceException">
        /// <paramref name="type"/> is null.
        /// </exception>
        public DartSourceFileGenerator(TypeBase type, string rootNamespace)
            : base(type, rootNamespace)
        {
        }

        protected override string Extension
        {
            get { return ".dart"; }
        }

        protected override void WriteFileContent()
        {
            WriteUsings();

            WriteType(Type);

        }

        private void WriteUsings()
        {
            StringCollection importList = Settings.Default.DartImportList;
            foreach (string usingElement in importList)
                WriteLine(usingElement);

            if (importList.Count > 0)
                AddBlankLine();
        }


        private void WriteType(TypeBase type)
        {
            if (type is CompositeType)
                WriteCompositeType((CompositeType) type);
            else if (type is EnumType)
                WriteEnum((EnumType) type);
            
        }

        private string ConditionClassDeclaration(string declaration, bool isPrivate)
        {
            var outString = declaration;
            if (isPrivate)
            {
                outString = outString.Replace("class ", "class _");
            }

            // Mixin
            if (outString.Contains("mixin"))
            {

                outString = outString.Replace("mixin class", "mixin");

            }

            // Static
            if (outString.Contains("static"))
            {

                outString = outString.Replace("static", "/* static */");

            }

            return outString;
        }

        private string ConditionDeclaration(string declaration)
        {
            var outString = declaration;
            
            // Public/Private
            var isPrivate = false;
            if (outString.StartsWith("public "))
            {
                outString = outString.Replace("public ", "");
            }

            if (outString.StartsWith("private "))
            {
                outString = outString.Replace("private ", "");
                isPrivate = true;
            }

            // Classes
            if (outString.Contains("class"))
            {
                return ConditionClassDeclaration(outString, isPrivate);
            }
            
            return outString;
        }

        private void WriteCompositeType(CompositeType type)
        {
            // Writing type declaration
            var declaration = ConditionDeclaration(type.GetDeclaration());
            WriteLine(declaration);
            WriteLine("{");
            IndentLevel++;

            if (type is ClassType)
            {
                foreach (TypeBase nestedType in ((ClassType) type).NestedChilds)
                {
                    WriteType(nestedType);
                    AddBlankLine();
                }
            }

            if (type.SupportsFields)
            {
                foreach (Field field in type.Fields)
                    WriteField(field);
            }

            bool needBlankLine = (type.FieldCount > 0 && type.OperationCount > 0);

            foreach (Operation operation in type.Operations)
            {
                if (needBlankLine)
                    AddBlankLine();
                needBlankLine = true;

                WriteOperation(operation);
            }

            // Writing closing bracket of the type block
            IndentLevel--;
            WriteLine("}");
        }

        private void WriteEnum(EnumType _enum)
        {
            // Writing type declaration
            WriteLine(_enum.GetDeclaration());
            WriteLine("{");
            IndentLevel++;

            int valuesRemained = _enum.ValueCount;
            foreach (EnumValue value in _enum.Values)
            {
                if (--valuesRemained > 0)
                    WriteLine(value.GetDeclaration() + ",");
                else
                    WriteLine(value.GetDeclaration());
            }

            // Writing closing bracket of the type block
            IndentLevel--;
            WriteLine("}");
        }

        private void WriteDelegate(DelegateType _delegate)
        {
            WriteLine(_delegate.GetDeclaration());
        }

        private void WriteField(Field field)
        {
            WriteLine(field.GetDeclaration());
        }

        private void WriteOperation(Operation operation)
        {
            WriteLine(operation.GetDeclaration());

            if (operation is Property)
            {
                WriteProperty((Property) operation);
            }
            else if (operation.HasBody)
            {
                if (operation is Event)
                {
                    WriteLine("{");
                    IndentLevel++;
                    WriteLine("add {  }");
                    WriteLine("remove {  }");
                    IndentLevel--;
                    WriteLine("}");
                }
                else
                {
                    WriteLine("{");
                    IndentLevel++;
                    WriteNotImplementedString();
                    IndentLevel--;
                    WriteLine("}");
                }
            }
        }

        private void WriteProperty(Property property)
        {
            WriteLine("{");
            IndentLevel++;

            if (!property.IsWriteonly)
            {
                if (property.HasImplementation)
                {
                    WriteLine("get");
                    WriteLine("{");
                    IndentLevel++;
                    WriteNotImplementedString();
                    IndentLevel--;
                    WriteLine("}");
                }
                else
                {
                    WriteLine("get;");
                }
            }
            if (!property.IsReadonly)
            {
                if (property.HasImplementation)
                {
                    WriteLine("set");
                    WriteLine("{");
                    IndentLevel++;
                    WriteNotImplementedString();
                    IndentLevel--;
                    WriteLine("}");
                }
                else
                {
                    WriteLine("set;");
                }
            }

            IndentLevel--;
            WriteLine("}");
        }

        private void WriteNotImplementedString()
        {
            if (Settings.Default.UseNotImplementedExceptions)
            {
                if (Settings.Default.CSharpImportList.Contains("System"))
                    WriteLine("throw new NotImplementedException();");
                else
                    WriteLine("throw new System.NotImplementedException();");
            }
            else
            {
                AddBlankLine(true);
            }
        }
    }
}
