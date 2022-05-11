// NClass - Free class diagram editor
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

        private struct Condition
        {
           public Condition(String declaration, bool isPrivate)
            {
                this.declaration = declaration;
                this.isPrivate = isPrivate;
            }
            public readonly string declaration;
            public readonly bool isPrivate;
        };


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

        private Condition ConditionClassDeclaration(Condition condition)
        {
            var outString = condition.declaration;
            if (condition.isPrivate)
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

            return new Condition(outString, condition.isPrivate);
        }

        private Condition ConditionFieldDeclaration(Condition condition)
        {
            var outString = condition.declaration;
            if (condition.isPrivate)
            {
                string lastWord = outString.Substring(outString.LastIndexOf(' ') + 1);
                outString = outString.Replace(lastWord, "");
                lastWord = "_" + lastWord;
                outString += lastWord;
            }

            return new Condition(outString, condition.isPrivate);
        }

        private Condition ConditionOperationDeclaration(Condition condition)
        {
            var outString = condition.declaration;
            if (condition.isPrivate)
            {
                string operationName = outString.Substring(outString.IndexOf(' ') + 1);
                outString = outString.Replace(operationName, "");
                operationName = "_" + operationName;
                outString += operationName;
            }

            return new Condition(outString, condition.isPrivate);
        }

        private Condition ConditionDeclaration(string declaration)
        {
            var outString = declaration;

            // Collapse multiple spaces into a single space
            outString = string.Join(" ", outString.Split(new char[] { ' ' }, 
                StringSplitOptions.RemoveEmptyEntries));

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

            return new Condition(outString, isPrivate);
        }


        private void WriteCompositeType(CompositeType type)
        {
            // Pre condition the declaration
            var conditioned = ConditionDeclaration(type.GetDeclaration());
           

            if (type is ClassType)
            {
                var condition = ConditionClassDeclaration(conditioned);
                WriteLine(condition.declaration);
                WriteLine("{");
                IndentLevel++;

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


        private void WriteField(Field field)
        {
            var condition = ConditionDeclaration(field.GetDeclaration());
            WriteLine(ConditionFieldDeclaration(condition).declaration);
        }

        private void WriteOperation(Operation operation)
        {
            
            if (operation is Property)
            {
                WriteProperty((Property) operation);
            }
            else if (operation.HasBody)
            {
                var condition = ConditionDeclaration(operation.GetDeclaration());
                WriteLine(ConditionOperationDeclaration(condition).declaration);
                WriteLine("{");
                IndentLevel++;
                WriteNotImplementedString();
                IndentLevel--;
                WriteLine("}");
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
                WriteLine("throw new UnimplementedError();");
            }
            else
            {
                AddBlankLine(true);
            }
        }
    }
}
