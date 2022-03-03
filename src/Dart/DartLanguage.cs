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
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NClass.Core;
using NClass.Translations;

namespace NClass.Dart
{
    public sealed class DartLanguage : OopLanguage
    {
        static DartLanguage instance = new DartLanguage();

        #region Regex patterns

        internal const string InitialChar = @"[@\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Pc}\p{Lm}]";
        internal const string DeclarationEnding = @"\s*(;\s*)?$";

        // System.String
        private const string TypeNamePattern = InitialChar + @"(\w|\." + InitialChar + @")*";

        // [,][][,,]
        private const string ArrayPattern = @"(\s*\[\s*(,\s*)*\])*";

        // System.String[]
        internal const string BaseTypePattern = TypeNamePattern + @"\??" + ArrayPattern;

        // <System.String[], System.String[]>
        private const string GenericPattern =
            @"<\s*" + BaseTypePattern + @"(\s*,\s*" + BaseTypePattern + @")*\s*>";
        
        // System.Collections.Generic.List<System.String[], System.String[]>[]
        internal const string GenericTypePattern =
            TypeNamePattern + @"(\s*" + GenericPattern + @")?\??" + ArrayPattern;
        
        // <List<int>[], List<string>>
        private const string GenericPattern2 =
            @"<\s*" + GenericTypePattern + @"(\s*,\s*" + GenericTypePattern + @")*\s*>";
        
        // System.Collections.Generic.List<List<int>[]>[]
        internal const string GenericTypePattern2 =
            TypeNamePattern + @"(\s*" + GenericPattern2 + @")?\??" + ArrayPattern;


        // Name
        internal const string NamePattern = InitialChar + @"\w*";
        
        // <T, K>
        private const string BaseGenericPattern =
            @"<\s*" + NamePattern + @"(\s*,\s*" + NamePattern + @")*\s*>";
        
        // Name<T>
        internal const string GenericNamePattern =
            NamePattern + @"(\s*" + BaseGenericPattern + ")?";

        // Interface.Method
        private const string OperationNamePattern =
            "(" + GenericTypePattern + @"(?<namedot>\.))?" + NamePattern;

        // Interface.Method<T>
        internal const string GenericOperationNamePattern =
            OperationNamePattern + @"(\s*" + BaseGenericPattern + ")?";


        // [static | virtual | abstract | override | sealed | new]
        internal const string OperationModifiersPattern =
            @"((?<modifier>static|virtual|abstract|override|sealed|new)\s+)*";

        // [public | protected internal | internal protected | internal | protected | private]
        internal const string AccessPattern = @"((?<access>public|protected\s+internal|" +
            @"internal\s+protected|internal|protected|private)\s+)*";

        // For validating identifier names.
        private const string ClosedNamePattern = @"^\s*(?<name>" + NamePattern + @")\s*$";

        // For validating generic identifier names.
        private const string ClosedGenericNamePattern = @"^\s*(?<name>" + GenericNamePattern + @")\s*$";

        // For validating type names.
        private const string ClosedTypePattern = @"^\s*(?<type>" + GenericTypePattern2 + @")\s*$";

        #endregion

        static Regex nameRegex = new Regex(ClosedNamePattern, RegexOptions.ExplicitCapture);
        static Regex genericNameRegex = new Regex(ClosedGenericNamePattern, RegexOptions.ExplicitCapture);
        static Regex typeRegex = new Regex(ClosedTypePattern, RegexOptions.ExplicitCapture);

        static readonly string[] reservedNames = {
            "assert", "break", "case", "catch", "class", "const", "continue", "default", "do", "else", "enum", "extends", "false", "final", "finally", "for", "if", "in", "is", "new", "null", "rethrow", "return", "super", "switch", 
            "this", "throw", "true", "try", "var", "void", "while", "with",  "abstract", "as", "covariant", "deferred", "dynamic", "export", "extension", "external", "factory", "function", "get", "implements", "import", 
            "interface", "library", "mixin", "operator", "part", "set", "static", "typedef"
    };
        static readonly string[] typeKeywords = {
            "bool", "double", "int", "Object", "String", "void"
        };
        static readonly Dictionary<AccessModifier, string> validAccessModifiers;
        static readonly Dictionary<ClassModifier, string> validClassModifiers;
        static readonly Dictionary<FieldModifier, string> validFieldModifiers;
        static readonly Dictionary<OperationModifier, string> validOperationModifiers;

        static DartClass objectClass;

        static DartLanguage()
        {
            // objectClass initialization
            string[] objectMethods = {
                "bool operator==(object other)",
                "int get hashCode",
                "String get runTimeType",
                "dynamic noSuchMethod(Invocation invocation)",
                "String toString()"
            };
            objectClass = new DartClass("Object");
            objectClass.AddConstructor();
            foreach (string methodDeclaration in objectMethods)
                objectClass.AddMethod().InitFromString(methodDeclaration);

            // validAccessModifiers initialization
            validAccessModifiers = new Dictionary<AccessModifier, string>(6);
            validAccessModifiers.Add(AccessModifier.Private, "_");

            // validClassModifiers initialization
            validClassModifiers = new Dictionary<ClassModifier, string>(3);
            validClassModifiers.Add(ClassModifier.Abstract, "abstract");
            validClassModifiers.Add(ClassModifier.Static, "static");

            // validFieldModifiers initialization
            validFieldModifiers = new Dictionary<FieldModifier, string>(5);
            validFieldModifiers.Add(FieldModifier.Static, "static");


            // validOperationModifiers initialization
            validOperationModifiers = new Dictionary<OperationModifier, string>(8);
            validOperationModifiers.Add(OperationModifier.Static, "static");
            validOperationModifiers.Add(OperationModifier.Abstract, "abstract");
            validOperationModifiers.Add(OperationModifier.Override, "override");
        }

        private DartLanguage()
        {
        }

        public static DartLanguage Instance
        {
            get { return instance; }
        }

        internal static DartClass ObjectClass
        {
            get { return objectClass; }
        }

        public override string Name
        {
            get { return "Dart"; }
        }

        public override string AssemblyName
        {
            get { return "Dart"; }
        }

        [XmlIgnore]
        public override Dictionary<AccessModifier, string> ValidAccessModifiers
        {
            get { return validAccessModifiers; }
        }

        [XmlIgnore]
        public override Dictionary<ClassModifier, string> ValidClassModifiers
        {
            get { return validClassModifiers; }
        }

        [XmlIgnore]
        public override Dictionary<FieldModifier, string> ValidFieldModifiers
        {
            get { return validFieldModifiers; }
        }

        [XmlIgnore]
        public override Dictionary<OperationModifier, string> ValidOperationModifiers
        {
            get { return validOperationModifiers; }
        }

        protected override string[] ReservedNames
        {
            get { return reservedNames; }
        }

        protected override string[] TypeKeywords
        {
            get { return typeKeywords; }
        }

        public override string DefaultFileExtension
        {
            get { return ".dart"; }
        }

        public override bool IsValidModifier(AccessModifier modifier)
        {
            return true;
        }

        public override bool IsValidModifier(FieldModifier modifier)
        {
            return true;
        }

        public override bool IsValidModifier(OperationModifier modifier)
        {
            return true;
        }

        /// <exception cref="BadSyntaxException">
        /// The <paramref name="operation"/> contains invalid modifier combinations.
        /// </exception>
        protected override void ValidateOperation(Operation operation)
        {
            ValidateAccessModifiers(operation);
            ValidateOperationModifiers(operation);
        }

        private static void ValidateOperationModifiers(Operation operation)
        {
            if (operation.IsStatic) {
                if (operation.IsVirtual) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "virtual", "static"));
                }
                if (operation.IsAbstract) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "abstract", "static"));
                }
                if (operation.IsOverride) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "override", "static"));
                }
                if (operation.IsSealed) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "sealed", "static"));
                }
            }

            if (operation.IsVirtual) {
                if (operation.IsAbstract) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "abstract", "virtual"));
                }
                if (operation.IsOverride) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "override", "virtual"));
                }
                if (operation.IsSealed) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "sealed", "virtual"));
                }
            }

            if (operation.IsHider) {
                if (operation.IsOverride) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "new", "override"));
                }
                if (operation.IsSealed) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "new", "sealed"));
                }
            }

            if (operation.IsSealed) {
                if (operation.IsAbstract) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "sealed", "abstract"));
                }
                if (!operation.IsOverride)
                    operation.IsOverride = true;
            }

            if (operation.IsAbstract) {
                ClassType parent = operation.Parent as ClassType;
                if (parent == null)
                    throw new BadSyntaxException(Strings.ErrorInvalidModifier);
                else
                    parent.Modifier = ClassModifier.Abstract;
            }
        }

        private static void ValidateAccessModifiers(Operation operation)
        {
            if (operation.AccessModifier != AccessModifier.Default &&
                operation.Parent is InterfaceType)
            {
                throw new BadSyntaxException(
                    Strings.ErrorInterfaceMemberAccess);
            }

            if (operation.Access == AccessModifier.Private) {
                if (operation.IsVirtual) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "private", "virtual"));
                }
                if (operation.IsAbstract) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "private", "abstract"));
                }
                if (operation.IsOverride) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "private", "override"));
                }
                if (operation.IsSealed) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "private", "sealed"));
                }
            }
        }

        /// <exception cref="BadSyntaxException">
        /// The <paramref name="field"/> contains invalid modifier combinations.
        /// </exception>
        protected override void ValidateField(Field field)
        {
            if (field.IsConstant) {
                if (field.IsStatic) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "static", "const"));
                }
                if (field.IsReadonly) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "readonly", "const"));
                }
                if (field.IsVolatile) {
                    throw new BadSyntaxException(string.Format(
                        Strings.ErrorInvalidModifierCombination, "volatile", "const"));
                }
            }
            if (field.IsReadonly && field.IsVolatile) {
                throw new BadSyntaxException(string.Format(
                    Strings.ErrorInvalidModifierCombination, "volatile", "readonly"));
            }
        }

        /// <exception cref="ArgumentException">
        /// The language does not support explicit interface implementation.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="operation"/> is null.-or-
        /// <paramref name="newParent"/> is null.
        /// </exception>
        protected override Operation Implement(Operation operation,
            CompositeType newParent, bool explicitly)
        {
            if (newParent == null)
                throw new ArgumentNullException("newParent");
            if (operation == null)
                throw new ArgumentNullException("operation");

            Operation newOperation = operation.Clone(newParent);

            newOperation.AccessModifier = AccessModifier.Public;
            newOperation.ClearModifiers();
            newOperation.IsStatic = false;

            if (explicitly) {
                newOperation.Name = string.Format("{0}.{1}",
                    ((InterfaceType) operation.Parent).Name, newOperation.Name);
            }

            return newOperation;
        }

        /// <exception cref="ArgumentException">
        /// <paramref name="operation"/> cannot be overridden.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="operation"/> is null.
        /// </exception>
        protected override Operation Override(Operation operation, CompositeType newParent)
        {
            if (operation == null)
                throw new ArgumentNullException("operation");

            if (!operation.IsVirtual && !operation.IsAbstract && !operation.IsOverride ||
                operation.IsSealed)
            {
                throw new ArgumentException(
                    Strings.ErrorCannotOverride, "operation");
            }

            Operation newOperation = operation.Clone(newParent);
            newOperation.IsVirtual = false;
            newOperation.IsAbstract = false;
            newOperation.IsOverride = true;

            return newOperation;
        }

        /// <exception cref="BadSyntaxException">
        /// The <paramref name="name"/> does not fit to the syntax.
        /// </exception>
        public override string GetValidName(string name, bool isGenericName)
        {
            Match match = (isGenericName ? genericNameRegex.Match(name) : nameRegex.Match(name));

            if (match.Success) {
                string validName = match.Groups["name"].Value;
                return base.GetValidName(validName, isGenericName);
            }
            else {
                throw new BadSyntaxException(Strings.ErrorInvalidName);
            }            
        }

        /// <exception cref="BadSyntaxException">
        /// The <paramref name="name"/> does not fit to the syntax.
        /// </exception>
        public override string GetValidTypeName(string name)
        {
            Match match = typeRegex.Match(name);

            if (match.Success) {
                string validName = match.Groups["type"].Value;
                return base.GetValidTypeName(validName);
            }
            else {
                throw new BadSyntaxException(Strings.ErrorInvalidTypeName);
            }
        }

        public override AccessModifier TryParseAccessModifier(string value)
        {
            if (value.Contains("protected") && value.Contains("internal"))
                return AccessModifier.ProtectedInternal;
            else
                return base.TryParseAccessModifier(value);
        }

        public override OperationModifier TryParseOperationModifier(string value)
        {
            if (value.Contains("sealed"))
                return OperationModifier.Sealed;
            else
                return base.TryParseOperationModifier(value);
        }

        public override string GetAccessString(AccessModifier access, bool forCode)
        {
            switch (access) {
                case AccessModifier.ProtectedInternal:
                    if (forCode)
                        return "protected internal";
                    else
                        return "Protected Internal";

                case AccessModifier.Default:
                    if (forCode)
                        return "";
                    else
                        return "Default";

                default:
                    if (forCode)
                        return access.ToString().ToLower();
                    else
                        return access.ToString();
            }
        }

        public override string GetFieldModifierString(FieldModifier modifier, bool forCode)
        {
            if (modifier == FieldModifier.None) {
                if (forCode)
                    return "";
                else
                    return "None";
            }

            StringBuilder builder = new StringBuilder(30);
            if ((modifier & FieldModifier.Hider) != 0)
                builder.Append(forCode ? "new " : "New, ");
            if ((modifier & FieldModifier.Constant) != 0)
                builder.Append(forCode ? "const " : "Const, ");
            if ((modifier & FieldModifier.Static) != 0)
                builder.Append(forCode ? "static " : "Static, ");
            if ((modifier & FieldModifier.Readonly) != 0)
                builder.Append(forCode ? "readonly " : "Readonly, ");
            if ((modifier & FieldModifier.Volatile) != 0)
                builder.Append(forCode ? "volatile " : "Volatile, ");

            if (forCode)
                builder.Remove(builder.Length - 1, 1);
            else
                builder.Remove(builder.Length - 2, 2);

            return builder.ToString();
        }

        public override string GetOperationModifierString(OperationModifier modifier, bool forCode)
        {
            if (modifier == OperationModifier.None) {
                if (forCode)
                    return "";
                else
                    return "None";
            }

            StringBuilder builder = new StringBuilder(30);
            if ((modifier & OperationModifier.Hider) != 0)
                builder.Append(forCode ? "new " : "New, ");
            if ((modifier & OperationModifier.Static) != 0)
                builder.Append(forCode ? "static " : "Static, ");
            if ((modifier & OperationModifier.Virtual) != 0)
                builder.Append(forCode ? "virtual " : "Virtual, ");
            if ((modifier & OperationModifier.Abstract) != 0)
                builder.Append(forCode ? "abstract " : "Abstract, ");
            if ((modifier & OperationModifier.Sealed) != 0)
                builder.Append(forCode ? "sealed " : "Sealed, ");
            if ((modifier & OperationModifier.Override) != 0)
                builder.Append(forCode ? "override " : "Override, ");

            if (forCode)
                builder.Remove(builder.Length - 1, 1);
            else
                builder.Remove(builder.Length - 2, 2);

            return builder.ToString();
        }

        public override string GetClassModifierString(ClassModifier modifier, bool forCode)
        {
            if (!forCode)
                return modifier.ToString();
            else if (modifier == ClassModifier.None)
                return "";
            else
                return modifier.ToString().ToLower();
        }

        protected override Package CreatePackage()
        {
            return new DartNamespace();
        }

        protected override ClassType CreateClass()
        {
            return new DartClass();
        }

        protected override StructureType CreateStructure()
        {
            return new DartStructure();
        }

        protected override InterfaceType CreateInterface()
        {
            return new DartInterface();
        }

        protected override EnumType CreateEnum()
        {
            return new DartEnum();
        }

        protected override DelegateType CreateDelegate()
        {
            return new DartDelegate();
        }

        protected override ArgumentList CreateParameterCollection()
        {
            return new DartArgumentList();
        }
    }
}
